using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Passives/Pestilence Passive")]
public class PestilencePassive : PassiveSkill
{
    private int m_HealResource = 0;

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
        m_HealResource += m_HealResourceForDealingDamage;
	}

    public int GetHealResource()
    {
        return m_HealResource;
    }

    public void UseHealResource()
    {
        m_HealResource -= m_HealResourceCastCost;
    }
}