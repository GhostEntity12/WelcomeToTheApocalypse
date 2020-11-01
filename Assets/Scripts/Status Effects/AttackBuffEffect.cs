using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Status Effects/Attack Buff Effect")]
public class AttackBuffEffect : InflictableStatus
{
	public int m_AttackIncrease = 0;

	public override bool CheckPrecondition(TriggerType trigger)
	{
		return trigger == m_TriggerType;
	}

	public override void TakeEffect(Unit affected)
	{
		affected.AddDealExtraDamage(m_AttackIncrease);
	}
}