using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Heal Skill")]
public class HealSkill : BaseSkill
{
	[Header("Skill Stats")]
	public int m_HealAmount;

	//The actual unit the skill will be used on.
	public Unit chosenTarget;

	public override void CastSkill()
	{
		base.CastSkill();
		// Heal each affected unit

		//If the selected target isnt dead or null
		foreach (Unit unit in affectedUnits)
		{
			//Increase the targets health by 3.
			unit.AddHealingFromSkill(m_HealAmount);
		}
	}
}
