using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InflictableStatus : StatusEffect
{
    // Starting time for how long the status lasts.
    public int m_StartingDuration = 0;
    // How much time remains for the status.
    public int m_RemainingDuration = 0;

    // Check the preconditions for the status effect to take effect.
    public override abstract bool CheckPrecondition(TriggerType trigger);

    protected void ResetDuration() { m_RemainingDuration = m_StartingDuration; }
}
