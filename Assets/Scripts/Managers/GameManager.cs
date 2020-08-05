using System.Collections.Generic;
using UnityEngine;

using static Ghost.BFS;

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

    // On startup.
    private void Awake()
    {
        m_MainCamera = Camera.main;
        m_MouseRay.origin = m_MainCamera.transform.position;
    }

    // Update.
    private void Update()
    {
        // If it's currently the player's turn, check their inputs.
        // Commented out for debugging.
        //if (m_CurrentTurn == Allegiance.Player)
            PlayerInputs();

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
            foreach (Node n in m_SelectedUnit.m_MovableNodes)
            {
                n.m_tile.SetActive(false);
            }
            // Deselect unit.
            m_SelectedUnit = null;
        }
        // Enemy ends turn.
        else
            m_TeamCurrentTurn = Allegiance.Player;

        // Reset the movement of the units whos turn it now is.
        foreach(Unit u in m_UnitsInCombat)
        {
            if (u.GetAllegiance() == m_TeamCurrentTurn)
                u.ResetCurrentMovement();
        }

        Debug.Log(m_TeamCurrentTurn);
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

        // Raycast hit a character, check for what the player can do with characters.
        if (Physics.Raycast(m_MouseRay, out m_MouseWorldRayHit, Mathf.Infinity, 1 << 9))
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (m_TargetingState != TargetingState.Skill)
                {
                    // Reset the nodes highlights before selecting the new unit
                    if (m_SelectedUnit)
                    {
                        foreach (Node n in m_SelectedUnit.m_MovableNodes)
                        {
                            //n.m_tile.SetActive(false); // Only SetActive() for now. Will need to be changed to handle different types of highlights
                        }
                    }

                    // Store the new unit
                    m_SelectedUnit = m_MouseWorldRayHit.transform.GetComponent<Unit>();
                    m_TargetingState = TargetingState.Move;

                    // Highlight the appropriate tiles
                    m_SelectedUnit.m_MovableNodes = GetNodesWithinRadius(m_SelectedUnit.GetCurrentMovement(), Grid.m_Instance.GetNode(m_SelectedUnit.transform.position));
                    m_SelectedUnit.HighlightMovableNodes();
                    Debug.Log(m_SelectedUnit.name);
                }
            }
        }

        // Raycast hit a tile, check for what the player can do with tiles.
        else if (Physics.Raycast(m_MouseRay, out m_MouseWorldRayHit, Mathf.Infinity, 1 << 8))
        {
            if (Input.GetMouseButtonDown(0))
            {
                // Select node to move to.
                if (m_TargetingState == TargetingState.Move)
                {
                    Node target = Grid.m_Instance.GetNode(m_MouseWorldRayHit.transform.position);
                    if (m_SelectedUnit.m_MovableNodes.Contains(target))
                    {
                        // Clear the previously highlighted tiles
                        foreach (Node n in m_SelectedUnit.m_MovableNodes)
                        {
                            n.m_tile.SetActive(false); // Only SetActive() for now. Will need to be changed to handle different types of highlights
                        }

                        Stack<Node> path = new Stack<Node>();
                        if (Grid.m_Instance.FindPath(m_SelectedUnit.transform.position, m_MouseWorldRayHit.transform.position, ref path))
                        {
                            m_SelectedUnit.SetMovementPath(path);
                            m_SelectedUnit.DecreaseCurrentMovement(m_SelectedUnit.GetMovementPath().Count);
                        }

                        // Should we do this after the unit has finished moving? - James L
                        m_SelectedUnit.HighlightMovableNodes(target);
                    }
                }

                // Select character to use a skill on.
                else if (m_TargetingState == TargetingState.Skill)
                {
                    // If hit tile is in affectable range,
                    m_SelectedUnit.ActivateSkill(m_SelectedSkill);

                    // else return;
                }
            }
        }

        // Selecting a skill with the number keys.
        for (int i = 0; i < m_AbilityHotkeys.Length; i++)
        {
            if (Input.GetKeyDown(m_AbilityHotkeys[i]))
            {
                SkillSelection(i);
                m_TargetingState = TargetingState.Skill;
                break;
            }
        }

        // Cancelling skill targeting.
        if (Input.GetMouseButtonDown(1))
        {
            if (m_TargetingState == TargetingState.Skill)
            {
                m_TargetingState = TargetingState.Move;
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
        m_SelectedSkill = m_SelectedUnit.GetSkill(skillNumber);
        m_TargetingState = TargetingState.Skill;
        Debug.Log(m_SelectedSkill.m_Description);
    }
}
