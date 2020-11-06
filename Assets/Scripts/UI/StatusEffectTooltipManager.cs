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

	void SetupSkin(StatusHolder holder, StatusEffect status, Unit unit)
	{
		holder.m_StatusImageLight.sprite = status.m_StatusIconLight;
		holder.m_StatusImageDark.sprite = status.m_StatusIconDark;
		holder.m_StatusImageLight.color = unit.m_UIData.m_IconColors.m_IconLight;
		holder.m_StatusImageDark.color = unit.m_UIData.m_IconColors.m_IconDark;
		holder.m_Background.sprite = unit.m_UIData.m_Bust.m_PassiveBackground;
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
			SetupSkin(m_RagsToRichesEffect, effect, selectedUnit);
		}
		else m_RagsToRichesEffect.gameObject.SetActive(false);

		if (selectedUnit.GetInflictableStatuses().OfType<AttackDebuffEffect>().Any())
		{
			m_FaminesHungerEffect.gameObject.SetActive(true);
			AttackDebuffEffect effect = selectedUnit.GetInflictableStatuses().OfType<AttackDebuffEffect>().First();
			m_FaminesHungerDescription.text = effect.m_StatusDescription.Replace("{decrease}", effect.m_AttackDecrease.ToString()).Replace("{duration}", effect.m_RemainingDuration.ToString());
			SetupSkin(m_FaminesHungerEffect, effect, selectedUnit);
		}
		else m_FaminesHungerEffect.gameObject.SetActive(false);

		if (selectedUnit.GetInflictableStatuses().OfType<DamageOverTimeEffect>().Any())
		{
			m_PestilencesMarkEffect.gameObject.SetActive(true);
			DamageOverTimeEffect effect = selectedUnit.GetInflictableStatuses().OfType<DamageOverTimeEffect>().First();
			m_PestilencesMarkDescription.text = effect.m_StatusDescription.Replace("{damage}", effect.m_DamageOverTime.ToString()).Replace("{duration}", effect.m_RemainingDuration.ToString());
			SetupSkin(m_PestilencesMarkEffect, effect, selectedUnit);
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
					m_PassiveStatus.text = $"{charges} {(charges == 1 ? "charge" : "charges")} of Benign Infection remaining";
					break;
				default:
					m_PassiveStatus.text = string.Empty;
					break;
			}

			SetupSkin(m_PassiveEffect, passive, GameManager.m_Instance.GetSelectedUnit());
		}
		m_PassiveEffect.gameObject.SetActive(passive);
	}

	public void ToggleTooltipVisibility(GameObject tooltip)
	{
		tooltip.SetActive(!tooltip.activeSelf);
	}
}
