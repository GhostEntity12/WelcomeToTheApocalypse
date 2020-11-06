using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Damage Skill")]
public class DamageSkill : BaseSkill
{
	[Header("Skill Stats")]
	public int m_DamageAmount = 0;

	[HideInInspector]
	public int m_ExtraDamage = 0;

	public void AddExtraDamage(int extra)
	{
		m_ExtraDamage += extra;
	}
}
