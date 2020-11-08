using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum Allegiance
{
	/// <summary>
	/// Character is on the player's side.
	/// </summary>
	Player,

	/// <summary>
	/// Character is an enemy to the player.
	/// </summary>
	Enemy,

	/// <summary>
	/// Character doesn't have an allegiance (just to be safe).
	/// </summary>
	None
}

public class Unit : MonoBehaviour
{
	public string m_CharacterName;
	/// <summary>
	/// The starting health of the character.
	/// </summary>
	public int m_StartingHealth = 6;

	/// <summary>
	/// The current health of the character.
	/// </summary>
	private int m_CurrentHealth = 0;

	/// <summary>
	/// The starting movement of the character.
	/// </summary>
	public int m_StartingMovement = 5;

	/// <summary>
	/// The current movement of the character.
	/// </summary>
	private int m_CurrentMovement = 0;

	/// <summary>
	/// How fast the unit moves.
	/// </summary>
	public float m_MoveSpeed = 3.0f;

	public List<BaseSkill> m_LearnedSkills = new List<BaseSkill>();

	/// <summary>
	/// The skills avaliable to the unit.
	/// </summary>
	private List<BaseSkill> m_Skills = new List<BaseSkill>();

	public PassiveSkill m_Passive;

	/// <summary>
	/// The passive effect of the character.
	/// </summary>
	private PassiveSkill m_PassiveSkill = null;

	/// <summary>
	/// The status debuffs on the character.
	/// </summary>
	private List<InflictableStatus> m_StatusEffects = new List<InflictableStatus>();

	/// <summary>
	/// The allegiance of the character.
	/// </summary>
	public Allegiance m_Allegiance = Allegiance.None;

	/// <summary>
	/// Is the character alive?
	/// </summary>
	private bool m_IsAlive = true;

	/// <summary>
	/// Is the character moving?
	/// </summary>
	private bool m_IsMoving = false;

	/// <summary>
	/// The path for the character to take to get to their destination.
	/// </summary>
	private Stack<Node> m_MovementPath = new Stack<Node>();

	/// <summary>
	/// The node the unit is targeting for their movement.
	/// </summary>
	private Node m_CurrentTargetNode = null;

	/// <summary>
	/// The node's the unit can move to.
	/// </summary>
	public List<Node> m_MovableNodes = new List<Node>();

	/// <summary>
	/// The image representing the unit's health.
	/// </summary>
	private HealthbarContainer m_Healthbar = null;

	/// <summary>
	/// The script for the health change indicator.
	/// </summary>
	private HealthChangeIndicator m_HealthChangeIndicatorScript = null;

	/// <summary>
	/// The starting action points of the unit.
	/// </summary>
	public int m_StartingActionPoints = 1;

	/// <summary>
	/// The currect action points of the unit.
	/// </summary>
	private int m_CurrentActionPoints = 0;

	/// <summary>
	/// Position of the unit's healthbar above their head.
	/// </summary>
	public Transform m_HealthbarPosition = null;

	public Action m_ActionOnFinishPath;

	public AIHeuristicCalculator m_AIHeuristicCalculator = null;

	public Animator m_animator;

	public ParticleSystem m_SummonParticle;

	private int m_TakeExtraDamage = 0;

	private int m_DealExtraDamage = 0;

	private int? m_DealingDamage;

	public TextAsset m_KillDialogue;

	public UIData m_UIData;

	public List<Transform> m_ParentedParticleSystems = new List<Transform>();

	[FMODUnity.EventRef]
	public string m_DeathSound = "";

	// On startup.

	void Awake()
	{
		m_CurrentHealth = m_StartingHealth;

		m_CurrentMovement = m_StartingMovement;

		m_CurrentActionPoints = m_StartingActionPoints;

		m_Skills = m_LearnedSkills.Select(s => Instantiate(s)).ToList();

		if (m_Passive)
		{
			m_PassiveSkill = Instantiate(m_Passive);
		}

		// Set all skills to startup stuff, cause scriptable objects don't reset on scene load.
		// @Grant - this is because you're only meant to use the instantiated versions!
		foreach (BaseSkill skill in m_LearnedSkills)
		{
			skill.Startup();
		}
	}

	void Start()
	{
		m_animator = GetComponent<Animator>();

		foreach (BaseSkill skill in m_Skills)
		{
			skill.CreatePrefab(this);
		}
	}

	void Update()
	{
		// If have a target that the unit hasn't arrived at yet, move towards the target position.
		if (m_IsMoving)
		{
			transform.position = Vector3.MoveTowards(transform.position, m_CurrentTargetNode.worldPosition, m_MoveSpeed * Time.deltaTime);

			// If have arrived at position (0.1 units close to target is close enough).
			if ((transform.position - m_CurrentTargetNode.worldPosition).magnitude < 0.1f)
			{
				// Set the actual position to the target
				transform.position = m_CurrentTargetNode.worldPosition; // Just putting this here so it sets the position exactly. - James L

				// Target the next node.

				if (m_MovementPath.Count > 0)
				{
					SetTargetNodePosition(m_MovementPath.Pop());
				}
				// Have arrived at the final node in the path, stop moving.
				else
				{
					m_IsMoving = false;

					//m_animator.SetBool("isWalking", m_IsMoving);

					Grid.m_Instance.SetUnit(this);
					m_ActionOnFinishPath?.Invoke();
					m_ActionOnFinishPath = null;
				}
			}
		}
	}

	/// <summary>
	/// Set the health of the unit.
	/// </summary>
	/// <param name="health"> What to set the unit's health to. </param>
	public void SetCurrentHealth(int health)
	{
		m_CurrentHealth = health;

		// Don't go over that max starting health.
		if (m_CurrentHealth > m_StartingHealth)
			m_CurrentHealth = m_StartingHealth;

		if (m_Healthbar != null)
		{
			m_Healthbar.gameObject.SetActive(true);
			m_Healthbar.transform.position = Camera.main.WorldToScreenPoint(m_HealthbarPosition.position);
			m_Healthbar.m_HealthbarImage.fillAmount = (float)m_CurrentHealth / m_StartingHealth;
			m_Healthbar.SetChildrenActive(true);
			m_HealthChangeIndicatorScript.SetStartPositionToCurrent();
			m_HealthChangeIndicatorScript.Reset();
			m_Healthbar.Reset();
		}
	}

	/// <summary>
	/// Get the current health of the unit.
	/// </summary>
	/// <returns> The current health of the unit. </returns>
	public int GetCurrentHealth() { return m_CurrentHealth; }

	public int GetStartingHealth() { return m_StartingHealth; }

	/// <summary>
	/// Increase the unit's current health.
	/// </summary>
	/// <param name="increase"> The amount to increase the unit's health by. </param>
	public void IncreaseCurrentHealth(int increase)
	{
		SetCurrentHealth(m_CurrentHealth + increase);

		if (m_Healthbar != null)
		{
			m_Healthbar.m_HealthChangeIndicator.text = "+" + increase;
			m_HealthChangeIndicatorScript.HealthIncreased();
		}
	}

	/// <summary>
	/// Decrease the unit's current health.
	/// </summary>
	/// <param name="decrease"> The amount to decrease the unit's health by. </param>
	public void DecreaseCurrentHealth()
	{
		ParticlesManager.m_Instance.RemoveUnitFromTarget();

		int damage = (int)m_DealingDamage + m_TakeExtraDamage;

		SetCurrentHealth(m_CurrentHealth - damage);
		m_TakeExtraDamage = 0;

		if (m_PassiveSkill != null)
		{
			if (m_PassiveSkill.CheckPrecondition(TriggerType.OnTakeDamage, this))
			{
				if (m_PassiveSkill.GetAffectSelf() == true)
					m_PassiveSkill.TakeEffect(this);
				else
					m_PassiveSkill.TakeEffect();
			}
			if (m_PassiveSkill.CheckPrecondition(TriggerType.OnTakeDamage))
			{
				if (m_PassiveSkill.GetAffectSelf() == true)
					m_PassiveSkill.TakeEffect(this);
				else
					m_PassiveSkill.TakeEffect();
			}
		}

		foreach (InflictableStatus status in m_StatusEffects)
		{
			if (status.CheckPrecondition(TriggerType.OnTakeDamage) == true)
			{
				status.TakeEffect(this);
			}
		}

		if (m_Healthbar != null)
		{
			m_Healthbar.m_HealthChangeIndicator.text = "-" + damage;
			m_HealthChangeIndicatorScript.HealthDecrease();
		}
	}

	private void AddStatusEffectFromSkill(InflictableStatus effect)
	{
		ParticlesManager.m_Instance.RemoveUnitFromTarget();
		AddStatusEffect(effect);
	}

	private void AddHealingFromSkill(int heal)
	{
		ParticlesManager.m_Instance.RemoveUnitFromTarget();
		IncreaseCurrentHealth(heal);
	}

	public void IncomingSkill(BaseSkill skill)
	{
		switch (skill)
		{
			case StatusSkill ss:

				if (ss.m_DamageAmount > 0)
				{
					m_DealingDamage = ss.m_DamageAmount + ss.m_ExtraDamage;
					if (m_CurrentHealth - m_DealingDamage <= 0)
					{
						m_animator.SetTrigger("TriggerDeath");
					}
					else
					{
						m_animator.SetTrigger("TriggerDamage");
						// Trigger Death Particle
					}
					AddStatusEffect(ss.m_Effect);
				}
				else
				{
					AddStatusEffectFromSkill(ss.m_Effect);
				}
				break;
			case DamageSkill ds:
				m_DealingDamage = ds.m_DamageAmount + ds.m_ExtraDamage;
				if (m_CurrentHealth - m_DealingDamage <= 0)
				{
					m_animator.SetTrigger("TriggerDeath");
				}
				else
				{
					m_animator.SetTrigger("TriggerDamage");
					// Trigger Death Particle
				}
				break;
			case HealSkill hs:
				AddHealingFromSkill(hs.m_HealAmount);
				break;
			default:
				break;
		}
	}

	public void IncomingNonSkillDamage(int damage)
	{
		m_DealingDamage = damage;
		if (m_CurrentHealth - m_DealingDamage <= 0)
		{
			m_animator.SetTrigger("TriggerDeath");
		}
		else
		{
			m_animator.SetTrigger("TriggerDamage");
			// Trigger Death Particle
		}
	}

	public void CallSkillEffects()
	{
		/* 
		 *  This is mostly error handling for casting a point blank basic ranged attack.
		 *  When this happens, the orb is applying the effects of the skill and clearing
		 *  the current skill before the second animation trigger which then attempts to
		 *  apply the effects. - James L
		 */
		if (ParticlesManager.m_Instance.m_ActiveSkill == null) return;

		if (ParticlesManager.m_Instance.m_ActiveSkill.m_Skill.m_SkillName == "Basic Ranged Attack")
		{
			return;
		}
		ParticlesManager.m_Instance.TakeSkillEffects();
	}

	public void PlaySkillParticleSystem()
	{
		SkillWithTargets activeSkill = ParticlesManager.m_Instance.m_ActiveSkill;
		if (activeSkill.m_Skill.m_SkillName == "Basic Ranged Attack")
		{
			Vector3 targetPos = activeSkill.m_Targets[0].transform.position;
			ParticlesManager.m_Instance.OnRanged(this, new Vector3(targetPos.x, 1, targetPos.z), activeSkill.m_Targets[0]);
			return;
		}
		switch (activeSkill.m_Skill.m_SpawnLocation)
		{
			case ParticleSpawnType.Target:
				activeSkill.m_Skill.PlayEffect(activeSkill.m_Targets[0]);
				break;
			case ParticleSpawnType.Caster:
				activeSkill.m_Skill.PlayEffect(GameManager.m_Instance.m_SelectedUnit);
				break;
			case ParticleSpawnType.Tile:
				activeSkill.m_Skill.PlayEffect(activeSkill.m_Skill.m_CastNode.worldPosition);
				break;
			case ParticleSpawnType.Other:
				break;
			default:
				break;
		}
	}

	/// <summary>
	/// Check if the unit's health is above 0.
	/// If equal to or below, the unit is not alive.
	/// </summary>
	public void KillUnit()
	{
		Debug.Log($"<color=#a87932>[Death] </color>{name} died");
		m_IsAlive = false;

		while (m_ParentedParticleSystems.Count > 0)
		{
			m_ParentedParticleSystems[0].parent = null;
			m_ParentedParticleSystems.RemoveAt(0);
		}

		// Check if the unit has the "DefeatEnemyWinCondition" script on it.
		// If it does, the player has won the level by defeating the boss.
		GetComponent<DefeatEnemyWinCondition>()?.EnemyDefeated();

		// If this is a player unit, check if the player has any units remaining.
		if (m_Allegiance == Allegiance.Player)
		{
			UnitsManager.m_Instance.m_DeadPlayerUnits.Add(this);
			UnitsManager.m_Instance.m_PlayerUnits.Remove(this);
		}
		else
		{
			AIManager.m_Instance.DisableUnits(this);
		}

		Node currentNode = Grid.m_Instance.GetNode(transform.position);
		currentNode.unit = null;
		currentNode.m_isBlocked = false;

		if (m_DeathSound != "")
			FMODUnity.RuntimeManager.PlayOneShot(m_DeathSound, transform.position);

		gameObject.SetActive(false);
	}

	public void PlayerDeath()
	{
		Debug.Log($"<color=#a87932>[Death] </color>{name} died");
		m_IsAlive = false;

		while (m_ParentedParticleSystems.Count > 0)
		{
			m_ParentedParticleSystems[0].parent = null;
		}

		// Check if the unit has the "DefeatEnemyWinCondition" script on it.
		// If it does, the player has won the level by defeating the boss.
		GetComponent<DefeatEnemyWinCondition>()?.EnemyDefeated();

		// If this is a player unit, check if the player has any units remaining.
		if (m_Allegiance == Allegiance.Player)
		{
			UnitsManager.m_Instance.m_DeadPlayerUnits.Add(this);
			UnitsManager.m_Instance.m_PlayerUnits.Remove(this);
		}
		else
		{
			AIManager.m_Instance.DisableUnits(this);
		}

		Node currentNode = Grid.m_Instance.GetNode(transform.position);
		currentNode.unit = null;
		currentNode.m_isBlocked = false;

		if (m_DeathSound != "")
			FMODUnity.RuntimeManager.PlayOneShot(m_DeathSound, transform.position);

		if (m_KillDialogue)
		{
			if (GetComponent<DefeatEnemyWinCondition>())
			{
				DialogueManager.instance.QueueDialogue(m_KillDialogue, () => UIManager.m_Instance.m_CrawlDisplay.LoadCrawl(Outcome.Win));
			}
			else if (!GameManager.m_Instance.CheckIfAnyPlayerUnitsAlive())
			{
				UIManager.m_Instance.m_CrawlDisplay.m_OnEndCrawlEvent = UIManager.m_Instance.ShowCrawlButtons;
				DialogueManager.instance.QueueDialogue(m_KillDialogue, () => UIManager.m_Instance.m_CrawlDisplay.LoadCrawl(Outcome.Loss));
			}
			else
			{
				DialogueManager.instance.QueueDialogue(m_KillDialogue, KillUnit);
			}
		}
	}

	/// <summary>
	/// Get the current amount of movement of the character.
	/// </summary>
	/// <returns> The unit's current movement. </summary>
	public int GetCurrentMovement() => m_CurrentMovement;

	/// <summary>
	/// Decrease the character's current amount of movement.
	/// </summary>
	/// <param name="decrease"> The amount to decrease the unit's movement pool by. </param>
	public void DecreaseCurrentMovement(int decrease) => m_CurrentMovement -= decrease;

	/// <summary>
	/// Reset the unit's current movement.
	/// </summary>
	public void ResetCurrentMovement() => m_CurrentMovement = m_StartingMovement;

	/// <summary>
	/// Get the list of skills of the unit.
	/// </summary>
	/// <returns> List of skills the unit has. </returns>
	public List<BaseSkill> GetSkills() => m_Skills;

	/// <summary>
	/// Get a specific skill.
	/// </summary>
	/// <param name="skillIndex"> The index of the skill to get. </param>
	public BaseSkill GetSkill(int skillIndex) { try { return m_Skills[skillIndex]; } catch (Exception) { return null; } }

	// Set the movement path of the character.
	public void SetMovementPath(Stack<Node> path)
	{
		m_MovementPath = path;
		m_IsMoving = true;

		m_animator.SetBool("isWalking", m_IsMoving);

		SetTargetNodePosition(m_MovementPath.Pop());
	}

	/// <summary>
	/// Set the target node of the unit.
	/// </summary>
	/// <param name="target"> The node to set as the target. </param>
	public void SetTargetNodePosition(Node target, bool onlySetNode = false)
	{
		// Unassign the unit on the current node.
		// Before setting the new target node.

		// Had to add a hack around this. sorry - James L
		if (!onlySetNode)
		{
			Grid.m_Instance.RemoveUnit(m_CurrentTargetNode);
		}
		m_CurrentTargetNode = target;
		transform.LookAt(m_CurrentTargetNode.worldPosition);
	}

	/// <summary>
	/// Get the unit's path.
	/// </summary>
	/// <returns> Stack of the unit's movement path. </returns>
	public Stack<Node> GetMovementPath() => m_MovementPath; 

	/// <summary>
	/// Get the unit's allegiance.
	/// </summary>
	/// <returns> The allegiance of the unit. </returns>
	public Allegiance GetAllegiance() => m_Allegiance; 

	/// <summary>
	/// Get if the unit is alive.
	/// </summary>
	/// <returns>If the unit is alive.</returns>
	public bool GetAlive() => m_IsAlive; 

	/// <summary>
	/// Get the unit's action points.
	/// </summary>
	/// <returns>The current amount of action points the unit has.</returns>
	public int GetActionPoints() => m_CurrentActionPoints; 

	/// <summary>
	/// Decrease the amount of action points the unit has.
	/// </summary>
	/// <param name="decrease">The amount to decrease the unit's action points by.</param>
	public void DecreaseActionPoints(int decrease)=> m_CurrentActionPoints -= decrease;

	/// <summary>
	/// Reset the unit's action points.
	/// </summary>
	public void ResetActionPoints() => m_CurrentActionPoints = m_StartingActionPoints;

	/// <summary>
	/// Add a status effect to the unit.
	/// </summary>
	/// <param name="effect"> The status effect to add to the unit. </param>
	public void AddStatusEffect(InflictableStatus effect) => m_StatusEffects.Add(effect); 

	public void RemoveStatusEffect(InflictableStatus effect) => m_StatusEffects.Remove(effect); 

	public List<InflictableStatus> GetInflictableStatuses() => m_StatusEffects; 

	/// <summary>
	/// Set the healthbar of the unit.
	/// </summary>
	/// <param name="healthbar">The healthbar game object.</param>
	public void SetHealthbar(HealthbarContainer healthbar)
	{
		m_Healthbar = healthbar.GetComponent<HealthbarContainer>();
		m_HealthChangeIndicatorScript = healthbar.m_HealthChangeIndicator.GetComponent<HealthChangeIndicator>();
	}

	public HealthbarContainer GetHealthBar() => m_Healthbar;

	/// <summary>
	/// Get the heuristic calculator on the unit.
	/// </summary>
	/// <returns>The unit's heuristic calculator.</returns>
	public AIHeuristicCalculator GetHeuristicCalculator() => m_AIHeuristicCalculator; 

	/// <summary>
	/// Get the passive skill on the unit.
	/// </summary>
	/// <returns>The unit's passive skill, null if it doesn't have one.</returns>
	public PassiveSkill GetPassiveSkill() => m_PassiveSkill; 

	/// <summary>
	/// Add extra damage for the unit to take when damaged.
	/// </summary>
	/// <param name="extra">The amount of extra damage to take.</param>
	public void AddTakeExtraDamage(int extra) => m_TakeExtraDamage += extra;

	/// <summary>
	/// Add extra damage for the unit to deal when attacking.
	/// </summary>
	/// <param name="extra">The amount of extra damage to deal.</param>
	public void AddDealExtraDamage(int extra) => m_DealExtraDamage += extra;

	public void SetDealExtraDamage(int extra)
	{
		m_DealExtraDamage = extra;
	}

	/// <summary>
	/// Check if the unit is moving.
	/// </summary>
	/// <returns>If the unit is moving or not.</returns>
	public bool GetMoving() => m_IsMoving;

	/// <summary>
	/// Gets the nodes the unit can move to, stores them and highlights them.
	/// </summary>
	/// <param name="startingNode"> The node to search from, can find it's own position if it can't be provided. </param>
	public void HighlightMovableNodes(Node startingNode = null)
	{
		m_MovableNodes = Grid.m_Instance.GetNodesWithinRadius(GetCurrentMovement(), startingNode ?? Grid.m_Instance.GetNode(transform.position)); // Returns the current node by default, but can be overridden

		foreach (Node node in m_MovableNodes)
		{
			node.m_NodeHighlight.ChangeHighlight(TileState.MovementRange);
		}
	}

	/// <summary>
	/// Activate one of the unit's skills.
	/// </summary>
	/// <param name="skill"> The skill to activate. </param>
	public void ActivateSkill(BaseSkill skill, Node castLocation)
	{
		Debug.Log($"<color=#9c4141>[Skill] </color>{GameManager.m_Instance.GetSelectedUnit().name} casts {skill.m_SkillName}" +
			$" {(castLocation.unit ? $"at {castLocation.unit.name}" : "")} ({castLocation.m_NodeHighlight.name})");
		// Doing my own search cause List.Find is gross.
		for (int i = 0; i < m_Skills.Count; ++i)
		{
			// Check if the unit has the skill being cast.
			if (m_Skills[i].m_SkillName == skill.m_SkillName)
			{
				skill.m_AffectedNodes = Grid.m_Instance.GetNodesWithinRadius(m_Skills[i].m_AffectedRange, castLocation, true);
				skill.m_CastNode = castLocation;
				if (m_PassiveSkill != null)
				{
					DamageSkill ds = skill as DamageSkill;

					// Check if skill being cast is a damage skill.
					if (ds != null)
					{
						// Make sure the skill knows what units it will affect, so we can check them for the passive.
						List<Unit> hitUnits = ds.FindAffectedUnits();

						if (m_PassiveSkill.GetAffectSelf() == false)
						{
							// Check which units meet the prerequisits for the unit's passive.
							foreach (Unit u in hitUnits)
							{
								foreach (InflictableStatus status in m_StatusEffects)
								{
									if (status.CheckPrecondition(TriggerType.OnDealDamage) == true)
									{
										status.TakeEffect(this);
									}
								}

								// Add extra damage to the skill from status effect (if there is any).
								if (m_DealExtraDamage > 0)
								{
									ds.AddExtraDamage(m_DealExtraDamage);
								}

								if (m_PassiveSkill.CheckPrecondition(TriggerType.OnDealDamage, u))
									m_PassiveSkill.TakeEffect(u);
								if (m_PassiveSkill.CheckPrecondition(TriggerType.OnDealDamage))
									m_PassiveSkill.TakeEffect();
							}
						}
						else
						{
							if (m_PassiveSkill.CheckPrecondition(TriggerType.OnDealDamage, this))
							{
								m_PassiveSkill.TakeEffect(this);
							}
							if (m_PassiveSkill.CheckPrecondition(TriggerType.OnDealDamage))
								m_PassiveSkill.TakeEffect(this);
						}
					}

					// Check if the skill being cast is the heal skill.
					HealSkill hs = skill as HealSkill;
					if (hs != null)
					{
						// Check if this unit has Pestilence's passive (should be Pestilence but you never know).
						PestilencePassive pesPassive = m_PassiveSkill as PestilencePassive;
						if (pesPassive != null)
						{
							// Use the heal resource before casting the skill.
							if (pesPassive.GetHealResource() > 0)
							{
								pesPassive.UseHealResource();
							}
							// If there is no heal resource remaining, output warning about it and leave function.
							else
							{
								Debug.LogWarning("Not enough heal resource for Pestilence to heal with.");
								return;
							}
						}
					}
				}
				if (skill.m_IsMagic)
				{
					// TODO Play cast system
				}
				skill.CastSkill();
				transform.LookAt(castLocation.worldPosition);

				// Play skill animation
				m_animator.SetTrigger("TriggerSkill");

				// Play the damage sound effect.
				FMODUnity.RuntimeManager.PlayOneShot(m_Skills[i].m_CastEvent, transform.position);
				return;
			}
		}

		Debug.LogError("Skill " + skill.m_SkillName + " couldn't be found in " + gameObject.name + ".");
	}



	/*=====================================DEBUG STUFF AHEAD=====================================*/

	[ContextMenu("Inflict Hunger")]
	protected void Hunger() => AddStatusEffect(Instantiate(Resources.Load("Skills/S_AttackDown")) as InflictableStatus);
	[ContextMenu("Inflict Mark")]
	protected void Mark() => AddStatusEffect(Instantiate(Resources.Load("Skills/S_DamageOverTime")) as InflictableStatus);
	[ContextMenu("Inflict Riches")]
	protected void Riches() => AddStatusEffect(Instantiate(Resources.Load("Skills/S_AttackUp")) as InflictableStatus);

	[ContextMenu("Passive Status")]
	protected void PassiveStatus()
	{
		if (m_PassiveSkill)
		{
			System.Reflection.FieldInfo[] fieldInfos = m_PassiveSkill.GetType().GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);

			string output = "======" + m_PassiveSkill.m_StatusName + "======\n";

			foreach (var item in fieldInfos)
			{
				try
				{
					output += $"{item.Name}: {item.GetValue(m_PassiveSkill)}\n";
				}
				catch (ArgumentException)
				{
					output += $"{item.Name}: unobtainable\n";
				}

			}

			Debug.Log(output, this);
		}
	}
}
