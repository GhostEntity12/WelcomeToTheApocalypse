using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageOverTimeEffect : InflictableStatus
{
    public int m_DamageOverTime = 0;

	public override bool CheckPrecondition(TriggerType trigger)
	{
		return trigger == m_TriggerType;
	}

	public override void TakeEffect(Unit affected)
	{
        if (m_RemainingDuration > 0)
        {
            affected.DecreaseCurrentHealth(m_DamageOverTime);
            m_RemainingDuration--;
        }
        else
        {
            affected.RemoveStatusEffect(this);
            ResetDuration();
        }		
	}
}