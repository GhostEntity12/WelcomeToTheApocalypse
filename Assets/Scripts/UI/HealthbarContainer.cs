using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthbarContainer : MonoBehaviour
{
    /// <summary>
    /// The healthbar image.
    /// </summary>
    public Image m_HealthbarImage = null;

    /// <summary>
    /// The background healthbar image.
    /// </summary>
    private Image m_HealthbarBackImage = null;

    /// <summary>
    /// The text that indicates a change to the unit's health.
    /// </summary>
    public TextMeshProUGUI m_HealthChangeIndicator = null;

    /// <summary>
    /// Is the healthbar restricted to a unit?
    /// </summary>
    public bool m_IsMagnetic = false;

    /// <summary>
    /// The unit for this healthbar.
    /// </summary>
    public Unit m_Unit;

    /// <summary>
    /// The transform of the healthbar's position.
    /// </summary>
    Transform m_Transform;

    /// <summary>
    /// The main camera.
    /// </summary>
    Camera m_MainCam;

    private float m_Timer = 0.0f;

    private Color m_NoAlpha = new Color(0, 0, 0, 0);

    private Color m_FillColor;

    private Color m_BackColor;

    private void Awake()
    {
        m_FillColor = m_HealthbarImage.color;
        m_HealthbarBackImage = GetComponent<Image>();
        m_BackColor = m_HealthbarBackImage.color;
    }

    private void Start()
    {
        m_MainCam = Camera.main;
        if (m_Unit)
            m_Transform = m_Unit.m_HealthbarPosition;
        m_HealthChangeIndicator.GetComponent<HealthChangeIndicator>().Create();
    }

    public void SetChildrenActive(bool active)
    {
        for (int i = 0; i < transform.childCount; ++i)
        {
            transform.GetChild(i).gameObject.SetActive(active);
        }
    }

    private void Update()
    {
        if (m_IsMagnetic && m_HealthbarImage.color != m_NoAlpha) 
        {
            transform.position = m_MainCam.WorldToScreenPoint(m_Transform.position);
        }

        m_HealthbarImage.color = Color.Lerp(m_FillColor, m_NoAlpha, m_Timer);
        m_HealthbarBackImage.color = Color.Lerp(m_BackColor, m_NoAlpha, m_Timer);

        m_Timer += Time.deltaTime;
    }

    public void Reset()
    {
        m_Timer = 0.0f;
        m_HealthbarImage.color = m_FillColor;
        m_HealthbarBackImage.color = m_BackColor;
    }

    public void UnitSetHealthbar()
    {
        if (m_Unit != null)
            m_Unit.SetHealthbar(this);
    }

    public void SetUnit(Unit unit)
    {
        m_Unit = unit;
    }
}
