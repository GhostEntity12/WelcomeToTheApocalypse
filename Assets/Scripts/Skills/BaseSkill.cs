using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum SkillType
{
	Basic = 1,
	Special = 2
}

public enum SkillTargets
{
	All,
	Allies,
	Foes,
}

public enum TargetType
{
	SingleTarget,
	Line,
	Terrain
}

public class BaseSkill : ScriptableObject
{
	[Header("Display")]
	// The name of the skill
	public string m_SkillName;
	// The light half of the icon which represents the skill
	public Sprite m_LightIcon;
	// The dark half of the icon which represents the skill
	public Sprite m_DarkIcon;
	//The skill's description
	[TextArea(1, 5)]
	public string m_Description;

	[Header("Targeting")]
	// Who the skill can hit
	public SkillTargets targets;
	// How the skill is cast
	public TargetType targetType;
	// Whether the caster can target themselves
	public bool excludeCaster;
	public Node m_SourceNode;

	[Header("Stats")]
	// What type of skill it is (and how many AP it costs to cast)
	public SkillType m_SkillType = SkillType.Special;
	// How many turns before the caster can cast the skill again
	public int m_CooldownLength = 3;
	// How many remaining turns before the caster can cast the skill again
	public int m_CurrentCooldown = 0;
	// How far away the skill can be cast from the caster
	public int m_CastableDistance;
	// How large of an area around the cast location will be affected
	public int m_AffectedRange;
	// The cost of using the skill
	public int m_Cost;

	[FMODUnity.EventRef]
	public string m_CastEvent = "";

	protected Unit[] affectedUnits;
	public List<Node> affectedNodes;

	public void Startup()
	{
		m_CurrentCooldown = 0;
	}

	public virtual void CastSkill()
	{
		FindAffectedUnits();

		// If it is the AI's turn, add to AI manager's list.
		if (GameManager.m_Instance.GetCurrentTurn() == Allegiance.Enemy)
			AIManager.m_Instance.m_AwaitingUnits.AddRange(affectedUnits);

		// Set the cooldown when the skill is used.
		m_CurrentCooldown = m_CooldownLength;

		// Update the cooldowns
		foreach (SkillButton button in UIManager.m_Instance.m_SkillSlots)
		{
			button.UpdateCooldownDisplay();
		}
	}

	/// <summary>
	/// Decrement the cooldown of the skill.
	/// </summary>
	public virtual void DecrementCooldown()
	{
		if (m_CurrentCooldown > 0)
		{
			--m_CurrentCooldown;
		}
	}

	/// <summary>
	/// Get the current cooldown on the skill.
	/// </summary>
	/// <returns>The current cooldown of the skill.</returns>
	public virtual int GetCurrentCooldown() { return m_CurrentCooldown; }

	public Unit[] GetAffectedUnits() { return affectedUnits; }

	public void FindAffectedUnits()
	{
		affectedUnits = affectedNodes.Select(t => t.unit)
			.Where(c => GameManager.IsTargetable(GameManager.m_Instance.GetSelectedUnit(), c, this))
			.Distinct() // Had to add this - for some reason, it grabbed the same character multiple times somehow
			.ToArray();

		/* Equivalent (mostly) non-LINQ version

        foreach (var item in affectedNodes.Select(n => n.unit)) 
        {
            if (!item) continue;

            if (GameManager.m_Instance.GetSelectedUnit() == item && this.excludeCaster) continue;

            if ((GameManager.m_Instance.GetSelectedUnit().m_Allegiance == item.m_Allegiance && this.targets == SkillTargets.Allies) ||
                (GameManager.m_Instance.GetSelectedUnit().m_Allegiance != item.m_Allegiance && this.targets == SkillTargets.Foes) ||
                (this.targets == SkillTargets.All))
            {
                affectedUnits.Append(item);
            }
            else continue;
        }*/
	}
}
