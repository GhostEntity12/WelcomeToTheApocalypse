using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Passives/Death Passive")]
public class DeathPassive : PassiveSkill
{
    public int m_ExtraDamage = 5;

    public override bool CheckPrecondition(TriggerType trigger, Unit target)
    {
        // If the trigger being checked is the same as the trigger type of the passive.
        if (base.CheckPrecondition(trigger, target) == true)
        {
            if(target.GetCurrentHealth() == target.m_StartingHealth)
            {
                Debug.Log("Death Passive triggered!");
                return true;
            }
        }
        
        return false;
    }

    public override void TakeEffect(Unit target)
    {
        target.AddExtraDamage(m_ExtraDamage);
    }
}