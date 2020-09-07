﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    //Instance of the AIManager.
    public static AIManager m_Instance = null;

    //Unit's that track the closest Unit that is controlled by the player, and a Unit for the current AI Unit.
    private Unit m_CurrentAIUnit;

    //The path for the AI to walk on.
    public Stack<Node> m_Path = new Stack<Node>();

    //A dictionary that contains the scores for the nodes, outlining which is more desirable to move to.
    public Dictionary<Node, int> nodeScores = new Dictionary<Node, int>();

    private Node optimalNode = new Node();

    private List<Node> modifyNodes = new List<Node>();

    //On Awake, initialise the instance of this manager.
    private void Awake()
    {
        m_Instance = this;
    }

    // This was Update(), I turned it into a function that the GameManager calls instead
    /// <summary>
    /// Makes all AI units take their turns
    /// </summary>
    public void TakeAITurn()
    {
        // Prune the active units
        DisableUnits(UnitsManager.m_Instance.m_ActiveEnemyUnits.Where(u => u.GetCurrentHealth() <= 0).ToList());

        Debug.Log($"Taking AI Turn: {UnitsManager.m_Instance.m_ActiveEnemyUnits.Count} units");
        // For each AI unit currently alive.
        foreach (Unit unit in UnitsManager.m_Instance.m_ActiveEnemyUnits)
        {
            // The current AI unit is assigned
            m_CurrentAIUnit = unit;
            GameManager.m_Instance.m_SelectedUnit = unit;

            //Calculate the heuristics of the unit and get them.
            CalculateHeursitics(unit);

            
            //Find the AI's best choice of move.
            optimalNode = FindOptimalNode(Grid.m_Instance.GetNodesWithinRadius(unit.GetCurrentMovement(), Grid.m_Instance.GetNode(unit.transform.position), true));

            Debug.LogWarning($"{unit.name} travelling to optimal node {optimalNode.m_NodeHighlight.name}", optimalNode.m_NodeHighlight);

            FindPathToOptimalNode();
        }

        //Tell the game manager it is not our turn anymore.
        GameManager.m_Instance.EndCurrentTurn();
    }

    //Finds the path from the two units and sets the AI movement path.
    // Could probably be rewritten
    public void FindPathToOptimalNode()
    {
        if (Grid.m_Instance.FindPath(m_CurrentAIUnit.transform.position, optimalNode.worldPosition, out m_Path, out int pathCost))
        {
            m_CurrentAIUnit.SetMovementPath(m_Path);
            print(m_CurrentAIUnit.name + ": " + string.Join(", ", m_CurrentAIUnit.GetMovementPath().ToList().Select(no =>no.m_NodeHighlight.name)));
            m_CurrentAIUnit.m_ActionOnFinishPath = CheckAttackRange;
        }
    }

    // Checks adjacent nodes of the AI unit to see if they are able to attack and hit the player.
    // This is not expandable for other units. consider loking at all the nodes in range like in GameManager.cs - James L
    public void CheckAttackRange(Unit u)
    {
        for (int i = 0; i < 4; i++)
        {
            Node node = Grid.m_Instance.GetNode(u.transform.position).adjacentNodes[i];
            if (node.unit?.m_Allegiance == Allegiance.Player)
            {
                Attack(node, u);
                break;
            }
        }
    }

    /// <summary>
    /// Triggers the unit's basic attack
    /// </summary>
    /// <param name="sourceNode"></param>
    public void Attack(Node sourceNode, Unit attacker) // TODO remove attacker - only exists for printng a debug statement
    {
        m_CurrentAIUnit.ActivateSkill(m_CurrentAIUnit.GetSkill(0), sourceNode);
        Debug.Log($"{attacker.name} is attacking {sourceNode.unit.name} with {m_CurrentAIUnit.GetSkill(0).name}");
    }

    /// <summary>
    /// Adds more units to the active units
    /// </summary>
    /// <param name="newUnits"></param>
    public void EnableUnits(List<Unit> newUnits)
    {
        UnitsManager.m_Instance.m_ActiveEnemyUnits.AddRange(newUnits);
        // In case of units already added being in the list.
        UnitsManager.m_Instance.m_ActiveEnemyUnits = UnitsManager.m_Instance.m_ActiveEnemyUnits.Distinct().ToList();
    }

    /// <summary>
    /// Removes units from the active units
    /// </summary>
    /// <param name="deadUnits"></param>
    public void DisableUnits(List<Unit> deadUnits) => UnitsManager.m_Instance.m_ActiveEnemyUnits = UnitsManager.m_Instance.m_ActiveEnemyUnits.Except(deadUnits).ToList();

    //This function returns the node with the highest MinMax score of available nodes the AI Unit can move to.
    public Node FindOptimalNode(List<Node> nodes)
    {
        try
        {
            optimalNode = nodes.Aggregate((next, highest) => next.GetMinMax() > highest.GetMinMax() ? next : highest);
        }
        catch (InvalidOperationException)
        {
            Debug.LogError("There are no nodes in the list!");
            optimalNode = null;
        }

        //Return out with the optimal node.
        return optimalNode; 
    }

    void CalculateHeursitics(Unit unit)
    {
        AIHeuristicCalculator heuristics = unit.GetHeuristicCalculator();

        modifyNodes.Distinct().ToList().ForEach(n => n.ResetHeuristic());
        modifyNodes.Clear();

        for (int i = 0; i < heuristics.m_AIActionHeuristics.Count; ++i)
        {
            switch (heuristics.m_AIActionHeuristics[i])
            {
                // Calculate heuristic for moving to each node.
                case AIHeuristics.Move:
                    foreach (Unit u in UnitsManager.m_Instance.m_PlayerUnits)
                    {
                        Stack<Node> path = new Stack<Node>();
                        if (!Grid.m_Instance.FindPath(unit.transform.position, u.transform.position, out path, out int pathCost, allowBlocked: true))
                        {
                            Debug.LogError("Pathfinding couldn't find a path between AI unit " + unit.name + " and " + u.name + ".");
                            continue;
                        }

                        // Go through path to closest unit, assign movement heuristic to normalized position on the stack of the path.
                        // Will favour shortest path.

                        int pathLength = path.Count - 1;

                        for (int j = 0; j < pathLength; ++j)
                        {
                            Node n = path.Pop();
                            n.SetMovement((float)j / pathLength);
                            modifyNodes.Add(n);
                        }
                    }
                    break;

                // Calculate heuristic for attacking.
                case AIHeuristics.Attack:
                    // Find the damage skills of the current unit for checking.

                    List<DamageSkill> damageSkills = unit.GetSkills().OfType<DamageSkill>().ToList();

                    foreach (DamageSkill skill in damageSkills)
                    {
                        List<Node> nodesInRange =
                            Grid.m_Instance.GetNodesWithinRadius(
                                skill.m_AffectedRange + skill.m_CastableDistance + unit.GetCurrentMovement(),
                                Grid.m_Instance.GetNode(unit.transform.position),
                                true
                                );

                        foreach (Node node in nodesInRange)
                        {
                            if (node.unit?.GetAllegiance() == Allegiance.Player)
                            {
                                List<Node> nodes = Grid.m_Instance.GetNodesWithinRadius(skill.m_CastableDistance, node);

                                for (int j = 0; j < nodes.Count; j++) 
                                {
                                    // TODO: figure out how to do this properly
                                    nodes[j]?.SetDamage(Mathf.Max(node.GetDamage(), skill.m_DamageAmount) * (Vector3.Distance(node.worldPosition, nodes[j].worldPosition)* 0.1f));
                                    modifyNodes.Add(nodes[j]);
                                }

                                if (node.unit.GetCurrentHealth() <= skill.m_DamageAmount)
                                {
                                    node.SetKill(heuristics.m_KillPoints);
                                    modifyNodes.Add(node);
                                }
                            }
                        }
                    }
                    break;

                // Calculate heuristic for healing.                
                case AIHeuristics.Heal:

                    List<HealSkill> healSkills = unit.GetSkills().OfType<HealSkill>().ToList();

                    foreach (HealSkill skill in healSkills)
                    {
                        List<Node> nodesInRange =
                            Grid.m_Instance.GetNodesWithinRadius(
                                skill.m_AffectedRange + skill.m_CastableDistance + unit.GetCurrentMovement(),
                                Grid.m_Instance.GetNode(unit.transform.position)
                                );

                        foreach (Node node in nodesInRange)
                        {
                            if (node.unit?.GetAllegiance() == Allegiance.Enemy)
                            {
                                node.SetHealing(Mathf.Max(node.GetDamage(), skill.m_HealAmount));
                                modifyNodes.Add(node);
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
