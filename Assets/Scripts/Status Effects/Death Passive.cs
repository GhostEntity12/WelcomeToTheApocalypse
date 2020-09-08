using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Passives/Death Passive")]
public class DeathPassive : PassiveSkill
{
    public override bool CheckPrecondition(TriggerType trigger)
    {
        // If the trigger being checked is the same as the trigger type of the passive: trigger the passive effect.
        if (trigger == m_TriggerType)
        {
            Debug.Log("Death Passive triggered!");
            return true;
        }

        return false;
    }
}