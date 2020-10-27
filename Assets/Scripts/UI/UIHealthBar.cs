using UnityEngine;
using UnityEngine.UI;

public class UIHealthBar : MonoBehaviour
{
    private Image m_HealthBarImage = null;

    void Awake()
    {
        m_HealthBarImage = GetComponent<Image>();
    }

    public void SetHealthAmount(float health)
    {
        m_HealthBarImage.fillAmount = health;
    }
}