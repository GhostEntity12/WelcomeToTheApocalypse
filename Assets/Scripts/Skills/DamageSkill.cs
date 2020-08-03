using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Damage Skill")]
public class DamageSkill : BaseSkill
{
	public int m_DamageAmount;

	public override void CastSkill()
	{
		base.CastSkill();
		// Damage each affected unit
	}
}
