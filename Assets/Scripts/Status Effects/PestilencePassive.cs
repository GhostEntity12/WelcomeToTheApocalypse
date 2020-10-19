﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Passives/Pestilence Passive")]
public class PestilencePassive : PassiveSkill
{
    [SerializeField]
    private int m_StartingHealResource = 0;

    [SerializeField]
    private int m_CurrentHealResource = 0;

    [SerializeField]
    private int m_HealResourceForDealingDamage = 0;

    [SerializeField]
    private int m_HealResourceCastCost = 0;

	public override bool CheckPrecondition(TriggerType trigger)
	{
		if (base.CheckPrecondition(trigger) == true)
        {
            return true;
        }

        return false;
	}

	public override void TakeEffect()
	{
        m_CurrentHealResource += m_HealResourceForDealingDamage;
	}

    public int GetHealResource()
    {
        return m_CurrentHealResource;
    }

    public void UseHealResource()
    {
        m_CurrentHealResource -= m_HealResourceCastCost;
    }
}