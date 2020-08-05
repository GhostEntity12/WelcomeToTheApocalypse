using Ghost;
using System.Collections.Generic;
using System.Linq;
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
    // Instance of the game manager.
    private static GameManager m_Instance = null;

    // Unit the player has selected.
    private Unit m_SelectedUnit = null;

    // The turn order.
    private Queue<Unit> m_TurnOrder = new Queue<Unit>();

    // Raycast for translating the mouse's screen position to world position.
    private RaycastHit m_MouseWorldRayHit = new RaycastHit();

    // Ray for raycasting the mouse's position.
    private Ray m_MouseRay = new Ray();

    // Reference to the main camera.
    private Camera m_MainCamera = null;

    private GameObject m_RayHitObject = null;

    // The action the player is targeting for.
    private TargetingState m_TargetingState = TargetingState.Move;

    // The skill the player is targeting for.
    private BaseSkill m_SelectedSkill = null;

    public KeyCode[] m_AbilityHotkeys = new KeyCode[3] { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3 };

	#region refactor me. PLEASE

	private Node m_CachedNode;

    List<Node> maxRange = new List<Node>();

	#endregion

	private void Awake()
    {
        m_MainCamera = Camera.main;
        m_MouseRay.origin = m_MainCamera.transform.position;
    }

    // Was FixedUpdate, but it was missing inputs - James L
    private void Update()
    {
        // Could be doing this with multiple casts and layer masks.
        // Implementation in commented out function below - James L
        Casting();

        Debug.DrawLine(m_MainCamera.transform.position, m_MouseWorldRayHit.point);
        //Debug.Log(m_MouseWorldRayHit.point);
    }

    public void NextTurn()
    {
        // Move the unit to the back of the turn order queue.
        m_TurnOrder.Enqueue(m_TurnOrder.Dequeue());
    }

    // Get instance of the game manager.
    public GameManager GetInstance()
    {
        if (m_Instance == null)
            m_Instance = new GameManager();

        return m_Instance;
    }

    public void Casting()
    {
        m_MouseRay = m_MainCamera.ScreenPointToRay(Input.mousePosition);

        #region refactor me. PLEASE

        if (m_TargetingState == TargetingState.Skill)
        {
            if (Physics.Raycast(m_MouseRay, out m_MouseWorldRayHit, Mathf.Infinity, 1 << 8))
            {
                if (Grid.m_Instance.GetNode(m_MouseWorldRayHit.transform.position) != m_CachedNode )
                {
                    m_CachedNode = Grid.m_Instance.GetNode(m_MouseWorldRayHit.transform.position);
                    if (m_CachedNode.m_NodeHighlight.m_isTargetable)
                    {
                        List<Node> targetableRange = GetNodesWithinRadius(m_SelectedSkill.m_AffectedRange, m_CachedNode);
                        foreach (Node node in maxRange)
                        {
                            node.m_NodeHighlight.m_isAffected = targetableRange.Contains(node);
                        }
                    }
                }
            }
        }

		#endregion

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
                        foreach (Node n in maxRange)
                        {
                            n.m_NodeHighlight.ChangeHighlight(TileState.None);
                        }
                    }

                    // Store the new unit
                    m_SelectedUnit = m_MouseWorldRayHit.transform.GetComponent<Unit>();
                    int highestMoveRange = m_SelectedUnit.GetSkills().Select(s => s.m_CastableDistance).Max() + m_SelectedUnit.GetSkills().Select(s => s.m_AffectedRange).Max();

                    print(highestMoveRange + m_SelectedUnit.m_StartingMovement);

                    maxRange = GetNodesWithinRadius(highestMoveRange + m_SelectedUnit.m_StartingMovement, Grid.m_Instance.GetNode(m_SelectedUnit.transform.position));
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
                            n.m_NodeHighlight.ChangeHighlight(TileState.None);
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
                m_TargetingState = TargetingState.None;
            }
        }
    }

    // Select a skill.
    public void SkillSelection(int skillNumber)
    {
        m_SelectedSkill = m_SelectedUnit.GetSkill(skillNumber);
        m_TargetingState = TargetingState.Skill;
        print(m_SelectedSkill.m_CastableDistance);
        foreach (NodeHighlight highlight in GetNodesWithinRadius(m_SelectedSkill.m_CastableDistance, Grid.m_Instance.GetNode(m_SelectedUnit.transform.position)).Select(n => n.m_NodeHighlight)) 
        {
            highlight.m_isTargetable = true;
        }
        Debug.Log(m_SelectedSkill.m_Description);
    }
}
