using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AIHeuristics
{
    Move,
    Attack,
    Kill,
    Heal
}

[CreateAssetMenu(fileName = "AI Heuristic Calculator", menuName = "AI Heuristic Calculator", order = 2)]
public class AIHeuristicCalculator : ScriptableObject
{
    public List<AIHeuristics> m_AIActionHeuristics = new List<AIHeuristics>();

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
                List<Node> moveableNodes = Grid.m_Instance.GetNodesWithinRadius(currentUnit.GetCurrentMovement(), Grid.m_Instance.GetNode(currentUnit.transform.position));

                // Find the damage skills of the current unit for checking.
                List<BaseSkill> unitSkills = currentUnit.GetSkills();
                List<DamageSkill> damageSkills = new List<DamageSkill>();

                foreach(BaseSkill s in unitSkills)
                {
                    // Try to convert the current skill to a damage skill.
                    DamageSkill ds = s as DamageSkill;

                    // If the damage skill isn't null, conversion was successful, and the skill is a damage skill.
                    if (ds != null)
                    {
                        damageSkills.Add(ds);
                    }
                }

                // Find the damage skill with the longest range.
                DamageSkill longestRangeDSkill = null;
                int longestDSkillRange = 0;

                foreach(DamageSkill s in damageSkills)
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
                                    if (n.GetDamage() > 0)
                                    {
                                        float currentDamage = n.GetDamage();
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
                                    }
                                }
                            }
                        }
                    }
                }
                break;

                case AIHeuristics.Kill:
                // Calculate heuristic for getting a kill at this node.
                break;

                case AIHeuristics.Heal:
                // Calculate heuristic for healing at this node.
                break;

                default:
                Debug.LogError("Heuristic calculator doesn't have valid heuristic to calculate for.");
                break;
            }
        }
    }
}