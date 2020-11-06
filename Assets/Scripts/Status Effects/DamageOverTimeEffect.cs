using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Status Effects/Damage Over Time Effect")]
public class DamageOverTimeEffect : InflictableStatus
{
	public int m_DamageOverTime = 0;

	public override bool CheckPrecondition(TriggerType trigger)
	{
		return trigger == m_TriggerType;
	}

	public override void TakeEffect(Unit affected)
	{
		affected.IncomingNonSkillDamage(m_DamageOverTime);
	}
}