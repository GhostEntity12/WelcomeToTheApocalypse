using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Heal Skill")]
public class HealSkill : BaseSkill
{
	[Header("Skill Stats")]
	public int m_HealAmount;

	//The actual unit the skill will be used on.
	public Unit chosenTarget;
}
