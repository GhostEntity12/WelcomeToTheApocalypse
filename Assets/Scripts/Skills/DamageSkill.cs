using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Damage Skill")]
public class DamageSkill : BaseSkill
{
	[Header("Skill Stats")]
	public int m_DamageAmount;

	public override void CastSkill()
	{
		base.CastSkill();
		// Damage each affected unit
	}
}
