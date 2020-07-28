using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Instance of the game manager.
    private static GameManager m_Instance = null;

    // Unit the player has selected.
    private Unit m_SelectedUnit = null;

    // The turn order.
    private Queue<Unit> m_TurnOrder = new Queue<Unit>();

    // Raycast for translating the mouse's screen position to world position.
    private RaycastHit m_MouseWorldPosition = new RaycastHit();

    // Ray for raycasting the mouse's position.
    private Ray m_MouseRay = new Ray();

    // Reference to the main camera.
    private Camera m_MainCamera = null;

    private void Awake()
    {
        m_MainCamera = Camera.main;
        m_MouseRay.origin = m_MainCamera.transform.position;
    }

    private void FixedUpdate()
    {
        // Get the position of where the player's mouse is pointing.
        m_MouseRay = m_MainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(m_MouseRay, out m_MouseWorldPosition))
        {
            // Mouse is over an object.

            Debug.DrawLine(m_MainCamera.transform.position, m_MouseWorldPosition.point);
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
}
