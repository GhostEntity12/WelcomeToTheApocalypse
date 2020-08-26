using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HealthChangeIndicator : MonoBehaviour
{
    /// <summary>
    /// The starting position of the text box.
    /// </summary>
    private Vector3 m_FloatStartPosition = Vector3.zero;

    /// <summary>
    /// How high the text box will reach when it floats up.
    /// </summary>
    public float m_FloatEndHeight = 5.0f;

    /// <summary>
    /// The end position of the text box when it floats up.
    /// </summary>
    private Vector3 m_FloatEndPosition = Vector3.zero;

    /// <summary>
    /// Timer.
    /// </summary>
    private float m_Timer = 0.0f;

    /// <summary>
    /// The text in the text box.
    /// </summary>
    private TextMeshProUGUI m_TMPro = null;

    /// <summary>
    /// The colour of the text when health is increased.
    /// </summary>
    public Color m_IncreaseHealthColour = new Color();

    /// <summary>
    /// The colour of the text when health is decreased.
    /// </summary>
    public Color m_DecreaseHealthColour = new Color();

    /// <summary>
    /// The text with no alpha.
    /// To fade to.
    /// </summary>
    private Color m_NoAlpha = new Color(0,0,0,0);

    /// <summary>
    /// The current colour of the text.
    /// </summary>
    private Color m_CurrentColour = new Color();

    private UnitHealthBarCanvas m_HealthbarCanvas = null;

    public void Create()
    {
        Debug.Log("Awake");
        m_HealthbarCanvas = transform.parent.GetComponent<UnitHealthBarCanvas>();
        ;
        m_TMPro = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        //transform.position = Vector3.Lerp(m_FloatStartPosition, m_FloatEndPosition, m_Timer);
        m_TMPro.color = Color.Lerp(m_CurrentColour, m_NoAlpha, m_Timer);

        m_Timer += Time.deltaTime;
    }

    /// <summary>
    /// Reset the text box.
    /// </summary>
    [ContextMenu("Reset")]
    public void Reset()
    {
        transform.position = m_FloatStartPosition;
        m_Timer = 0.0f;
    }

    /// <summary>
    /// If health was increased.
    /// </summary>
    public void HealthIncreased()
    {
        LeanTween.moveY(gameObject, m_FloatStartPosition.y + m_FloatEndHeight, 1);

        m_NoAlpha = new Color(m_IncreaseHealthColour.r, m_IncreaseHealthColour.g, m_IncreaseHealthColour.b, 0.0f);
        m_CurrentColour = m_IncreaseHealthColour;
        m_TMPro.color = m_CurrentColour;
    }

    /// <summary>
    /// If health was decreased.
    /// </summary>
    public void HealthDecrease()
    {
        LeanTween.moveY(gameObject, m_FloatStartPosition.y + m_FloatEndHeight, 1);

        m_NoAlpha = new Color(m_DecreaseHealthColour.r, m_DecreaseHealthColour.g, m_DecreaseHealthColour.b, 0.0f);
        m_CurrentColour = m_DecreaseHealthColour;
        m_TMPro.color = m_CurrentColour;
    }

    public void SetStartPosition(Vector3 start)
    {
        m_FloatStartPosition = start;
        m_FloatEndPosition = new Vector3(m_FloatStartPosition.x, m_FloatStartPosition.y + m_FloatEndHeight, m_FloatStartPosition.z);
    }
}
