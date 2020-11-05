using System.Linq;
using TMPro;
using UnityEngine;

public class StatusEffectTooltipManager : MonoBehaviour
{
	public static StatusEffectTooltipManager m_Instance;

	public StatusHolder m_PassiveEffect;
	public StatusHolder m_RagsToRichesEffect;
	public StatusHolder m_FaminesHungerEffect;
	public StatusHolder m_PestilencesMarkEffect;

	public TextMeshProUGUI m_PassiveName;
	public TextMeshProUGUI m_PassiveDescription;
	public TextMeshProUGUI m_PassiveStatus;

	public TextMeshProUGUI m_RagsToRichesDescription;
	public TextMeshProUGUI m_FaminesHungerDescription;
	public TextMeshProUGUI m_PestilencesMarkDescription;

	private void Awake()
	{
		m_Instance = this;
	}

	public void UpdateActiveEffects()
	{
		Unit selectedUnit = GameManager.m_Instance.GetSelectedUnit();

		UpdatePassive();

		if (selectedUnit.GetInflictableStatuses().OfType<AttackBuffEffect>().Any())
		{
			m_RagsToRichesEffect.gameObject.SetActive(true);
			AttackBuffEffect effect = selectedUnit.GetInflictableStatuses().OfType<AttackBuffEffect>().First();
			m_RagsToRichesDescription.text = effect.m_StatusDescription.Replace("{increase}", effect.m_AttackIncrease.ToString()).Replace("{duration}", effect.m_RemainingDuration.ToString());
			m_RagsToRichesEffect.m_StatusImageLight.sprite = effect.m_StatusIconLight;
			m_RagsToRichesEffect.m_StatusImageDark.sprite = effect.m_StatusIconDark;
			m_RagsToRichesEffect.m_Background.sprite = selectedUnit.m_UIData.m_Bust.m_PassiveBackground;
		}
		else m_RagsToRichesEffect.gameObject.SetActive(false);

		if (selectedUnit.GetInflictableStatuses().OfType<AttackDebuffEffect>().Any())
		{
			m_FaminesHungerEffect.gameObject.SetActive(true);
			AttackDebuffEffect effect = selectedUnit.GetInflictableStatuses().OfType<AttackDebuffEffect>().First();
			m_FaminesHungerDescription.text = effect.m_StatusDescription.Replace("{decrease}", effect.m_AttackDecrease.ToString()).Replace("{duration}", effect.m_RemainingDuration.ToString());
			m_FaminesHungerEffect.m_StatusImageLight.sprite = effect.m_StatusIconLight;
			m_FaminesHungerEffect.m_StatusImageDark.sprite = effect.m_StatusIconDark;
			m_FaminesHungerEffect.m_Background.sprite = selectedUnit.m_UIData.m_Bust.m_PassiveBackground;
		}
		else m_FaminesHungerEffect.gameObject.SetActive(false);

		if (selectedUnit.GetInflictableStatuses().OfType<DamageOverTimeEffect>().Any())
		{
			m_PestilencesMarkEffect.gameObject.SetActive(true);
			DamageOverTimeEffect effect = selectedUnit.GetInflictableStatuses().OfType<DamageOverTimeEffect>().First();
			m_PestilencesMarkDescription.text = effect.m_StatusDescription.Replace("{damage}", effect.m_DamageOverTime.ToString()).Replace("{duration}", effect.m_RemainingDuration.ToString());
			m_PestilencesMarkEffect.m_StatusImageLight.sprite = effect.m_StatusIconLight;
			m_PestilencesMarkEffect.m_StatusImageDark.sprite = effect.m_StatusIconDark;
			m_PestilencesMarkEffect.m_Background.sprite = selectedUnit.m_UIData.m_Bust.m_PassiveBackground;
		}
		else m_PestilencesMarkEffect.gameObject.SetActive(false);
	}

	public void UpdatePassive()
	{
		PassiveSkill passive = GameManager.m_Instance.GetSelectedUnit().GetPassiveSkill();
		if (passive)
		{
			m_PassiveName.text = passive.m_StatusName;
			m_PassiveDescription.text = passive.m_StatusDescription;
			switch (passive)
			{
				case DeathPassive dp:
					if (dp.m_PassiveStatusEffect.m_RemainingDuration > 0)
					{
						m_PassiveStatus.text = $"Currently receiving +{dp.m_PassiveStatusEffect.m_RemainingDuration} bonus damage";
					}
					else
					{
						m_PassiveStatus.text = $"{dp.m_StatusName} is inactive";
					}
					break;
				case PestilencePassive pp:
					int charges = Mathf.FloorToInt((float)pp.m_CurrentHealResource / pp.m_HealResourceCastCost);
					m_PassiveStatus.text = $"Pestilence has {charges} {(charges == 1 ? "charge" : "charges")} of Benign Infection remaining";
					break;
				default:
					break;
			}

			m_PassiveEffect.m_StatusImageLight.sprite = passive.m_StatusIconLight;
			m_PassiveEffect.m_StatusImageDark.sprite = passive.m_StatusIconDark;
			m_PassiveEffect.m_Background.sprite = GameManager.m_Instance.GetSelectedUnit().m_UIData.m_Bust.m_PassiveBackground;
		}
		m_PassiveEffect.gameObject.SetActive(passive);
	}

	public void ToggleTooltipVisibility(GameObject tooltip)
	{
		tooltip.SetActive(!tooltip.activeSelf);
	}
}
