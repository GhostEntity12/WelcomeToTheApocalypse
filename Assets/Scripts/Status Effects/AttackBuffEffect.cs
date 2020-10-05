using System.Collections;
using System.Collections.Generic;
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
		if (m_RemainingDuration > 0)
        {
            affected.AddExtraSkillDamage(m_AttackIncrease);
        }
        else
        {
            affected.RemoveStatusEffect(this);
        }
	}
}