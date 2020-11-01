using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Passives/Famine Passive")]
public class FaminePassive : PassiveSkill
{
	public int m_HealthRegeneration = 0;

	public override bool CheckPrecondition(TriggerType trigger)
	{
		if (base.CheckPrecondition(trigger) == true)
			return true;

		return false;
	}

	public override void TakeEffect(Unit affected)
	{
		affected.IncreaseCurrentHealth(m_HealthRegeneration);
	}
}
