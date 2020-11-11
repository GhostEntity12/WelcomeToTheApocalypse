﻿using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillButton : MonoBehaviour
{
	public BaseSkill m_Skill;

	public Image m_SidesImage;

	public Image m_LightningImage;

	public Image m_CenterImage;

	public Image m_LightIcon;

	public Image m_DarkIcon;

	public Image m_TooltipImage;

	public Image m_Cooldown;

	public TextMeshProUGUI m_NameText;

	public TextMeshProUGUI m_DescriptionText;

	public TextMeshProUGUI m_CooldownText;

	public TextMeshProUGUI m_RangeText;

	public Transform m_ApSlots;

	public void TriggerSkill() => GameManager.m_Instance.SkillSelection(m_Skill, this);

	void Awake()
	{
		m_TooltipImage.gameObject.SetActive(false);
		m_LightningImage.material = new Material(m_LightningImage.material);
	}

	public void UpdateTooltip()
	{
		if (!m_Skill) return;
		string baseDescription = m_Skill.m_Description;
		if (m_Skill is DamageSkill)
		{
			baseDescription = baseDescription.Replace("{damage}", (m_Skill as DamageSkill).m_DamageAmount.ToString());
		}
		if (m_Skill is HealSkill)
		{
			baseDescription = baseDescription.Replace("{heal}", (m_Skill as HealSkill).m_HealAmount.ToString());
		}
		if (m_Skill is StatusSkill)
		{
			try
			{
				baseDescription = baseDescription.Replace("{duration}", (m_Skill as StatusSkill).m_Effect.m_StartingDuration.ToString());
				if ((m_Skill as StatusSkill).m_Effect is DamageOverTimeEffect)
				{
					baseDescription = baseDescription.Replace("{effectDamage}", ((m_Skill as StatusSkill).m_Effect as DamageOverTimeEffect).m_DamageOverTime.ToString());
				}
			}
			catch (NullReferenceException)
			{
				Debug.LogError("Skill is lacking a required status!");
			}
		}
		baseDescription = baseDescription.Replace("{distance}", m_Skill.m_CastableDistance.ToString()).Replace("{range}", m_Skill.m_AffectedRange.ToString());

		m_NameText.text = m_Skill.m_SkillName;

		m_CooldownText.text = $"<smallcaps><b>Cooldown</b></smallcaps><size=14>\n{(m_Skill.m_CooldownLength == 0 ? "None" : $"{m_Skill.m_CooldownLength} {(m_Skill.m_CooldownLength == 1 ? "turn" : "turns")}")}";

		m_RangeText.text = $"<smallcaps><b>Range</b></smallcaps><size=14>\n{m_Skill.m_CastableDistance} {(m_Skill.m_CastableDistance == 1 ? "tile" : "tiles")}";

		for (int i = 0; i < m_ApSlots.childCount; i++)
		{
			m_ApSlots.GetChild(i).gameObject.SetActive(i < (int)m_Skill.m_SkillType);
		}

		m_DescriptionText.text = baseDescription;
	}

	public void DisplayTooltip(bool show)
	{
		m_TooltipImage.gameObject.SetActive(show);
	}

	public void UpdateCooldownDisplay()
	{
		if (m_Skill)
		{
			m_Cooldown.fillAmount =
				m_Skill.GetCurrentCooldown() > 0 ? // Partially filled if on cooldown
					(float)m_Skill.m_CurrentCooldown / m_Skill.m_CooldownLength :
				m_Skill.m_Cost > GameManager.m_Instance.GetSelectedUnit().GetActionPoints() ? // Filled if AP cost is too high
					1 :
				0; // Empty otherwise
			PassiveSkill passive = GameManager.m_Instance.GetSelectedUnit().GetPassiveSkill();
			if (m_Skill.m_SkillName == "Benign Infection" &&
				passive is PestilencePassive &&
				(passive as PestilencePassive).m_CurrentHealResource == 0)
			{
				m_Cooldown.fillAmount = 1;
			}
		}
	}
}
