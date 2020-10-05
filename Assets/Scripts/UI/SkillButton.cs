using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillButton : MonoBehaviour
{
	public BaseSkill m_Skill;

	public Image m_BgImage;

	public Image m_LightImage;

	public Image m_DarkImage;

	public TextMeshProUGUI m_Description;

	public Image m_DescriptionImage;

	public void TriggerSkill() => GameManager.m_Instance.SkillSelection(m_Skill);

	void Awake()
	{
		m_DescriptionImage.gameObject.SetActive(false);
	}

	public void UpdateTooltip()
	{
		print(m_Skill);
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
			}
			catch (NullReferenceException)
			{
				Debug.LogError("Skill is lacking a required status!");
			}
		}
		baseDescription = baseDescription.Replace("{distance}", m_Skill.m_CastableDistance.ToString());
		baseDescription = baseDescription.Replace("{range}", m_Skill.m_AffectedRange.ToString());
		baseDescription = baseDescription.Replace("{cooldown}", m_Skill.m_CooldownLength.ToString());
		print(baseDescription);
		m_Description.text = baseDescription;
	}

	public void DisplayTooltip(bool show)
	{
		m_DescriptionImage.gameObject.SetActive(show);
	}
}
