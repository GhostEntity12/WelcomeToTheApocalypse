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

public abstract class StatusEffect : MonoBehaviour
{
    // How this status effect is triggered.
    public TriggerType m_TriggerType;

    // Icon for the status effect.
    public Sprite m_StatusIcon;

    // Description of the status effect.
    public string m_StatusDescription;

    // Check the preconditions for the status effect to take effect.
    protected abstract bool CheckPrecondition(TriggerType trigger);
}
