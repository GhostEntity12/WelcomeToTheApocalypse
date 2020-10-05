using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Damage Skill")]
public class DamageSkill : BaseSkill
{
	[Header("Skill Stats")]
	public int m_DamageAmount = 0;

	private int m_ExtraDamage = 0;

	public override void CastSkill()
	{
		base.CastSkill();

		foreach (Unit unit in affectedUnits)
		{
			unit.DecreaseCurrentHealth(m_DamageAmount + m_ExtraDamage);
		}
		// Damage each affected unit
		m_ExtraDamage = 0;
	}

	public void AddExtraDamage(int extra)
	{
		m_ExtraDamage += extra;
	}
}
