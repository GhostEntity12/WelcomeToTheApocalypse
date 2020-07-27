using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum TriggerType
{
    OnTurnStart,
    OnDealDamage,
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
    protected abstract void CheckPrecondition(TriggerType trigger);
}
