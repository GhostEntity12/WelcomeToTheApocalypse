using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Status Skill")]
public class StatusSkill : DamageSkill
{
	public InflictableStatus m_Effect;

	public override void CastSkill()
	{
		base.CastSkill();

		// Apply status/refresh if it already exists
	}
}
