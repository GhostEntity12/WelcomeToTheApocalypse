using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIHealthBar : MonoBehaviour
{
	public Image m_HealthBarBackground = null;
	public Image m_HealthBarFill = null;

	public GameObject m_HealthbarTooltip;
	TextMeshProUGUI m_HealthBarTooltipText;

	private void Awake()
	{
		m_HealthBarTooltipText = m_HealthbarTooltip.GetComponentInChildren<TextMeshProUGUI>();
	}

	public void SetHealthAmount(float health)
	{
		m_HealthBarFill.fillAmount = health;
	}

	private void Update()
	{
		if (m_HealthbarTooltip.activeSelf)
		{
			m_HealthBarTooltipText.text = $"{PlayerManager.m_Instance.GetSelectedUnit().GetCurrentHealth()}/{PlayerManager.m_Instance.GetSelectedUnit().GetStartingHealth()} HP";
		}
	}

	public void ToggleTooltip()
	{
		m_HealthbarTooltip.SetActive(!m_HealthbarTooltip.activeSelf);
	}
}