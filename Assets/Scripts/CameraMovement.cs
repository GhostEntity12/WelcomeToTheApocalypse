using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float m_MoveSpeed = 3.0f;

    public BoxCollider m_Bounds;

    private Vector3 m_MovementInput = Vector3.zero;

    // Update is called once per frame
    void Update()
    {
        // Get player input this frame.
        m_MovementInput += transform.right * Input.GetAxis("Horizontal");
        m_MovementInput += transform.forward * Input.GetAxis("Vertical");

        // Apply delta time.
        m_MovementInput *= Time.deltaTime;

        // Move the camera.
        transform.position += (m_MovementInput * m_MoveSpeed);
    }
}
