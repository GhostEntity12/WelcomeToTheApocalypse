using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AIHeuristics
{
    Move,
    Attack,
    Heal
}

[CreateAssetMenu(fileName = "AI Heuristic Calculator", menuName = "AI Heuristic Calculator", order = 2)]
public class AIHeuristicCalculator : ScriptableObject
{
    public List<AIHeuristics> m_AIActionHeuristics = new List<AIHeuristics>();

    public float m_KillPoints = 10;

    /// <summary>
    /// Calculate the heuristics for this unit.
    /// </summary>
    public void CalculateHeursitic()
    {
        List<Unit> activeUnits = UnitsManager.m_Instance.m_ActiveEnemyUnits;
        Unit currentUnit = null;

        foreach(Unit u in UnitsManager.m_Instance.m_ActiveEnemyUnits)
        {
            if (u.GetHeuristicCalculator() == this)
            {
                currentUnit = u;
                break;
            }
        }

        // Make sure we found the current unit.
        if (currentUnit == null)
        {
            Debug.LogError("Heuristic Calculator couldn't find unit.");
            return;
        }

        // Get the nodes the AI unit can affect this turn (movement + range of skill with largest range).
        //List<Node> reachableNodesToAffect = Grid.m_Instance.GetNodesWithinRadius(currentUnit.GetCurrentMovement() + skillRange, Grid.m_Instance.GetNode(currentUnit.transform.position));

        List<Unit> playerUnits = UnitsManager.m_Instance.m_PlayerUnits;
        List<Node> moveableNodes = Grid.m_Instance.GetNodesWithinRadius(currentUnit.GetCurrentMovement(), Grid.m_Instance.GetNode(currentUnit.transform.position));

        for (int i = 0; i < m_AIActionHeuristics.Count; ++i)
        {
            switch(m_AIActionHeuristics[i])
            {
                // Calculate heuristic for moveing to each node.
                case AIHeuristics.Move:
                foreach(Unit u in playerUnits)
                {
                    Stack<Node> path = new Stack<Node>();
                    int pathCost = 0;
                    if (!Grid.m_Instance.FindPath(currentUnit.transform.position, u.transform.position, ref path, out pathCost))
                    {
                        Debug.LogError("Pathfinding couldn't find a path between AI unit " + currentUnit.name + " and " + u.name + ".");
                        continue;
                    }

                    // Go through path to closest unit, assign movement heuristic to normalized position on the stack of the path.
                    // Will favour shortest path.
                    for(int j = 0; j < path.Count; ++j)
                    {
                        path.Pop().SetMovement(j / path.Count);
                    }
                }
                break;

                case AIHeuristics.Attack:
                // Calculate heuristics for attacking at this node.

                // Find the damage skills of the current unit for checking.
                List<BaseSkill> damageUnitSkills = currentUnit.GetSkills();
                List<DamageSkill> realDamageSkills = new List<DamageSkill>();

                foreach(BaseSkill s in damageUnitSkills)
                {
                    // Try to convert the current skill to a damage skill.
                    DamageSkill ds = s as DamageSkill;

                    // If the damage skill isn't null, conversion was successful, and the skill is a damage skill.
                    if (ds != null)
                    {
                        realDamageSkills.Add(ds);
                    }
                }

                // Find the damage skill with the longest range.
                DamageSkill longestRangeDSkill = null;
                int longestDSkillRange = 0;

                foreach(DamageSkill s in realDamageSkills)
                {
                    if(s.m_CastableDistance + s.m_AffectedRange > longestDSkillRange)
                    {
                        longestDSkillRange = s.m_CastableDistance + s.m_AffectedRange;
                        longestRangeDSkill = s;
                    }
                }

                // Go through each player unit.
                foreach(Unit u in playerUnits)
                {                    
                    // Go through each node the AI unit can move to.
                    foreach(Node n in moveableNodes)
                    {
                        float currentDamage = n.GetDamage();
                        List<Node> nodesSkillRange = Grid.m_Instance.GetNodesWithinRadius(longestDSkillRange, n);

                        // Go through each node the AI unit can hit with their longest range skill from moveable node currently being checked.
                        foreach(Node s in nodesSkillRange)
                        {
                            // If the node has a unit on it, check if they're controlled by the player and assign heuristic.
                            if(s.unit != null)
                            {
                                if (s.unit.GetAllegiance() == Allegiance.Player)
                                {
                                    // If the current node's damage heuristic is greater than 0, it has already been assigned.
                                    // Find attacking which unit will give best result to break tie.
                                    if (currentDamage > 0)
                                    {                                        
                                        float newDamage = longestRangeDSkill.m_DamageAmount / s.unit.GetCurrentHealth();
                                        if (newDamage > currentDamage)
                                        {
                                            currentDamage = newDamage;
                                            n.SetDamage(currentDamage);
                                        }
                                    }
                                    // Else, this is first time damage being set, just set normally.
                                    else
                                    {
                                        n.SetDamage(longestRangeDSkill.m_DamageAmount / s.unit.GetCurrentHealth());
                                        currentDamage = longestRangeDSkill.m_DamageAmount / s.unit.GetCurrentHealth();
                                    }
                                    
                                    // If the damage of this skill is greater than or equal to the health of the target unit, the skill will kill that unit.
                                    // Add the kill points to the node.
                                    if (longestRangeDSkill.m_DamageAmount >= s.unit.GetCurrentHealth())
                                    {
                                        n.SetKill(m_KillPoints);
                                    }
                                }
                            }
                        }
                    }
                }
                break;

                case AIHeuristics.Heal:
                // Calculate heuristic for healing at this node.                

                // Find the heal skills of the current unit for checking.
                List<BaseSkill> healUnitSkills = currentUnit.GetSkills();
                List<HealSkill> realHealSkills = new List<HealSkill>();

                foreach(BaseSkill s in healUnitSkills)
                {
                    // Try to convert the current skill to a heal skill.
                    HealSkill hs = s as HealSkill;

                    // If the heal skill isn't null, conversion was successful, and the skill is a heal skill.
                    if (hs != null)
                    {
                        realHealSkills.Add(hs);
                    }
                }

                // Find the heal skill with the longest range.
                HealSkill longestRangeHSkill = null;
                int longestHSkillRange = 0;

                foreach(HealSkill s in realHealSkills)
                {
                    if(s.m_CastableDistance + s.m_AffectedRange > longestHSkillRange)
                    {
                        longestHSkillRange = s.m_CastableDistance + s.m_AffectedRange;
                        longestRangeHSkill = s;
                    }
                }

                // Go through each player unit.
                foreach(Unit u in playerUnits)
                {                    
                    // Go through each node the AI unit can move to.
                    foreach(Node n in moveableNodes)
                    {
                        float currentHeal = n.GetHealing();
                        List<Node> nodesSkillRange = Grid.m_Instance.GetNodesWithinRadius(longestHSkillRange, n);

                        // Go through each node the AI unit can affect with their longest range skill from moveable node currently being checked.
                        foreach(Node s in nodesSkillRange)
                        {
                            // If the node has a unit on it, check if they're controlled by the enemy and assign heuristic.
                            if(s.unit != null)
                            {
                                if (s.unit.GetAllegiance() == Allegiance.Enemy)
                                {
                                    // If the current node's heal heuristic is greater than 0, it has already been assigned.
                                    // Find healing which unit will give best result to break tie.
                                    if (currentHeal > 0)
                                    {                                        
                                        float newHeal = longestRangeHSkill.m_HealAmount / s.unit.GetCurrentHealth();
                                        if (newHeal > currentHeal)
                                        {
                                            currentHeal = newHeal;
                                            n.SetHealing(currentHeal);
                                        }
                                    }
                                    // Else, this is first time damage being set, just set normally.
                                    else
                                    {
                                        n.SetHealing(longestRangeHSkill.m_HealAmount / s.unit.GetCurrentHealth());
                                        currentHeal = longestRangeHSkill.m_HealAmount / s.unit.GetCurrentHealth();
                                    }
                                }
                            }
                        }
                    }
                }
                break;

                default:
                Debug.LogError("Heuristic calculator doesn't have valid heuristic to calculate for.");
                break;
            }
        }
    }
}