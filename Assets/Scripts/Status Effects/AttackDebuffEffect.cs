using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Status Effects/Attack Debuff Effect")]
public class AttackDebuffEffect : InflictableStatus
{
    public int m_AttackDecrease = 0;

	public override bool CheckPrecondition(TriggerType trigger)
	{
		return trigger == m_TriggerType;
	}

	public override void TakeEffect(Unit affected)
	{        
        affected.AddDealExtraDamage(-m_AttackDecrease);
	}
}