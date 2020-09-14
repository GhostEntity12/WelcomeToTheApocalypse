using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Damage Skill")]
public class DamageSkill : BaseSkill
{
	[Header("Skill Stats")]
	public int m_DamageAmount;

	public override void CastSkill()
	{
		base.CastSkill();

		foreach (Unit unit in affectedUnits)
		{
			unit.DecreaseCurrentHealth(m_DamageAmount);
		}
		// Damage each affected unit

	}
}
