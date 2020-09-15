using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Passives/Death Passive")]
public class DeathPassive : PassiveSkill
{
    public int m_ExtraDamage = 5;

    public override bool CheckPrecondition(TriggerType trigger, Unit affected)
    {
        // If the trigger being checked is the same as the trigger type of the passive.
        if (base.CheckPrecondition(trigger) == true)
        {
            if(affected.GetCurrentHealth() == affected.m_StartingHealth)
            {
                Debug.Log("Death Passive triggered!");
                return true;
            }
        }
        
        return false;
    }

    public override void TakeEffect(Unit affected)
    {
        affected.AddExtraDamage(m_ExtraDamage);
    }
}