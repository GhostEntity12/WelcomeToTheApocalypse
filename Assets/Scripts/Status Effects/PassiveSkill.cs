using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PassiveSkill : StatusEffect
{
    // Check the preconditions for the status effect to take effect.
    public abstract override bool CheckPrecondition(TriggerType trigger);
}
