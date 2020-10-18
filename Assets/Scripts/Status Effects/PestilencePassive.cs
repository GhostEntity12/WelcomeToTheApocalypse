using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Passives/Pestilence Passive")]
public class PestilencePassive : PassiveSkill
{
    public int m_ExtraDamage = 0;

	public override bool CheckPrecondition(TriggerType trigger, Unit affected)
	{
		if (base.CheckPrecondition(trigger) == true)
        {
            if (affected.GetCurrentHealth() < affected.m_StartingHealth)
            {
                Debug.Log("Pestilence Passive triggered!");
                return true;
            }
        }

        return false;
	}

	public override void TakeEffect(Unit affected)
	{
        affected.AddTakeExtraDamage(m_ExtraDamage);
	}
}