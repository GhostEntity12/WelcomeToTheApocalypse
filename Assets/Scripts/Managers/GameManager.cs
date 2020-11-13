using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum TargetingState
{
	// Player is currently selecting a node for movement.
	Move,

	// Player is currently selecting a node for using a skill.
	Skill,

	// Player isn't selecting a node for anything. (just to be safe)
	None
}

public class GameManager : MonoBehaviour
{
	/// <summary>
	/// Instance of the game manager.
	///</summary>
	public static GameManager m_Instance = null;

	/// <summary>
	/// Unit the player has selected.
	/// </summary>
	public Unit m_SelectedUnit = null;

	/// <summary>
	/// Raycast for translating the mouse's screen position to world position.
	/// </summary>
	private RaycastHit m_MouseWorldRayHit = new RaycastHit();

	/// <summary>
	/// Ray for raycasting the mouse's position.
	/// </summary>
	private Ray m_MouseRay = new Ray();

	/// <summary>
	/// Reference to the main camera.
	/// </summary>
	private Camera m_MainCamera = null;

	/// <summary>
	/// The action the player is targeting for.
	/// </summary>
	private TargetingState m_TargetingState = TargetingState.Move;

	/// <summary>
	/// The skill the player is targeting for.
	/// </summary>
	private BaseSkill m_SelectedSkill = null;

	/// <summary>
	/// Hotkeys for the abilities the player can activate.
	/// </summary>
	private readonly KeyCode[] m_AbilityHotkeys = new KeyCode[3] { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3 };

	/// <summary>
	/// Which team's turn is it currently.
	/// </summary>
	public Allegiance m_TeamCurrentTurn = Allegiance.Player;

	/// <summary>
	/// The cost of the player's unit moving to their target location.
	/// </summary>
	private int m_MovementCost = 0;

	/// <summary>
	/// If the left mouse button is down.
	/// </summary>
	private bool m_LeftMouseDown = false;

	/// <summary>
	/// The node the mouse is hovering over.
	/// </summary>
	private Node m_CachedNode;

	/// <summary>
	/// The max range of a skill on the pathfinding grid.
	/// </summary>
	[HideInInspector]
	public List<Node> m_maxSkillRange = new List<Node>();

	List<Node> m_ClearRange = new List<Node>();

	List<Unit> m_AffectedUnits = new List<Unit>();

	/// <summary>
	/// Is the mouse hovering over a UI element that will block the player's inputs?
	/// </summary>
	public bool m_MouseOverUIBlockingElements = false;

	private CameraMovement m_CameraMovement;

	public int m_PodClearBonus = 5;

	public bool m_DidHealthBonus;

	public TextAsset m_FailScript;
	public TextAsset m_WinScript;

	[FMODUnity.EventRef]
	public string m_TurnEndSound = "";

	// On startup.
	private void Awake()
	{
		m_MainCamera = Camera.main;

		m_MouseRay.origin = m_MainCamera.transform.position;

		m_Instance = this;

		CreateVersionText();

		if (!FindObjectOfType<MusicManager>())
		{
			GameObject musicManager = new GameObject("MusicManager", typeof(MusicManager));
			musicManager.GetComponent<MusicManager>().m_MusicEvent = "event:/Music";
		}
	}

	private void Start()
	{
		m_CameraMovement = m_MainCamera.GetComponentInParent<CameraMovement>();
	}

	// Update.
	private void Update()
	{
		// If it's currently the player's turn, check their inputs.
		// Commented out for debugging.
		if (m_TeamCurrentTurn == Allegiance.Player)
		{
			if (!UIManager.m_Instance.m_ActiveUI)
			{
				PlayerInputs();
			}
		}

		Debug.DrawLine(m_MainCamera.transform.position, m_MouseWorldRayHit.point);
	}

	/// <summary>
	/// Get the unit currently selected by the player.
	/// </summary>
	/// <returns> The unit the player has selected. </returns>
	public Unit GetSelectedUnit() { return m_SelectedUnit; }

	public BaseSkill GetSelectedSkill() { return m_SelectedSkill; }

	public void TryEndTurn()
	{
		// Check player units for prematurely ending turn here.
		if (UIManager.m_Instance.IsPrematureTurnEnding())
		{
			UIManager.m_Instance.m_PrematureTurnEndScreen.DisplayPrematureEndScreen(true);
			return;
		}

		EndCurrentTurn();
	}

	/// <summary>
	/// End the current turn.
	/// </summary>
	public void EndCurrentTurn()
	{
		if (m_TeamCurrentTurn == Allegiance.Enemy)
			m_TeamCurrentTurn = Allegiance.Player;
		else if (m_TeamCurrentTurn == Allegiance.Player)
			m_TeamCurrentTurn = Allegiance.Enemy;

		Debug.Log($"============{m_TeamCurrentTurn} turn============");

		UIManager.m_Instance.SwapTurnIndicator(m_TeamCurrentTurn);

		UIManager.m_Instance.SlideSkills(UIManager.ScreenState.Offscreen);

		m_TargetingState = TargetingState.None;

		// Play the end turn sound on the camera.
		FMODUnity.RuntimeManager.PlayOneShot(m_TurnEndSound, Camera.main.transform.position);

		// Remove all the highlights
		if (m_SelectedUnit)
		{
			foreach (Node n in m_SelectedUnit.m_MovableNodes)
			{
				n.m_NodeHighlight.ChangeHighlight(TileState.None);
			}
		}
		foreach (Node n in m_maxSkillRange)
		{
			n.m_NodeHighlight.ChangeHighlight(TileState.None);
		}

		foreach (Unit unit in m_TeamCurrentTurn == Allegiance.Player ? UnitsManager.m_Instance.m_PlayerUnits : UnitsManager.m_Instance.m_ActiveEnemyUnits)
		{
			unit.SetDealExtraDamage(0);
			unit.ResetActionPoints();
			unit.ResetCurrentMovement();

			// Check the passives of all the player units for any that trigger at the start of their turn.
			PassiveSkill ps = unit.GetPassiveSkill();
			if (ps)
			{
				if (ps.CheckPrecondition(TriggerType.OnTurnStart))
					ps.TakeEffect(unit);
			}


			// Reduce cooldowns
			foreach (BaseSkill s in unit.GetSkills())
			{
				s.DecrementCooldown();
			}

			// Deal with inflicted statuses
			// So using .ToList() creates a compy of the list to iterate through
			// but continues romoving from the source list. Not particularly
			// efficient for large lists, but easy enough here.
			foreach (InflictableStatus status in unit.GetInflictableStatuses().ToList())
			{
				// If returns true, status effect's duration has reached 0, remove the status effect.
				if (status.DecrementDuration())
				{
					unit.RemoveStatusEffect(status);
				}
				// Otherwise do the effect
				else if (status.CheckPrecondition(TriggerType.OnTurnStart) == true)
				{
					status.TakeEffect(unit);
				}
			}
		}

		m_SelectedUnit = null;
		AIManager.m_Instance.SetAITurn(m_TeamCurrentTurn == Allegiance.Enemy);
	}

	/// <summary>
	/// Get's the player's inputs.
	/// </summary>
	public void PlayerInputs()
	{
		m_MouseRay = m_MainCamera.ScreenPointToRay(Input.mousePosition);

		m_LeftMouseDown = Input.GetMouseButtonDown(0);

		HotKeys();

		if (UIManager.m_Instance.CheckUIBlocking()) return;

		// Mouse is over a unit.
		if (Physics.Raycast(m_MouseRay, out m_MouseWorldRayHit, Mathf.Infinity, 1 << 9))
		{
			Unit rayHitUnit = m_MouseWorldRayHit.transform.GetComponent<Unit>();

			// Check if the player is selecting another character.
			switch (m_TargetingState)
			{
				case TargetingState.Move:
					if (m_SelectedUnit && !m_SelectedUnit.GetMoving())
					{
						UpdateMoveablePreview(null, Grid.m_Instance.GetNode(m_SelectedUnit.transform.position));
					}
					// Selecting new unit
					if (m_LeftMouseDown)
					{
						// If the unit the player is clicking on isn't the selected unit and the unit is alive, select that unit.
						if (rayHitUnit != m_SelectedUnit && rayHitUnit.GetAlive())
						{
							SelectUnit(rayHitUnit);
							UpdateMoveablePreview(null, Grid.m_Instance.GetNode(m_SelectedUnit.transform.position));
						}
					}
					break;
				case TargetingState.Skill:
					Node hitNode = Grid.m_Instance.GetNode(rayHitUnit.transform.position);
					if (hitNode != m_CachedNode)
						UpdateSkillPreview(hitNode);
					if (m_LeftMouseDown && !m_MouseOverUIBlockingElements && GetCurrentTurn() == Allegiance.Player)
					{
						SkillSelection(m_CachedNode);
					}
					break;
				default:
					break;
			}
		}
		// Mouse is over a tile.
		else if (Physics.Raycast(m_MouseRay, out m_MouseWorldRayHit, Mathf.Infinity, 1 << 8))
		{
			Node hitNode = Grid.m_Instance.GetNode(m_MouseWorldRayHit.transform.position);
			switch (m_TargetingState)
			{
				case TargetingState.Move:
					if (m_SelectedUnit && !m_SelectedUnit.GetMoving())
					{
						// The player is choosing a tile to move a unit to.
						if (m_SelectedUnit.GetAllegiance() == Allegiance.Player &&
							m_SelectedUnit.m_MovableNodes.Contains(hitNode) &&
							m_SelectedUnit.GetCurrentMovement() > 0)
						{
							// On click, make sure a unit is selected.
							if (m_LeftMouseDown)
							{
								if (Grid.m_Instance.FindPath(m_SelectedUnit.transform.position, m_MouseWorldRayHit.transform.position, out Stack<Node> path, out m_MovementCost, true))
								{
									// Set the unit's path
									m_SelectedUnit.SetMovementPath(path);
									m_SelectedUnit.DecreaseCurrentMovement(m_MovementCost);
								}

								m_SelectedUnit.m_MovableNodes = Grid.m_Instance.GetNodesWithinRadius(m_SelectedUnit.GetCurrentMovement(), hitNode);
								// Should we do this after the unit has finished moving? - James L
								UpdateMoveablePreview(null, hitNode);
							}
							else
							{
								UpdateMoveablePreview(hitNode, Grid.m_Instance.GetNode(m_SelectedUnit.transform.position));
							}
						}
						else
						{
							UpdateMoveablePreview(null, Grid.m_Instance.GetNode(m_SelectedUnit.transform.position));
						}
					}
					break;
				case TargetingState.Skill:
					if (hitNode != m_CachedNode)
						UpdateSkillPreview(hitNode);
					if (m_LeftMouseDown && !m_MouseOverUIBlockingElements && GetCurrentTurn() == Allegiance.Player)
					{
						SkillSelection(m_CachedNode);
					}
					break;
				default:
					break;
			}
		}
	}

	void HotKeys()
	{
		// Cancelling skill targeting.
		if (Input.GetMouseButtonDown(1))
		{
			CancelSkill();
		}

		// Selecting a skill with the number keys.
		for (int i = 0; i < m_AbilityHotkeys.Length; i++)
		{
			if (Input.GetKeyDown(m_AbilityHotkeys[i]))
			{
				// Make sure the player can use the skill before selecting it.
				if (m_SelectedUnit.GetSkill(i).GetCurrentCooldown() == 0 && m_SelectedUnit.GetActionPoints() >= m_SelectedUnit.GetSkill(i).m_Cost)
				{
					SkillSelection(i);
					break;
				}
			}
		}

		// Show health bars of player units and active enemies.
		if (Input.GetKey(KeyCode.Tab))
		{
			foreach (Unit u in UnitsManager.m_Instance.m_PlayerUnits)
			{
				u.GetHealthBar().Reset();
			}
			foreach (Unit u in UnitsManager.m_Instance.m_ActiveEnemyUnits)
			{
				u.GetHealthBar().Reset();
			}
		}

		if (Input.GetKeyDown(KeyCode.Space) && GetCurrentTurn() == Allegiance.Player)
		{
			TryEndTurn();
		}
	}

	void SelectUnit(Unit unit)
	{
		// Auto focus
		m_CameraMovement.m_AutoMoveDestination = new Vector3(unit.transform.position.x, 0, unit.transform.position.z);

		// Reset the nodes highlights before selecting the new unit
		m_maxSkillRange.ForEach(s => s.m_NodeHighlight.m_IsInTargetArea = false);
		m_SelectedUnit?.m_MovableNodes.ForEach(u => u.m_NodeHighlight.ChangeHighlight(TileState.None));

		// Store the new unit
		m_SelectedUnit = unit;
		UIManager.m_Instance.SwapSkillsUI(m_SelectedUnit.m_UIData);
		UIManager.m_Instance.m_UIHealthBar.SetHealthAmount((float)m_SelectedUnit.GetCurrentHealth() / m_SelectedUnit.GetStartingHealth());

		// Highlight the appropriate tiles
		m_SelectedUnit.m_MovableNodes = Grid.m_Instance.GetNodesWithinRadius(m_SelectedUnit.GetCurrentMovement(), Grid.m_Instance.GetNode(m_SelectedUnit.transform.position));
		UpdateMoveablePreview(null, Grid.m_Instance.GetNode(m_SelectedUnit.transform.position));

		StatusEffectTooltipManager.m_Instance.UpdateActiveEffects();

		// Update the UI's action point counter to display the newly selected unit's action points.
		UIManager.m_Instance.m_ActionPointCounter.ResetActionPointCounter();
		UIManager.m_Instance.m_ActionPointCounter.UpdateActionPointCounter();

		// Store all the reachable nodes so they can be easily cleared
		m_ClearRange = Grid.m_Instance.GetNodesWithinRadius(m_SelectedUnit.GetCurrentMovement(), Grid.m_Instance.GetNode(m_SelectedUnit.transform.position), true);
	}

	public void RefreshHighlights()
	{
		if (GetSelectedUnit())
		{
			m_ClearRange = Grid.m_Instance.GetNodesWithinRadius(m_SelectedUnit.GetCurrentMovement(), Grid.m_Instance.GetNode(m_SelectedUnit.transform.position), true);
			switch (m_TargetingState)
			{
				case TargetingState.Move:
					UpdateMoveablePreview(null, Grid.m_Instance.GetNode(m_SelectedUnit.transform.position));
					break;
				case TargetingState.Skill:
					UpdateSkillPreview(null);
					break;
				default:
					break;
			}
		}
	}

	void UpdateSkillPreview(Node hitNode)
	{
		if (!m_SelectedSkill) return; 

		List<Node> nodesToClear = m_maxSkillRange.Concat(m_SelectedUnit.m_MovableNodes).ToList();
		List<Node> nodesTargetable = Grid.m_Instance.GetNodesWithinRadius(m_SelectedSkill.m_CastableDistance, Grid.m_Instance.GetNode(m_SelectedUnit.transform.position), true);

		nodesToClear = nodesToClear.Except(nodesTargetable).ToList();
		nodesToClear.ForEach(n => n.m_NodeHighlight.ChangeHighlight(TileState.None));

		if (hitNode != null)
		{
			// Update the cache
			m_CachedNode = hitNode;

			bool canCastOnNode;
			switch (m_SelectedSkill.targetType)
			{
				case TargetType.SingleTarget:
					canCastOnNode = m_CachedNode.unit && IsTargetable(m_SelectedUnit, m_CachedNode.unit, m_SelectedSkill) && nodesTargetable.Contains(m_CachedNode);
					break;
				case TargetType.Terrain:
					canCastOnNode = nodesTargetable.Contains(m_CachedNode);
					break;
				default:
					Debug.LogError("Not Implemented!");
					canCastOnNode = false;
					break;
			}

			if (canCastOnNode)
			{
				List<Node> nodesTargeted = Grid.m_Instance.GetNodesWithinRadius(m_SelectedSkill.m_AffectedRange, m_CachedNode, true);
				nodesTargetable = nodesTargetable.Except(nodesTargeted).ToList();
				nodesTargeted.ForEach(n => n.m_NodeHighlight.ChangeHighlight(TileState.EffectRange));

				// Show/clear heath previews
				List<Unit> newAffectedUnits = nodesTargeted.Where(n => n.unit && IsTargetable(m_SelectedUnit, n.unit, m_SelectedSkill)).Select(n => n.unit).Distinct().ToList();
				foreach (Unit affectedUnit in newAffectedUnits.Except(m_AffectedUnits)) // Only new affected units
				{
					affectedUnit.m_Healthbar.m_KeepFocus = true;

					switch (m_SelectedSkill)
					{
						case DamageSkill ds:
							affectedUnit.m_Healthbar.ChangeFill(((float)affectedUnit.GetCurrentHealth() - (ds.m_DamageAmount + ds.m_ExtraDamage)) / affectedUnit.GetStartingHealth(), false);
							break;
						case HealSkill hs:
							affectedUnit.m_Healthbar.ChangeFill(((float)affectedUnit.GetCurrentHealth() + hs.m_HealAmount) / affectedUnit.GetStartingHealth(), false);
							break;
						default:
							break;
					}
				}

				foreach (Unit unaffectedUnit in m_AffectedUnits.Except(newAffectedUnits)) // No longer affected units
				{
					unaffectedUnit.m_Healthbar.m_KeepFocus = false;
					unaffectedUnit.m_Healthbar.ChangeFill((float)unaffectedUnit.GetCurrentHealth() / unaffectedUnit.GetStartingHealth(), false);
				}

				m_AffectedUnits = newAffectedUnits;
			}
			else
			{
				foreach (Unit unaffectedUnit in m_AffectedUnits) // No longer affected units
				{
					unaffectedUnit.m_Healthbar.m_KeepFocus = false;
					unaffectedUnit.m_Healthbar.ChangeFill((float)unaffectedUnit.GetCurrentHealth() / unaffectedUnit.GetStartingHealth(), false);
				}

				m_AffectedUnits.Clear();
			}
		}

		nodesTargetable.ForEach(n => n.m_NodeHighlight.ChangeHighlight(TileState.TargetRange));
	}

	void UpdateMoveablePreview(Node hitNode, Node centerNode)
	{
		if (hitNode != null && hitNode == m_CachedNode) return;

		List<Node> nodesToClear = m_ClearRange.Concat(m_maxSkillRange).ToList();
		List<Node> nodesWalkable = m_SelectedUnit.m_MovableNodes;

		nodesToClear = nodesToClear.Except(nodesWalkable).ToList();
		nodesToClear.ForEach(n => n.m_NodeHighlight.ChangeHighlight(TileState.None));
		nodesWalkable.ForEach(n => n.m_NodeHighlight.ChangeHighlight(TileState.MovementRange));

		if (m_SelectedUnit.m_MovableNodes.Contains(hitNode))
		{
			hitNode.m_NodeHighlight.ChangeHighlight(TileState.TargetMovement);
		}
	}

	void SkillSelection(Node castNode)
	{
		// Cast the skill the player has selected.
		// If hit unit is in affectable range,
		if (CanTargetNode(castNode))
		{
			if (m_SelectedUnit.GetActionPoints() >= m_SelectedSkill.m_Cost)
			{
				m_SelectedUnit.DecreaseActionPoints(m_SelectedSkill.m_Cost);
				m_SelectedUnit.ActivateSkill(m_SelectedSkill, castNode);

				UIManager.m_Instance.m_ActionPointCounter.UpdateActionPointCounter();

				// Now deselect the skill and clear the targeting highlights.
				CancelSkill();
			}
			else
			{
				Debug.Log("<color=#9c4141>[Skill]</color> Not enough action points!", m_SelectedUnit);
			}
		}
	}

	bool CanTargetNode(Node node)
	{
		List<Node> nodesTargetable = Grid.m_Instance.GetNodesWithinRadius(m_SelectedSkill.m_CastableDistance, Grid.m_Instance.GetNode(m_SelectedUnit.transform.position), true);

		switch (m_SelectedSkill.targetType)
		{
			case TargetType.SingleTarget:
				return node.unit && IsTargetable(m_SelectedUnit, node.unit, m_SelectedSkill) && nodesTargetable.Contains(m_CachedNode);
			case TargetType.Terrain:
				return nodesTargetable.Contains(m_CachedNode);
			default:
				Debug.LogError("Not Implemented!");
				return false;
		}
	}

	void CancelSkill()
	{
		if (m_TargetingState == TargetingState.Skill)
		{
			foreach (SkillButton button in UIManager.m_Instance.m_SkillSlots)
			{
				button.m_LightningImage.materialForRendering.SetFloat("_UIVerticalPan", 0);
			}

			m_TargetingState = TargetingState.Move;

			UpdateMoveablePreview(null, Grid.m_Instance.GetNode(m_SelectedUnit.transform.position));

			m_SelectedSkill = null;

			foreach (Unit unaffectedUnit in m_AffectedUnits) // No longer affected units
			{
				unaffectedUnit.m_Healthbar.m_KeepFocus = false;
				unaffectedUnit.m_Healthbar.ChangeFill((float)unaffectedUnit.GetCurrentHealth() / unaffectedUnit.GetStartingHealth(), false);
			}
		}
	}

	/// <summary>
	/// Select a skill.
	/// </summary>
	/// <param name="skill"> The skill being selected. </param>
	public void SkillSelection(BaseSkill skill, SkillButton button)
	{
		if (ParticlesManager.m_Instance.m_ActiveSkill != null)// || (ParticlesManager.m_Instance.m_ActiveSkill.m_Skill != null && ParticlesManager.m_Instance.m_ActiveSkill.m_Targets != null))
		{
			Debug.LogWarning($"{ParticlesManager.m_Instance.m_ActiveSkill.m_Skill} is currently active!");
			return;
		}
		// Don't allow progress if the character is an enemy (player can mouse over for info, but not use the skill)
		if (m_SelectedUnit.GetAllegiance() == Allegiance.Enemy) return;

		// Make sure the player has a unit selected.
		if (m_SelectedUnit != null)
		{
			// Check if the skill being cast is the heal skill.
			HealSkill hs = skill as HealSkill;
			if (hs != null)
			{
				// Check if this unit has Pestilence's passive (should be Pestilence but you never know).
				PestilencePassive pesPassive = m_SelectedUnit.GetPassiveSkill() as PestilencePassive;
				if (pesPassive != null)
				{
					// If there is no heal resource remaining, output warning about it and leave function.
					if (pesPassive.GetHealResource() < pesPassive.m_HealResourceCastCost)
					{
						Debug.LogWarning("Not enough heal resource for Pestilence to heal with.");
						return;
					}
				}
			}

			// Make sure the unit can afford to cast the skill and the skill isn't on cooldown before selecting it.
			// Just in case.
			if (m_SelectedUnit.GetActionPoints() >= skill.m_Cost && skill.GetCurrentCooldown() == 0)
			{
				foreach (SkillButton b in UIManager.m_Instance.m_SkillSlots)
				{
					b.m_LightningImage.materialForRendering.SetFloat("_UIVerticalPan", 0);
				}
				button.m_LightningImage.materialForRendering.SetFloat("_UIVerticalPan", 1);

				// Update the GameManager's fields
				m_SelectedSkill = skill;
				m_TargetingState = TargetingState.Skill;

				// Get the new affectable area.
				m_maxSkillRange = Grid.m_Instance.GetNodesWithinRadius(m_SelectedSkill.m_CastableDistance + m_SelectedSkill.m_AffectedRange, Grid.m_Instance.GetNode(m_SelectedUnit.transform.position), true);

				UpdateSkillPreview(null);
			}
		}
	}

	/// <summary>
	/// Select a skill.
	/// </summary>
	/// <param name="skillNumber"> Index of the skill being selected. </param>
	public void SkillSelection(int skillNumber) => SkillSelection(m_SelectedUnit.GetSkill(skillNumber), UIManager.m_Instance.m_SkillSlots[skillNumber]);

	/// <summary>
	/// Determines whether a given skill can hit a given target
	/// </summary>
	/// <param name="source">The casting character</param>
	/// <param name="target">The targeted character</param>
	/// <param name="skill">The skill the casting character is using</param>
	/// <returns></returns>
	public static bool IsTargetable(Unit source, Unit target, BaseSkill skill)
	{
		if (!target) return false;

		if (source == target && skill.excludeCaster) return false;

		if ((source.m_Allegiance == target.m_Allegiance && skill.targets == SkillTargets.Allies) ||
			(source.m_Allegiance != target.m_Allegiance && skill.targets == SkillTargets.Foes) ||
			(skill.targets == SkillTargets.All))
		{
			return true;
		}
		else return false;
	}

	/// <summary>
	/// Check the player's units to see if they're alive.
	/// </summary>
	public bool CheckIfAnyPlayerUnitsAlive()
	{
		// If true, all player units are dead.
		if (UnitsManager.m_Instance.m_PlayerUnits.Where(u => u.GetAlive()).Count() == 0)
		{
			// All the player's units are dead, the player lost.
			// Pause the game and display the lose screen for the player.
			Debug.Log("Everybody's dead, everybody's dead Dave!");
			return false;
		}
		return true;
	}

	/// <summary>
	/// Get the allegiance of which team's turn it currently is.
	/// </summary>
	/// <returns>The allegiance of the team whose turn it currently is.</returns>
	public Allegiance GetCurrentTurn() { return m_TeamCurrentTurn; }

	public void PodClearCheck()
	{
		if (UnitsManager.m_Instance.m_ActiveEnemyUnits.Count == 0)
		{
			if (!m_DidHealthBonus)
			{
				foreach (var item in UnitsManager.m_Instance.m_PlayerUnits.Where(u => u.GetAlive()))
				{
					item.IncreaseCurrentHealth(m_PodClearBonus);
				}
				m_DidHealthBonus = true;
			}
		}
	}

	public static void CreateVersionText()
	{
		if (GameObject.Find("VersionCanvas")) return;
		if (Application.version.Contains("Gold")) return;

		GameObject cgo = new GameObject("VersionCanvas", typeof(Canvas), typeof(CanvasScaler));
		DontDestroyOnLoad(cgo);
		CanvasScaler cs = cgo.gameObject.GetComponent<CanvasScaler>();
		Canvas c = cgo.GetComponent<Canvas>();
		c.renderMode = RenderMode.ScreenSpaceOverlay;
		cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
		cs.referenceResolution = new Vector2(1920, 1080);

		GameObject g = new GameObject("Version", typeof(CanvasRenderer), typeof(TextMeshProUGUI));
		RectTransform rt = g.GetComponent<RectTransform>();
		TextMeshProUGUI versionText = g.GetComponent<TextMeshProUGUI>();
		g.transform.SetParent(c.transform);
		g.transform.localScale = Vector3.one;
		rt.pivot = Vector2.zero;
		rt.anchorMax = Vector2.zero;
		rt.anchorMin = Vector2.zero;
		rt.position = Vector3.zero;
		versionText.autoSizeTextContainer = true;
		versionText.text = Application.version;
	}

	public void LoadMainMenu()
	{
		MusicManager.m_Instance.SetHorsemen(0);
		SceneManager.LoadScene(0);
	}
}
