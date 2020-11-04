using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Passives/Pestilence Passive")]
public class PestilencePassive : PassiveSkill
{
	[SerializeField]
	private int m_StartingHealResource = 0;

	[SerializeField]
	public int m_CurrentHealResource = 0;

	[SerializeField]
	private int m_HealResourceForDealingDamage = 0;

	[SerializeField]
	public int m_HealResourceCastCost = 0;

	private void Awake()
	{
		m_CurrentHealResource = m_StartingHealResource;
	}

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