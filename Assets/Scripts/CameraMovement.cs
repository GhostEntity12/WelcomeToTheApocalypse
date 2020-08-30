using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [Header("Movement")]
    public float m_MoveSpeed = 3.0f;
    private Vector3 m_MovementInput = Vector3.zero;
    public Collider m_CameraLimits;
    private Bounds m_CameraBounds;

    [Header("Rotation")]
    public float m_RotationSpeed = 0.5f;
    public LeanTweenType rotationType;
    public KeyCode m_RotateLeftKey = KeyCode.Q;
    public KeyCode m_RotateRightKey = KeyCode.E;
    private bool m_IsRotating;
    FacingDirection m_LookDirection = FacingDirection.North;

    HealthbarContainer[] healthbarContainers;

    enum FacingDirection
    {
        North, East, South, West
    }

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

    private void Start()
    {
        healthbarContainers = FindObjectsOfType<HealthbarContainer>();
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
        Vector3 clampedPosition = new Vector3
        {
            x = Mathf.Clamp(targetPosition.x, m_CameraBounds.min.x, m_CameraBounds.max.x),
            z = Mathf.Clamp(targetPosition.z, m_CameraBounds.min.z, m_CameraBounds.max.z)
        };


        // Move the camera.
        transform.position = clampedPosition;

        if (!m_IsRotating)
        {
            if (Input.GetKeyDown(m_RotateLeftKey))
            {
                m_IsRotating = true;
                m_LookDirection = (FacingDirection)(((int)m_LookDirection + 5) % 4);
                LeanTween.rotate(gameObject, new Vector3(0, (int)m_LookDirection * 90f, 0), 0.4f).setEase(rotationType).setOnComplete(() => m_IsRotating = false);
            }
            else if (Input.GetKeyDown(m_RotateRightKey))
            {
                m_IsRotating = true;
                m_LookDirection = (FacingDirection)(((int)m_LookDirection + 3) % 4);
                LeanTween.rotate(gameObject, new Vector3(0, (int)m_LookDirection * 90f, 0), 0.4f).setEase(rotationType).setOnComplete(() => m_IsRotating = false);
            }
        }

        //foreach (HealthbarContainer hbc in healthbarContainers)
        //{
        //    hbc.m_IsMagnetic = m_IsRotating;
        //}
    }
}
