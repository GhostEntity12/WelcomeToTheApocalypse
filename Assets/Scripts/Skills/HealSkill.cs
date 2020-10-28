using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Heal Skill")]
public class HealSkill : BaseSkill
{
	[Header("Skill Stats")]
	public int m_HealAmount;

	//The actual unit the skill will be used on.
	public Unit chosenTarget;

	public void AssignSkillProperties()
	{
		/*
		The caster can only heal its allies.
		Only one target can be healed with each use of the skill.
		*/
		targets = SkillTargets.Allies;
		targetType = TargetType.SingleTarget;
		m_SkillType = SkillType.Basic;

		//The caster can also be the one the ability is used on.
		excludeCaster = false;

		//Heal skill can't be used again for two turns. (Subject to change for designers)
		m_CooldownLength = 2;

		//The skill has a range of two.
		m_CastableDistance = 2;

		//The skill is not AOE, it is single target.
		m_AffectedRange = 1;

		//The heal amount is 3 points (Subject to change for designers)
		m_HealAmount = 3;
	}

	public override void CastSkill()
	{
		base.CastSkill();
		// Heal each affected unit

		//If the selected target isnt dead or null
		foreach (Unit unit in affectedUnits)
		{
			//Increase the targets health by 3.
			unit.IncreaseCurrentHealth(m_HealAmount);
		}
	}
}
