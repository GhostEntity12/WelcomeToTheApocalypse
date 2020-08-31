using UnityEngine;
using UnityEngine.UI;

public class SkillButton : MonoBehaviour
{
	[SerializeField]
	int m_Index;

	public BaseSkill m_Skill;

	public Image m_Image;

	public void TriggerSkill() => GameManager.m_Instance.SkillSelection(m_Skill);
}
