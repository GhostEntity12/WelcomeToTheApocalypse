using System.Collections.Generic;
using UnityEngine;

using static Ghost.BFS;

public enum TargetingState
{
    // Player is currently selecting a node for movement.
    Move,
    // Player is currently selecting a node for attacking.
    Attack,
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

    private TargetingState m_TargetingState = TargetingState.Move;

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

        //Casting();

        // Get the position of where the player's mouse is pointing.
        m_MouseRay = m_MainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(m_MouseRay, out m_MouseWorldRayHit))
        {
            // Mouse is over an object.
            m_RayHitObject = m_MouseWorldRayHit.collider.gameObject;

            if (m_RayHitObject.CompareTag("Tile"))
            {
                // Player selected a tile.
                if (Input.GetMouseButtonDown(0))
                {
                    // If the player is selecting a tile to move unit to.
                    if (m_TargetingState == TargetingState.Move)
                    {
                        Node target = Grid.m_Instance.GetNode(m_RayHitObject.transform.position);
                        m_SelectedUnit.SetTargetPosition(target.worldPosition);
                    }
                }
            }

            Debug.DrawLine(m_MainCamera.transform.position, m_MouseWorldRayHit.point);
        }
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

    // Some of this feels like it's structured badly. Might want to discuss the architecture of this one. - James L
    public void Casting()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray mousePointRay = m_MainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo = new RaycastHit();

            if (m_TargetingState == TargetingState.Attack && Physics.Raycast(mousePointRay, out hitInfo, Mathf.Infinity, 1 << 8)) // Hit a tile and is attacking
            {
                // If hit tile is in affectable range,
                // Do attack

                // else return;
            }
            else if (Physics.Raycast(mousePointRay, out hitInfo, Mathf.Infinity, 1 << 9)) // Hit a character
            {
                // Reset the nodes highlights before selecting the new unit
                if (m_SelectedUnit)
                {
                    foreach (Node n in m_SelectedUnit.m_MovableNodes)
                    {
                        n.tile.SetActive(false); // Only SetActive() for now. Will need to be changed to handle different types of highlights
                    }
                }

                // Store the new unit
                m_SelectedUnit = hitInfo.transform.GetComponent<Unit>();
                m_TargetingState = TargetingState.Move;

                // Highlight the appropriate tiles
                m_SelectedUnit.HighlightMovableNodes();

            }
            else if (m_TargetingState == TargetingState.Move && Physics.Raycast(mousePointRay, out hitInfo, Mathf.Infinity, 1 << 8)) // Hit a tile and is in the move state
            {
                Node target = Grid.m_Instance.GetNode(hitInfo.transform.position);
                if (m_SelectedUnit.m_MovableNodes.Contains(target))
                {
                    // Clear the previously highlighted tiles
                    foreach (Node n in m_SelectedUnit.m_MovableNodes)
                    {
                        n.tile.SetActive(false); // Only SetActive() for now. Will need to be changed to handle different types of highlights
                    }

                    m_SelectedUnit.SetTargetPosition(target.worldPosition);

                    // Should remove the required movement here

                    // Should we do this after the unit has finished moving? - James L
                    m_SelectedUnit.HighlightMovableNodes(target);
                }
            }
        }
    }
}
