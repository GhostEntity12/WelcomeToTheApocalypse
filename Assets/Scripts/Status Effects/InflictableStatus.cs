public abstract class InflictableStatus : StatusEffect
{
    // Starting time for how long the status lasts.
    public int m_StartingDuration = 0;
    // How much time remains for the status.
    public int m_RemainingDuration = 0;

    // Check the preconditions for the status effect to take effect.
    public override abstract bool CheckPrecondition(TriggerType trigger);

    /// <summary>
    /// Decrement the duration of the status effect.
    /// </summary>
    /// <returns>If the status effect's duration has eneded or not.</returns>
    public bool DecrementDuration()
    {
        if (m_RemainingDuration <= 0)
        {
            return true;
        }
        else
        {
            --m_RemainingDuration;
            return false;
        }
    }
}
