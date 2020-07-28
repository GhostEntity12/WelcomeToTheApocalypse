using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float m_MoveSpeed = 3.0f;

    public Bounds m_Bounds;

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

        // Check bounds.

        // Along z.
        // Outside max z.
        if (transform.position.z > m_Bounds.extents.z)
            transform.position = new Vector3(transform.position.x, transform.position.y, m_Bounds.max.z);
        // Outside min z.
        //else if (transform.position.z < m_Bounds.extents.z)
        //    transform.position = new Vector3(transform.position.x, transform.position.y, m_Bounds.min.z);

        // Along x.
        // Outside max x.
        if (transform.position.x > m_Bounds.extents.x)
            transform.position = new Vector3(m_Bounds.min.x, transform.position.y, transform.position.z);
        // Outside min x.
        //else if (transform.position.x < m_Bounds.extents.x)
        //    transform.position = new Vector3(m_Bounds.min.x, transform.position.y, transform.position.z);
    }
}
