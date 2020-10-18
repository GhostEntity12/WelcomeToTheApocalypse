﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Passives/Death Passive")]
public class DeathPassive : PassiveSkill
{
    public int m_ExtraDamage = 5;

    private int m_AliveActiveEnemiesIter = 0;

    [SerializeField]
    private DeathPassiveStatusEffect m_PassiveStatusEffect = null;

    public override bool CheckPrecondition(TriggerType trigger, Unit affected)
    {
        // If the trigger being checked is the same as the trigger type of the passive.
        if (base.CheckPrecondition(trigger) == true)
        {
            foreach (Unit u in UnitsManager.m_Instance.m_ActiveEnemyUnits)
            {
                if (u.GetAlive() == true)
                {
                    ++m_AliveActiveEnemiesIter;
                }
            }
            // No alive active enemies, activate Death's passive.
            if (m_AliveActiveEnemiesIter == 0)
            {
                Debug.Log("Death Passive Activated!");
                affected.AddStatusEffect(m_PassiveStatusEffect);
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