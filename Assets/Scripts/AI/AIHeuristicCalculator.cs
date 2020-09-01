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

        // Figure out the skill the unit has with the largest range.
        List<BaseSkill> unitSkills = currentUnit.GetSkills();

        BaseSkill skillLargestRange = unitSkills[0];

        int skillRange = skillLargestRange.m_CastableDistance + skillLargestRange.m_AffectedRange;

        // Check which skill has the largest range.
        foreach(BaseSkill s in unitSkills)
        {
            if ((s.m_CastableDistance + s.m_AffectedRange) > (skillLargestRange.m_CastableDistance + skillLargestRange.m_AffectedRange))
            {
                skillLargestRange = s;
                skillRange = skillLargestRange.m_CastableDistance + skillLargestRange.m_AffectedRange;
            }
        }

        // Get the nodes the AI unit can affect this turn (movement + range of skill with largest range).
        //List<Node> reachableNodesToAffect = Grid.m_Instance.GetNodesWithinRadius(currentUnit.GetCurrentMovement() + skillRange, Grid.m_Instance.GetNode(currentUnit.transform.position));

        List<Unit> playerUnits = UnitsManager.m_Instance.m_PlayerUnits;

        for (int i = 0; i < m_AIActionHeuristics.Count; ++i)
        {
            switch(m_AIActionHeuristics[i])
            {
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
                break;

                case AIHeuristics.Kill:
                // Calculate heuristic for getting a kill at this node.
                break;

                case AIHeuristics.Heal:
                // Calculate heuristic for healing at this node.
                break;

                default:
                Debug.LogError("Heuristic calculator doesn't have valid heuristic.");
                break;
            }
        }
    }
}