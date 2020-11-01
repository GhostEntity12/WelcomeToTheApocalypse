using UnityEngine;
using UnityEngine.UI;

public class UIHealthBar : MonoBehaviour
{
    public Image m_HealthBarBackground = null;
    public Image m_HealthBarFill = null;

    public void SetHealthAmount(float health)
    {
        m_HealthBarFill.fillAmount = health;
    }
}