using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillButton : MonoBehaviour
{
	public BaseSkill m_Skill;

	public Image m_Image;

	public TextMeshProUGUI m_Description;

	public void TriggerSkill() => GameManager.m_Instance.SkillSelection(m_Skill);

	public void UpdateTooltip()
	{
		print(m_Skill);
		m_Description.text = m_Skill.m_Description;
	}
}
