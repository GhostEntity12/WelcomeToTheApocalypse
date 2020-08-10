using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Status Skill")]
public class StatusSkill : DamageSkill
{
	public InflictableStatus m_Effect;

	public override void CastSkill()
	{
		base.CastSkill();

		foreach(Unit u in affectedUnits)
		{
			u.AddStatusEffect(m_Effect);
		}
	}
}
