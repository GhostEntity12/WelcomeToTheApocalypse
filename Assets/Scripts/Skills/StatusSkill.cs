using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Status Skill")]
public class StatusSkill : DamageSkill
{
	public InflictableStatus m_Effect;

	public override void CastSkill()
	{
		base.CastSkill();

		foreach (Unit u in affectedUnits)
		{
			// Create a copy of the effect and add that to the target, rather than adding a reference to the same effect for multiple targets.
			u.AddStatusEffect(Instantiate(m_Effect));
		}
	}
}
