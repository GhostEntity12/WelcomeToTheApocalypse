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
    // The icon which represents the skill
    public Sprite m_Icon;
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

    [Header("Stats")]
    // What type of skill it is (and how many AP it costs to cast)
    public SkillType m_SkillType = SkillType.Special;
    // How many turns before the caster can cast the skill again
    public int m_CooldownLength;
    // How many remaining turns before the caster can cast the skill again
    public int m_CooldownRemaining;
    // How far away the skill can be cast from the caster
    public int m_CastableDistance;
    // How large of an area around the cast location will be affected
    public int m_AffectedRange;

    protected Unit[] affectedUnits;
    public List<Node> affectedNodes;

    public virtual void CastSkill()
    {
        Debug.Log("Skill cast");
        affectedUnits = affectedNodes.Select(t => t.unit)
            .Where(c => GameManager.IsTargetable(GameManager.m_Instance.GetSelectedUnit(), c, this))
            .ToArray();

        // Equivalent (mostly) non-LINQ version

        //foreach (var item in affectedNodes.Select(n => n.unit)) 
        //{
        //    if (!item) continue;

        //    if (GameManager.m_Instance.GetSelectedUnit() == item && this.excludeCaster) continue;

        //    if ((GameManager.m_Instance.GetSelectedUnit().m_Allegiance == item.m_Allegiance && this.targets == SkillTargets.Allies) ||
        //        (GameManager.m_Instance.GetSelectedUnit().m_Allegiance != item.m_Allegiance && this.targets == SkillTargets.Foes) ||
        //        (this.targets == SkillTargets.All))
        //    {
        //        affectedUnits.Append(item);
        //    }
        //    else continue;
        //}
    }
}
