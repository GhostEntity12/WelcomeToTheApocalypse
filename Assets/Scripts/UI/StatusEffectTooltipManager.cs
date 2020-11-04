using System.Linq;
using TMPro;
using UnityEngine;

public class StatusEffectTooltipManager : MonoBehaviour
{
	public static StatusEffectTooltipManager m_Instance;

	public GameObject m_PassiveEffect;
	public GameObject m_RagsToRichesEffect;
	public GameObject m_FaminesHungerEffect;
	public GameObject m_PestilencesMarkEffect;

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
			m_RagsToRichesEffect.SetActive(true);
			AttackBuffEffect effect = selectedUnit.GetInflictableStatuses().OfType<AttackBuffEffect>().First();
			m_RagsToRichesDescription.text = effect.m_StatusDescription.Replace("{increase}", effect.m_AttackIncrease.ToString()).Replace("{duration}", effect.m_RemainingDuration.ToString());
		}
		else m_RagsToRichesEffect.SetActive(false);

		if (selectedUnit.GetInflictableStatuses().OfType<AttackDebuffEffect>().Any())
		{
			m_FaminesHungerEffect.SetActive(true);
			AttackDebuffEffect effect = selectedUnit.GetInflictableStatuses().OfType<AttackDebuffEffect>().First();
			m_FaminesHungerDescription.text = effect.m_StatusDescription.Replace("{decrease}", effect.m_AttackDecrease.ToString()).Replace("{duration}", effect.m_RemainingDuration.ToString());
		}
		else m_FaminesHungerEffect.SetActive(false);

		if (selectedUnit.GetInflictableStatuses().OfType<DamageOverTimeEffect>().Any())
		{
			m_PestilencesMarkEffect.SetActive(true);
			DamageOverTimeEffect effect = selectedUnit.GetInflictableStatuses().OfType<DamageOverTimeEffect>().First();
			m_PestilencesMarkDescription.text = effect.m_StatusDescription.Replace("{damage}", effect.m_DamageOverTime.ToString()).Replace("{duration}", effect.m_RemainingDuration.ToString());
		}
		else m_PestilencesMarkEffect.SetActive(false);
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
					m_PassiveStatus.text = $"Pestilence has {charges} charge{(charges == 1 ? "" : "s")} of Benign Infection remaining";
					break;
				default:
					break;
			}
		}
		m_PassiveEffect.SetActive(passive);
	}

	public void ToggleTooltipVisibility(GameObject tooltip)
	{
		tooltip.SetActive(!tooltip.activeSelf);
	}
}
