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
    private Unit m_SelectedUnit = null;

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
    private KeyCode[] m_AbilityHotkeys = new KeyCode[3] { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3 };

    /// <summary>
    /// Which team's turn is it currently.
    /// </summary>
    private Allegiance m_TeamCurrentTurn = Allegiance.Player;

    /// <summary>
    /// List of the units in currently "in combat".
    /// </summary>
    public List<Unit> m_UnitsInCombat = new List<Unit>();

    /// <summary>
    /// The cost of the player's unit moving to their target location.
    /// </summary>
    private int m_MovementCost = 0;

    /// <summary>
    /// List of the unit's the player controls.
    /// </summary>
    public List<Unit> m_PlayerUnits = new List<Unit>();

    /// <summary>
    /// The screen for when the player loses.
    /// </summary>
    public Canvas m_LoseScreen = null;

    private bool m_LeftMouseDown = false;

    #region refactor me. PLEASE

    private Node m_CachedNode;

    private List<Node> m_maxSkillRange = new List<Node>();

    private DialogueManager dm;

    #endregion

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
        dm = DialogueManager.instance;

        // Get all the unit's the player controls.
        foreach(Unit u in GameObject.FindObjectsOfType<Unit>())
        {
            if (u.GetAllegiance() == Allegiance.Player)
                m_PlayerUnits.Add(u);
        }
    }

    // Update.
    private void Update()
    {
        // If it's currently the player's turn, check their inputs.
        // Commented out for debugging.
        //if (m_CurrentTurn == Allegiance.Player)
        //if (!dm.dialogueActive)
        //{
            PlayerInputs();
        //}

        Debug.DrawLine(m_MainCamera.transform.position, m_MouseWorldRayHit.point);
        //Debug.Log(m_MouseWorldRayHit.point);
    }

    /// <summary>
    /// Get the unit currently selected by the player.
    /// </summary>
    /// <returns> The unit the player has selected. </returns>
    public Unit GetSelectedUnit() { return m_SelectedUnit; }

    /// <summary>
    /// End the current turn.
    /// </summary>
    public void EndCurrentTurn()
    {
        // Player ends turn.
        if (m_TeamCurrentTurn == Allegiance.Player)
        {
            m_TeamCurrentTurn = Allegiance.Enemy;

            // Stop highlighting node's the player can move to.
            foreach (Node n in m_SelectedUnit?.m_MovableNodes)
            {
                n.m_NodeHighlight.ChangeHighlight(TileState.None);
            }
            // Deselect unit.
            m_SelectedUnit = null;
        }
        // Enemy ends turn.
        else
        {
            m_TeamCurrentTurn = Allegiance.Player;

            // Reset the action points of the player's units.
            foreach(Unit u in m_PlayerUnits)
            {
                u.ResetActionPoints();
            }
        }

        // Reset the movement of the units whos turn it now is.
        foreach (Unit u in m_UnitsInCombat)
        {
            if (u.GetAllegiance() == m_TeamCurrentTurn)
                u.ResetCurrentMovement();
        }

        //Debug.Log(m_TeamCurrentTurn);
        UIManager.m_Instance.SlideSkills(UIManager.ScreenState.Offscreen);
    }

    /// <summary>
    /// Add a list of enemies to the turn order.
    /// </summary>
    public void AddToTurnOrder(List<Unit> add)
    {
        foreach (Unit u in add)
        {
            m_UnitsInCombat.Add(u);
        }
    }

    /// <summary>
    /// Get's the player's inputs.
    /// </summary>
    public void PlayerInputs()
    {
        m_MouseRay = m_MainCamera.ScreenPointToRay(Input.mousePosition);

        m_LeftMouseDown = Input.GetMouseButtonDown(0);
        
        // Mouse is over a unit.
        if (Physics.Raycast(m_MouseRay, out m_MouseWorldRayHit, Mathf.Infinity, 1 << 9))
        {
            Unit rayHitUnit = m_MouseWorldRayHit.transform.GetComponent<Unit>();
            // Check if the player is selecting another character.
            if (m_TargetingState == TargetingState.Move)
            {
                // Check player input.
                if (m_LeftMouseDown)
                {
                    // If the unit the player is hovering over isn't the selected unit and the unit is on the player's side.
                    // Select that unit.
                    if (rayHitUnit != m_SelectedUnit && rayHitUnit.GetAllegiance() == m_TeamCurrentTurn) // TODO: revert to only player select
                    {                        
                        // Reset the nodes highlights before selecting the new unit
                        m_maxSkillRange.ForEach(s => s.m_NodeHighlight.m_IsInTargetArea = false);
                        m_SelectedUnit?.m_MovableNodes.ForEach(u => u.m_NodeHighlight.ChangeHighlight(TileState.None));

                        // Store the new unit
                        m_SelectedUnit = rayHitUnit;
                        UIManager.m_Instance.SwapUI(UIManager.m_Instance.GetUIStyle(m_SelectedUnit));

                        // Highlight the appropriate tiles
                        m_SelectedUnit.m_MovableNodes = Grid.m_Instance.GetNodesWithinRadius(m_SelectedUnit.GetCurrentMovement(), Grid.m_Instance.GetNode(m_SelectedUnit.transform.position));
                        //print(m_SelectedUnit.m_MovableNodes.Count);
                        m_SelectedUnit.HighlightMovableNodes();
                    }
                }
            }
            //Check if the player is casting a skill on a unit.
            else if (m_TargetingState == TargetingState.Skill)
            {
                // Check player input.
                if (m_LeftMouseDown)
                {
                    // Cast the skill the player has selected.
                    // If hit unit is in affectable range,
                    Node unitNode = Grid.m_Instance.GetNode(rayHitUnit.transform.position);
                    if (unitNode.m_NodeHighlight.m_IsTargetable)
                    {
                        if (m_SelectedUnit.GetActionPoints() >= m_SelectedSkill.m_Cost)
                        {
                            m_SelectedSkill.affectedNodes = Grid.m_Instance.GetNodesWithinRadius(m_SelectedSkill.m_AffectedRange, unitNode, true);
                            m_SelectedUnit.ActivateSkill(m_SelectedSkill);
                            m_SelectedUnit.DecreaseActionPoints(m_SelectedSkill.m_Cost);
                            Debug.Log(m_SelectedUnit.GetActionPoints());

                            // Now deselect the skill and clear the targeting highlights.
                            m_TargetingState = TargetingState.Move;

                            foreach (Node n in m_maxSkillRange)
                            {
                                m_maxSkillRange.ForEach(m => m.m_NodeHighlight.m_IsAffected = false);
                                m_maxSkillRange.ForEach(m => m.m_NodeHighlight.m_IsInTargetArea = false);
                                n.m_NodeHighlight.ChangeHighlight(TileState.None);
                            }
            
                            m_SelectedUnit.HighlightMovableNodes();
            
                            m_SelectedSkill = null;
                        }
                        else
                        {
                            Debug.Log("Not enough action points!");
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
                if (Grid.m_Instance.GetNode(m_MouseWorldRayHit.transform.position) != m_CachedNode)
                {
                    // Update the cache
                    m_CachedNode = Grid.m_Instance.GetNode(m_MouseWorldRayHit.transform.position);
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
                if (m_LeftMouseDown)
                {
                    // Cast the skill the player has selected.
                    // If hit tile is in affectable range,
                    if (hitNode.m_NodeHighlight.m_IsTargetable)
                    {
                        if (m_SelectedUnit.GetActionPoints() >= m_SelectedSkill.m_Cost)
                        {
                            m_SelectedSkill.affectedNodes = Grid.m_Instance.GetNodesWithinRadius(m_SelectedSkill.m_AffectedRange, hitNode, true);
                            m_SelectedUnit.ActivateSkill(m_SelectedSkill);
                            m_SelectedUnit.DecreaseActionPoints(m_SelectedSkill.m_Cost);
                            Debug.Log(m_SelectedUnit.GetActionPoints());

                            // Now deselect the skill and clear the targeting highlights.
                            m_TargetingState = TargetingState.Move;

                            foreach (Node n in m_maxSkillRange)
                            {
                                m_maxSkillRange.ForEach(m => m.m_NodeHighlight.m_IsAffected = false);
                                m_maxSkillRange.ForEach(m => m.m_NodeHighlight.m_IsInTargetArea = false);
                                n.m_NodeHighlight.ChangeHighlight(TileState.None);
                            }
            
                            m_SelectedUnit.HighlightMovableNodes();
            
                            m_SelectedSkill = null;
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
                if (m_SelectedUnit != null)
                {
                    // Check input.
                    if (m_LeftMouseDown)
                    {
                        if (m_SelectedUnit.m_MovableNodes.Contains(hitNode))
                        {
                            // Clear the previously highlighted tiles
                            foreach (Node n in m_SelectedUnit.m_MovableNodes)
                            {
                                n.m_NodeHighlight.ChangeHighlight(TileState.None);
                            }
                            Stack<Node> path = new Stack<Node>();
                            if (Grid.m_Instance.FindPath(m_SelectedUnit.transform.position, m_MouseWorldRayHit.transform.position, ref path, out m_MovementCost))
                            {
                                m_SelectedUnit.SetMovementPath(path);
                                // Decrease the unit's movement by the cost.
                                //- 1 because the cost it gets is the number of nodes in the path, which includes the node the unit starts on.
                                m_SelectedUnit.DecreaseCurrentMovement(m_MovementCost - 1);
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
                // Make sure the unit can afford to cast the skill before selecting it.
                if (m_SelectedUnit.GetActionPoints() >= m_SelectedUnit.GetSkill(i).m_Cost)
                {
                    SkillSelection(i);
                    m_TargetingState = TargetingState.Skill;
                    break;
                }
                else
                {
                    Debug.Log("You don't have enough action points to select this skill!");
                }
            }
        }
        
        // Cancelling skill targeting.
        if (Input.GetMouseButtonDown(1))
        {
            if (m_TargetingState == TargetingState.Skill)
            {
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

        if (Input.GetKeyDown(KeyCode.Space))
        {
            EndCurrentTurn();
        }
    }

    /// <summary>
    /// Select a skill.
    /// </summary>
    /// <param name="skillNumber"> Index of the skill being selected. </param>
    public void SkillSelection(int skillNumber)
    {
        // TODO: replace so the buttons just can't be clicked.
        // if (m_SelectedUnit.GetAllegiance() == Allegiance.Enemy) return;
        // Reset the nodes in the old target range
        m_maxSkillRange.ForEach(n => n.m_NodeHighlight.m_IsInTargetArea = false);

        // Update the GameManager's fields
        m_SelectedSkill = m_SelectedUnit.GetSkill(skillNumber);
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

        //Debug.Log(m_SelectedSkill.m_Description);
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
    public void CheckPlayerUnitsAlive()
    {
        // The number of units alive.
        int alive = 0;

        // Go through all the player's units, and check how many are alive.
        foreach(Unit u in m_PlayerUnits)
        {
            if (u.GetAlive())
            {
                alive++;
            }
        }

        // If true, all player units are dead.
        if (alive == 0)
        {
            // All the player's units are dead, the player lost.
            // Pause the game and display the lose screen for the player.
            Debug.Log("Everybody's dead, everybody's dead Dave!");

            Time.timeScale = 0.0f;
            m_LoseScreen.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Get the allegiance of which team's turn it currently is.
    /// </summary>
    /// <returns>The allegiance of the team whose turn it currently is.</returns>
    public Allegiance GetCurrentTurn() { return m_TeamCurrentTurn; }

    public static void CreateVersionText()
    {
        if (GameObject.Find("VersionCanvas")) return;

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
