using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class AIManager : MonoBehaviour
{
    //Instance of the AIManager.
    public static AIManager m_Instance = null;

    //Unit's that track the closest Unit that is controlled by the player, and a Unit for the current AI Unit.
    private Unit m_CurrentAIUnit;

    //The path for the AI to walk on.
    public Stack<Node> m_Path = new Stack<Node>();

    private Node m_OptimalNode = new Node();

    private List<Node> m_ModifyNodes = new List<Node>();

    private bool m_AITurn = false;

    private int m_AIIterator = 0;

    //On Awake, initialise the instance of this manager.
    private void Awake()
    {
        m_Instance = this;
    }

    void Update()
    {
        if (m_AITurn == true)
        {
            TakeAITurn();
        }
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
        
        // Check if we're done with the AI's turn.
        if (m_AIIterator == UnitsManager.m_Instance.m_ActiveEnemyUnits.Count)
        {
            // If iterator is at the end of active AI units, reset iterator and end AI turn.
            m_AIIterator = 0;
            //Tell the game manager it is not our turn anymore.
            GameManager.m_Instance.EndCurrentTurn();
            return;
        }

        // The current AI unit is assigned
        m_CurrentAIUnit = UnitsManager.m_Instance.m_ActiveEnemyUnits[m_AIIterator];
        GameManager.m_Instance.m_SelectedUnit = m_CurrentAIUnit;
        // For each AI unit currently alive.
        if (m_CurrentAIUnit.GetMoving() == false)
        {
            //Calculate the heuristics of the unit and get them.
            CalculateHeursitics(m_CurrentAIUnit);


            //Find the AI's best choice of move.
            m_OptimalNode = FindOptimalNode(Grid.m_Instance.GetNodesWithinRadius(m_CurrentAIUnit.GetCurrentMovement(), Grid.m_Instance.GetNode(m_CurrentAIUnit.transform.position), true));

            Debug.LogWarning($"{m_CurrentAIUnit.name} travelling to optimal node {m_OptimalNode.m_NodeHighlight.name}", m_OptimalNode.m_NodeHighlight);

            FindPathToOptimalNode();
        }
    }

    //Finds the path from the two units and sets the AI movement path.
    // Could probably be rewritten
    public void FindPathToOptimalNode()
    {
        if (Grid.m_Instance.FindPath(m_CurrentAIUnit.transform.position, m_OptimalNode.worldPosition, out m_Path, out int pathCost))
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
                Attack(node);
                break;
            }
        }
        ++m_AIIterator;
    }

    /// <summary>
    /// Triggers the unit's basic attack
    /// </summary>
    /// <param name="sourceNode"></param>
    public void Attack(Node sourceNode)
    {
        m_CurrentAIUnit.ActivateSkill(m_CurrentAIUnit.GetSkill(0), sourceNode);
    }

    /// <summary>
    /// Adds more units to the active units
    /// </summary>
    /// <param name="newUnits"></param>
    public void EnableUnits(Unit[] newUnits) => EnableUnits(newUnits.ToList());

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
    public void DisableUnits(List<Unit> deadUnits)
    {
        UnitsManager.m_Instance.m_ActiveEnemyUnits = UnitsManager.m_Instance.m_ActiveEnemyUnits.Except(deadUnits).ToList();

        GameManager.m_Instance.PodClearCheck();
    }

    /// <summary> 
    /// Returns the node with the highest MinMax score of available nodes the AI Unit can move to.
    /// </summary>
    /// <param name="nodes">The nodes from which to select the best node</param>
    /// <returns></returns>
    public Node FindOptimalNode(List<Node> nodes)
    {
        try
        {
            m_OptimalNode = nodes.Aggregate((next, highest) => next.GetMinMax() > highest.GetMinMax() ? next : highest);
        }
        catch (InvalidOperationException)
        {
            Debug.LogError("There are no nodes in the list!");
            m_OptimalNode = null;
        }

        //Return out with the optimal node.
        return m_OptimalNode; 
    }

    public void SetAITurn(bool aiTurn)
    {
        m_AITurn = aiTurn;
    }

    void CalculateHeursitics(Unit unit)
    {
        AIHeuristicCalculator heuristics = unit.GetHeuristicCalculator();

        if (!heuristics)
        {
            Debug.LogError($"Unit {unit.name} was marked as an AI unit but lacks a Heuristics Calculator!");
            return;
        }

        m_ModifyNodes.Distinct().ToList().ForEach(n => n.ResetHeuristic());
        m_ModifyNodes.Clear();

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
                            m_ModifyNodes.Add(n);
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
                                    m_ModifyNodes.Add(nodes[j]);
                                }

                                if (node.unit.GetCurrentHealth() <= skill.m_DamageAmount)
                                {
                                    node.SetKill(heuristics.m_KillPoints);
                                    m_ModifyNodes.Add(node);
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
                                m_ModifyNodes.Add(node);
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
