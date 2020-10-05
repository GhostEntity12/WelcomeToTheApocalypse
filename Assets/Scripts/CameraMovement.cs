using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public CanvasGroup m_PixelScreen;
    private CanvasGroup m_BlackScreen;
    private bool m_IsSwapping;

    [Header("Movement")]
    public float m_AutoMoveSpeed;
    public Vector3? m_AutoMoveDestination;
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
    FacingDirection m_LookDirection;

    Camera m_Camera;

    bool m_CanPixel;

    enum FacingDirection
    {
        North, NorthEast, East, SouthEast, South, SouthWest, West, NorthWest
    }

    private void Awake()
    {
        m_CanPixel = m_PixelScreen != null;
        m_BlackScreen = UIManager.m_Instance.m_BlackScreen;
        m_Camera = Camera.main;
        if (m_CameraLimits)
        {
            m_CameraBounds = m_CameraLimits.bounds;
        }
        else
        {
            m_CameraBounds = new Bounds(transform.position, Vector3.one);
            Debug.LogError("Missing collider to limit camera movement");
        }
        // Snap to nearest 90 deg angle
        transform.rotation = Quaternion.Euler(0f, Mathf.Round(transform.eulerAngles.y / 45) * 45, 0f);
        // Get approx. angle to determine direction
        m_LookDirection = (FacingDirection)(int)(transform.eulerAngles.y / 45);

    }

    // Update is called once per frame
    void Update()
    {
        // Get player input this frame.
        m_MovementInput += transform.right * Input.GetAxis("Horizontal");
        m_MovementInput += transform.forward * Input.GetAxis("Vertical");

        if (m_MovementInput.magnitude != 0)
        {
            m_AutoMoveDestination = null;
        }
        else if (m_AutoMoveDestination != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, (Vector3)m_AutoMoveDestination, Mathf.Clamp(Vector3.Distance(transform.position, (Vector3)m_AutoMoveDestination) * m_AutoMoveSpeed * Time.deltaTime, 1f * Time.deltaTime, 50f * Time.deltaTime));
        }

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
                RotateLeft(0.4f);
            else if (Input.GetKeyDown(m_RotateRightKey))
                RotateRight(0.4f);
        }

        if (Input.GetKeyDown(KeyCode.F3) && !m_IsSwapping)
        {
            m_IsSwapping = true;
            SwapToFromIsoCam();
        }
        else if (Input.GetKeyDown(KeyCode.F4) && m_Camera.orthographic)
        {
            m_IsSwapping = true;
            SwapPixMode();
        }
    }

    void RotateLeft(float rotSpeed)
    {
        m_IsRotating = true;
        m_LookDirection = (FacingDirection)(((int)m_LookDirection + 9) % 8);
        LeanTween.rotate(gameObject, new Vector3(0, (int)m_LookDirection * 45f, 0), rotSpeed).setEase(rotationType).setOnComplete(() => m_IsRotating = false);
    }
    
    void RotateRight(float rotSpeed)
    {
        m_IsRotating = true;
        m_LookDirection = (FacingDirection)(((int)m_LookDirection + 7) % 8);
        LeanTween.rotate(gameObject, new Vector3(0, (int)m_LookDirection * 45f, 0), rotSpeed).setEase(rotationType).setOnComplete(() => m_IsRotating = false);
    }

    void SwapToFromIsoCam()
    {
        StartCoroutine(Ghost.Fade.FadeCanvasGroup(m_BlackScreen, 0.15f, 0, 1, () =>
                {
                    m_Camera.orthographic = !m_Camera.orthographic;
                    if ((int)m_LookDirection % 2 == 0 && m_Camera.orthographic)
                        RotateLeft(0.05f);
                    if (!m_Camera.orthographic && m_PixelScreen.alpha == 1)
                        SwapPixMode();
                    StartCoroutine(Ghost.Fade.FadeCanvasGroup(m_BlackScreen, 0.15f, 1, 0, 0f));
                    m_IsSwapping = false;
                }));
    }

    void SwapPixMode()
    {
        if (!m_CanPixel) return;

        bool b = m_PixelScreen.alpha == 0;
        StartCoroutine(Ghost.Fade.FadeCanvasGroup(m_PixelScreen, 0.15f, b ? 0 : 1, b ? 1 : 0, () => m_IsSwapping = false));
    }
}
