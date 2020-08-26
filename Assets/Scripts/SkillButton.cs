using UnityEngine;

public class SkillButton : MonoBehaviour
{
	[SerializeField]
	int m_Index;

	public void TriggerSkill() => GameManager.m_Instance.SkillSelection(m_Index);
}
