using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float m_MoveSpeed = 3.0f;

    private Vector3 m_MovementInput = Vector3.zero;

    public Collider m_CameraLimits;

    private Bounds m_CameraBounds;

    private void Awake()
    {
        if (m_CameraLimits)
        {
            m_CameraBounds = m_CameraLimits.bounds;
        }
        else
        {
            m_CameraBounds = new Bounds(transform.position, Vector3.one);
            Debug.LogError("Missing collider to limit camera movement");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Get player input this frame.
        m_MovementInput += transform.right * Input.GetAxis("Horizontal");
        m_MovementInput += transform.forward * Input.GetAxis("Vertical");

        // Apply delta time.
        m_MovementInput *= Time.deltaTime;

        // Get the target position
        Vector3 targetPosition = transform.position + (m_MovementInput * m_MoveSpeed);

        // Clamp the position
        Vector3 clampedPosition = new Vector3();

        clampedPosition.x = Mathf.Clamp(targetPosition.x, m_CameraBounds.min.x, m_CameraBounds.max.x);
        clampedPosition.z = Mathf.Clamp(targetPosition.z, m_CameraBounds.min.z, m_CameraBounds.max.z);


        // Move the camera.
        transform.position = clampedPosition;
    }
}
