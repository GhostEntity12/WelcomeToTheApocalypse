using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Heal Skill")]
public class HealSkill : BaseSkill
{
	public int m_HealAmount;

	public override void CastSkill()
	{
		base.CastSkill();
		// Heal each affected unit
	}
}
