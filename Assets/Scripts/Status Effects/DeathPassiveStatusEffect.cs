using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Status Effects/Death's Passive Effect")]
public class DeathPassiveStatusEffect : InflictableStatus
{
	public override bool CheckPrecondition(TriggerType trigger)
	{
		if(trigger == m_TriggerType)
            return true;
        else
            return false;
	}

	public override void TakeEffect(Unit affected)
	{
		affected.SetDealExtraDamage(m_RemainingDuration);
	}	
}
