using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TriggerType
{
    // Trigger at the start of the character's turn.
    OnTurnStart,
    // Trigger when the character deals damage to something.
    OnDealDamage,
    // Trigger when the character takes damage.
    OnTakeDamage
}

public abstract class StatusEffect : ScriptableObject
{
    // How this status effect is triggered.
    public TriggerType m_TriggerType;

    // Icon for the status effect.
    public Sprite m_StatusIcon;

    // Description of the status effect.
    [TextArea(1, 5)]
    public string m_StatusDescription;

    public bool m_AffectSelf = false;

    /// <summary>
    /// Check the preconditoins for the status effect to take effect.
    /// </summary>
    /// <param name="trigger">What triggers the status effect.</param>
    /// <returns>If the status effect was triggered.</returns>
    public virtual bool CheckPrecondition(TriggerType trigger)
    {
        if (trigger == m_TriggerType)
            return true;
        else
            return false;
    }

    /// <summary>
    /// Check the preconditoins for the status effect to take effect.
    /// </summary>
    /// <param name="trigger">What triggers the status effect.</param>
    /// <param name="target">The target of the status effect.</param>
    /// <returns>If the status effect was triggered.</returns>
    public virtual bool CheckPrecondition(TriggerType trigger, Unit affected)
    {
        if (trigger == m_TriggerType)
            return true;
        else
            return false;
    }

    public virtual void TakeEffect() {}
    public virtual void TakeEffect(Unit affected) {}

    public bool GetAffectSelf() { return m_AffectSelf; }
}
