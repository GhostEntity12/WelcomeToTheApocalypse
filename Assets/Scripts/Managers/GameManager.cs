using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
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
    // TODO: set back to private
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
    private List<Node> m_maxSkillRange = new List<Node>();

    /// <summary>
    /// Is the mouse hovering over a UI element that will block the player's inputs?
    /// </summary>
    public bool m_MouseOverUIBlockingElements = false;

    private CameraMovement m_CameraMovement;

    public int m_PodClearBonus = 5;
    public bool m_DidHealthBonus;

    [FMODUnity.EventRef]
    public string m_TurnEndSound = "";

    // On startup.
    private void Awake()
    {
        m_MainCamera = Camera.main;
        m_MouseRay.origin = m_MainCamera.transform.position;

        m_Instance = this;

        CreateVersionText();
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
        //if (m_CurrentTurn == Allegiance.Player)
        if (!UIManager.m_Instance.m_ActiveUI)
        {
            PlayerInputs();
        }

        Debug.DrawLine(m_MainCamera.transform.position, m_MouseWorldRayHit.point);
    }

    /// <summary>
    /// Get the unit currently selected by the player.
    /// </summary>
    /// <returns> The unit the player has selected. </returns>
    public Unit GetSelectedUnit() { return m_SelectedUnit; }

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
        UIManager.m_Instance.SlideSkills(UIManager.ScreenState.Offscreen);

        // Player ends turn, start enemy turn
        if (m_TeamCurrentTurn == Allegiance.Player)
        {
            m_TeamCurrentTurn = Allegiance.Enemy;

            UIManager.m_Instance.SwapTurnIndicator(m_TeamCurrentTurn);

            foreach (Unit u in UnitsManager.m_Instance.m_PlayerUnits)
            {
                u.SetDealExtraDamage(0);
                foreach (InflictableStatus IS in u.GetInflictableStatuses())
                {
                    // If returns true, status effect's duration has reached 0, remove the status effect.
                    if (IS.DecrementDuration() == true)
                    {
                        u.RemoveStatusEffect(IS);
                    }
                }
            }

            // Stop highlighting node's the player can move to.
            if (m_SelectedUnit)
            {
                foreach (Node n in m_SelectedUnit.m_MovableNodes)
                {
                    n.m_NodeHighlight.ChangeHighlight(TileState.None);
                }
            }
            // Clear the skill targeting highlights.
            foreach (Node n in m_maxSkillRange)
            {
                m_maxSkillRange.ForEach(m => m.m_NodeHighlight.m_IsAffected = false);
                m_maxSkillRange.ForEach(m => m.m_NodeHighlight.m_IsInTargetArea = false);
                n.m_NodeHighlight.ChangeHighlight(TileState.None);
            }
            // Reset stuff.
            m_SelectedUnit = null;
            m_SelectedSkill = null;
            m_TargetingState = TargetingState.Move;

            // Check the passives of all the enemy units for any that trigger at the start of their turn.
            foreach (Unit u in UnitsManager.m_Instance.m_ActiveEnemyUnits)
            {
                u.ResetActionPoints();

                PassiveSkill ps = u.GetPassiveSkill();
                if (ps != null)
                {
                    if (ps.CheckPrecondition(TriggerType.OnTurnStart, u) || ps.CheckPrecondition(TriggerType.OnTurnStart))
                    {
                        if (ps.GetAffectSelf() == true)
                            ps.TakeEffect(u);
                        else
                            ps.TakeEffect();
                    }
                }
            }

            // Play the end turn sound on the camera.
            FMODUnity.RuntimeManager.PlayOneShot(m_TurnEndSound, Camera.main.transform.position);

            // Tell the AI Manager to take its turn
            AIManager.m_Instance.SetAITurn(true);
        }
        // Enemy ends turn, start player turn
        else
        {
            m_TeamCurrentTurn = Allegiance.Player;

            AIManager.m_Instance.SetAITurn(false);

            UIManager.m_Instance.SwapTurnIndicator(m_TeamCurrentTurn);

            // Deselect unit.
            m_SelectedUnit = null;

            // Reset the player's units.
            foreach (Unit u in UnitsManager.m_Instance.m_PlayerUnits)
            {
                u.ResetActionPoints();

                // Check the passives of all the player units for any that trigger at the start of their turn.
                PassiveSkill ps = u.GetPassiveSkill();

                if (ps != null)
                    ps.CheckPrecondition(TriggerType.OnTurnStart);

                foreach (BaseSkill s in u.GetSkills())
                {
                    s.DecrementCooldown();
                }

                foreach (InflictableStatus status in u.GetInflictableStatuses())
                {
                    if (status.CheckPrecondition(TriggerType.OnTurnStart) == true)
                    {
                        status.TakeEffect(u);
                    }
                }
            }
        }

        // Reset the movement of the units whos turn it now is.
        foreach (Unit u in UnitsManager.m_Instance.m_AllUnits)
        {
            if (u.GetAllegiance() == m_TeamCurrentTurn)
                u.ResetCurrentMovement();
        }

        // Tell end turn button who's turn it currently is.
        UIManager.m_Instance.m_EndTurnButton.UpdateCurrentTeamTurn(m_TeamCurrentTurn);
    }

    /// <summary>
    /// Get's the player's inputs.
    /// </summary>
    public void PlayerInputs()
    {
        m_MouseRay = m_MainCamera.ScreenPointToRay(Input.mousePosition);

        m_LeftMouseDown = Input.GetMouseButtonDown(0);

        m_MouseOverUIBlockingElements = UIManager.m_Instance.CheckUIBlocking();

        // Mouse is over a unit.
        if (Physics.Raycast(m_MouseRay, out m_MouseWorldRayHit, Mathf.Infinity, 1 << 9))
        {
            Unit rayHitUnit = m_MouseWorldRayHit.transform.GetComponent<Unit>();
            // Check if the player is selecting another character.
            if (m_TargetingState == TargetingState.Move)
            {
                // Check player input.
                if (m_LeftMouseDown && !m_MouseOverUIBlockingElements)
                {
                    m_CameraMovement.m_AutoMoveDestination = new Vector3(rayHitUnit.transform.position.x, 0, rayHitUnit.transform.position.z);
                    // If the unit the player is hovering over isn't the selected unit and the unit is alive, select that unit.
                    if (rayHitUnit != m_SelectedUnit && rayHitUnit.GetAlive() == true)
                    {
                        // Reset the nodes highlights before selecting the new unit
                        m_maxSkillRange.ForEach(s => s.m_NodeHighlight.m_IsInTargetArea = false);
                        m_SelectedUnit?.m_MovableNodes.ForEach(u => u.m_NodeHighlight.ChangeHighlight(TileState.None));

                        // Store the new unit
                        m_SelectedUnit = rayHitUnit;
                        UIManager.m_Instance.SwapSkillsUI(m_SelectedUnit.m_UIData);
                        UIManager.m_Instance.m_UIHealthBar.SetHealthAmount((float)m_SelectedUnit.GetCurrentHealth() / m_SelectedUnit.GetStartingHealth());

                        // Highlight the appropriate tiles
                        m_SelectedUnit.m_MovableNodes = Grid.m_Instance.GetNodesWithinRadius(m_SelectedUnit.GetCurrentMovement(), Grid.m_Instance.GetNode(m_SelectedUnit.transform.position));
                        m_SelectedUnit.HighlightMovableNodes();

                        StatusEffectTooltipManager.m_Instance.UpdateActiveEffects();

                        // Update the UI's action point counter to display the newly selected unit's action points.
                        UIManager.m_Instance.m_ActionPointCounter.ResetActionPointCounter();
                        UIManager.m_Instance.m_ActionPointCounter.UpdateActionPointCounter();
                    }
                }
            }
            //Check if the player is casting a skill on a unit.
            else if (m_TargetingState == TargetingState.Skill)
            {
                Node unitNode = Grid.m_Instance.GetNode(rayHitUnit.transform.position);
                // Display the target area for the skill and it's area of effect.
                if (unitNode != m_CachedNode)
                {
                    // Update the cache
                    m_CachedNode = unitNode;
                    // If it's targetable
                    if (m_CachedNode.m_NodeHighlight.m_IsTargetable)
                    {
                        // Display pink area
                        List<Node> targetableRange = Grid.m_Instance.GetNodesWithinRadius(m_SelectedSkill.m_AffectedRange, m_CachedNode, true);
                        m_maxSkillRange.ForEach(n => n.m_NodeHighlight.m_IsAffected = targetableRange.Contains(n));
                    }
                    // Otherwise clear the pink area
                    else
                    {
                        m_maxSkillRange.ForEach(n => n.m_NodeHighlight.m_IsAffected = false);
                    }
                }
                // Check player input.
                if (m_LeftMouseDown && !m_MouseOverUIBlockingElements)
                {
                    // Cast the skill the player has selected.
                    // If hit unit is in affectable range,
                    if (unitNode.m_NodeHighlight.m_IsTargetable)
                    {
                        if (m_SelectedUnit.GetActionPoints() >= m_SelectedSkill.m_Cost)
                        {
                            m_SelectedUnit.DecreaseActionPoints(m_SelectedSkill.m_Cost);
                            m_SelectedUnit.ActivateSkill(m_SelectedSkill, unitNode);
                            Debug.Log(m_SelectedUnit.GetActionPoints(), m_SelectedUnit);

                            UIManager.m_Instance.m_ActionPointCounter.UpdateActionPointCounter();

                            // Now deselect the skill and clear the targeting highlights.
                            CancelSkill();
                        }
                        else
                        {
                            Debug.Log("Not enough action points!", m_SelectedUnit);
                        }
                    }
                }
            }
        }
        // Mouse is over a tile.
        else if (Physics.Raycast(m_MouseRay, out m_MouseWorldRayHit, Mathf.Infinity, 1 << 8))
        {
            Node hitNode = Grid.m_Instance.GetNode(m_MouseWorldRayHit.transform.position);
            // The player is currently targeting for a skill.
            if (m_TargetingState == TargetingState.Skill)
            {
                // Display the target area for the skill and it's area of effect.
                if (hitNode != m_CachedNode)
                {
                    // Update the cache
                    m_CachedNode = hitNode;
                    // If it's targetable
                    if (m_CachedNode.m_NodeHighlight.m_IsTargetable)
                    {
                        // Display pink area
                        List<Node> targetableRange = Grid.m_Instance.GetNodesWithinRadius(m_SelectedSkill.m_AffectedRange, m_CachedNode, true);
                        m_maxSkillRange.ForEach(n => n.m_NodeHighlight.m_IsAffected = targetableRange.Contains(n));
                    }
                    // Otherwise clear the pink area
                    else
                    {
                        m_maxSkillRange.ForEach(n => n.m_NodeHighlight.m_IsAffected = false);
                    }
                }

                // Check player input.
                if (m_LeftMouseDown && !m_MouseOverUIBlockingElements)
                {
                    // Cast the skill the player has selected.
                    // If hit tile is in affectable range,
                    if (hitNode.m_NodeHighlight.m_IsTargetable)
                    {
                        if (m_SelectedUnit.GetActionPoints() >= m_SelectedSkill.m_Cost)
                        {
                            m_SelectedUnit.ActivateSkill(m_SelectedSkill, hitNode);
                            m_SelectedUnit.DecreaseActionPoints(m_SelectedSkill.m_Cost);

                            UIManager.m_Instance.m_ActionPointCounter.UpdateActionPointCounter();
                            // Now deselect the skill and clear the targeting highlights.
                            CancelSkill();
                        }
                        else
                        {
                            Debug.Log("Not enough action points!");
                        }
                    }
                }
            }
            // The player is choosing a tile to move a unit to.
            else if (m_TargetingState == TargetingState.Move)
            {
                // Make sure a unit is selected.
                if (m_SelectedUnit != null && m_SelectedUnit.GetAllegiance() == Allegiance.Player && m_SelectedUnit.GetMoving() == false)
                {
                    // Check input.
                    if (m_LeftMouseDown && !m_MouseOverUIBlockingElements)
                    {
                        if (m_SelectedUnit.m_MovableNodes.Contains(hitNode))
                        {
                            // Clear the previously highlighted tiles
                            foreach (Node n in m_SelectedUnit.m_MovableNodes)
                            {
                                n.m_NodeHighlight.ChangeHighlight(TileState.None);
                            }
                            Stack<Node> path = new Stack<Node>();
                            if (Grid.m_Instance.FindPath(m_SelectedUnit.transform.position, m_MouseWorldRayHit.transform.position, out path, out m_MovementCost, true))
                            {
                                m_SelectedUnit.SetMovementPath(path);
                                // Decrease the unit's movement by the cost.
                                m_SelectedUnit.DecreaseCurrentMovement(m_MovementCost);
                            }
                            // Should we do this after the unit has finished moving? - James L
                            m_SelectedUnit.HighlightMovableNodes(hitNode);
                        }
                    }
                }
            }
        }

        // Selecting a skill with the number keys.
        for (int i = 0; i < m_AbilityHotkeys.Length; i++)
        {
            if (Input.GetKeyDown(m_AbilityHotkeys[i]))
            {
                // Make sure the player can use the skill before selecting it.
                if (m_SelectedUnit.m_LearnedSkills[i].GetCurrentCooldown() == 0 && m_SelectedUnit.GetActionPoints() >= m_SelectedUnit.m_LearnedSkills[i].m_Cost)
                {
                    SkillSelection(i);
                    break;
                }
            }
        }

        // Cancelling skill targeting.
        if (Input.GetMouseButtonDown(1))
        {
            CancelSkill();
        }

        if (Input.GetKeyDown(KeyCode.Space) && GetCurrentTurn() == Allegiance.Player)
        {
            TryEndTurn();
        }

        // Show health bars of player units and active enemies.
        if (Input.GetKey(KeyCode.Tab))
		{
            foreach(Unit u in UnitsManager.m_Instance.m_PlayerUnits)
			{
                u.GetHealthBar().Reset();
			}
            foreach(Unit u in UnitsManager.m_Instance.m_ActiveEnemyUnits)
			{
                u.GetHealthBar().Reset();
            }
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

            // Clear the skill targeting highlights.
            foreach (Node n in m_maxSkillRange)
            {
                m_maxSkillRange.ForEach(m => m.m_NodeHighlight.m_IsAffected = false);
                m_maxSkillRange.ForEach(m => m.m_NodeHighlight.m_IsInTargetArea = false);
                n.m_NodeHighlight.ChangeHighlight(TileState.None);
            }

            m_SelectedUnit.HighlightMovableNodes();

            m_SelectedSkill = null;
        }
    }

    /// <summary>
    /// Select a skill.
    /// </summary>
    /// <param name="skill"> The skill being selected. </param>
    public void SkillSelection(BaseSkill skill, SkillButton button)
    {
        // Don't allow progress if the character is an enemy (player can mouse over for info, but not use the skill)
        if (m_SelectedUnit.GetAllegiance() == Allegiance.Enemy) return;

        // Make sure the player has a unit selected.
        if (m_SelectedUnit != null)
        {
            // Make sure the unit can afford to cast the skill and the skill isn't on cooldown before selecting it.
            // Just in case.
            if (m_SelectedUnit.GetActionPoints() >= skill.m_Cost && skill.GetCurrentCooldown() == 0)
            {
                foreach (SkillButton b in UIManager.m_Instance.m_SkillSlots)
                {
                    b.m_LightningImage.materialForRendering.SetFloat("_UIVerticalPan", 0);
                }
                button.m_LightningImage.materialForRendering.SetFloat("_UIVerticalPan", 1);
                // Reset the nodes in the old target range
                foreach (Node n in m_maxSkillRange)
                {
                    m_maxSkillRange.ForEach(m => m.m_NodeHighlight.m_IsAffected = false);
                    m_maxSkillRange.ForEach(m => m.m_NodeHighlight.m_IsInTargetArea = false);
                    n.m_NodeHighlight.ChangeHighlight(TileState.None);
                }

                // Update the GameManager's fields
                m_SelectedSkill = skill;
                m_TargetingState = TargetingState.Skill;

                // Get the new affectable area
                m_maxSkillRange = Grid.m_Instance.GetNodesWithinRadius(m_SelectedSkill.m_CastableDistance + m_SelectedSkill.m_AffectedRange, Grid.m_Instance.GetNode(m_SelectedUnit.transform.position), true);

                // Reset the highlight of movement nodes
                m_SelectedUnit.m_MovableNodes.ForEach(n => n.m_NodeHighlight.ChangeHighlight(TileState.None));

                // Tell the new nodes they're in range
                m_maxSkillRange.ForEach(n => n.m_NodeHighlight.m_IsInTargetArea = true);

                // Tell the appropriate nodes in distance (red) that they're in distance
                foreach (Node node in Grid.m_Instance.GetNodesWithinRadius(m_SelectedSkill.m_CastableDistance, Grid.m_Instance.GetNode(m_SelectedUnit.transform.position), true))
                {
                    switch (m_SelectedSkill.targetType)
                    {
                        case TargetType.SingleTarget:
                            node.m_NodeHighlight.m_IsTargetable = IsTargetable(m_SelectedUnit, node.unit, m_SelectedSkill);
                            break;
                        case TargetType.Line:
                            throw new NotImplementedException("Line target type not supported");
                        case TargetType.Terrain:
                            node.m_NodeHighlight.m_IsTargetable = true;
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Select a skill.
    /// </summary>
    /// <param name="skillNumber"> Index of the skill being selected. </param>
    public void SkillSelection(int skillNumber)
    {
        SkillSelection(m_SelectedUnit.m_LearnedSkills[skillNumber], UIManager.m_Instance.m_SkillSlots[skillNumber]);
    }

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
}
