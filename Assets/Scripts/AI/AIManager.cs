using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.UI;

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

    private BaseSkill m_OptimalSkill = null;

    /// <summary>
    /// List of MinMax scores of all the nodes in the scene.
    /// </summary>
    private List<Node> m_NodeHeuristicList = new List<Node>();

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

    /// <summary>
    /// Makes all AI units take their turns
    /// </summary>
    public void TakeAITurn()
    {
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
        // Make sure the current unit isn't moving.
        if (m_CurrentAIUnit.GetMoving() == false)
        {
            //Calculate the heuristics of the unit and get them.
            CalculateHeursitics(m_CurrentAIUnit);

            //Find the AI's best choice of move.
            m_OptimalNode = FindOptimalNode(Grid.m_Instance.GetNodesWithinRadius(m_CurrentAIUnit.GetCurrentMovement(), Grid.m_Instance.GetNode(m_CurrentAIUnit.transform.position), true));

            // While there is another unit standing on the current optimal node, get the next best node.
            while (m_OptimalNode.unit != null)
            {
                m_OptimalNode = m_NodeHeuristicList.First();
                // Remove it from the list.
                m_NodeHeuristicList.RemoveAt(0);
            }

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
            print(m_CurrentAIUnit.name + ": " + string.Join(", ", m_CurrentAIUnit.GetMovementPath().ToList().Select(no => no.m_NodeHighlight.name)));
            // Make sure the AI wants to attack or heal once it reaches it's destination.
            if (m_OptimalNode.GetDamage() > 0 || m_OptimalNode.GetHealing() > 0)
                m_CurrentAIUnit.m_ActionOnFinishPath = CheckAttackRange;
        }
    }

    // Checks adjacent nodes of the AI unit to see if they are able to attack and hit the player.
    // This is not expandable for other units. consider loking at all the nodes in range like in GameManager.cs - James L
    public void CheckAttackRange(Unit u)
    {
        Attack(m_OptimalNode);
    }

    /// <summary>
    /// Triggers the unit's basic attack
    /// </summary>
    /// <param name="sourceNode"></param>
    public void Attack(Node sourceNode)
    {
        Debug.Log(m_OptimalSkill.GetCurrentCooldown());
        if (m_OptimalSkill.GetCurrentCooldown() <= 0)
            m_CurrentAIUnit.ActivateSkill(m_OptimalSkill, Grid.m_Instance.GetNode(m_OptimalNode.GetAITarget().transform.position));
    }

    /// <summary>
    /// Adds more units to the active units
    /// </summary>
    /// <param name="newUnits"></param>
    public void EnableUnits(Unit[] newUnits) => EnableUnits(newUnits.ToList());

    public void IncrementAIUnitIterator()
    {
        m_AIIterator++;

        // Reset things from the AI unit's turn.
        foreach(Node node in m_ModifyNodes)
		{
            node.ResetHeuristic();
		}
        m_OptimalSkill = null;
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
        GameManager.m_Instance.m_DidHealthBonus = false;
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
    /// <returns>The most optimal node for the AI unit to move to.</returns>
    public Node FindOptimalNode(List<Node> nodes)
    {
        try
        {
            for (int i = 0; i < nodes.Count; ++i)
            {
                // Get the nodes as a list rather than the most optimal one.
                // Allows us to keep track of the MinMax scores of other nodes.
                m_NodeHeuristicList = nodes.OrderByDescending(n => n.GetMinMax()).ToList();
                m_OptimalNode = m_NodeHeuristicList.First();
                m_NodeHeuristicList.RemoveAt(0);
            }
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

        // If the AI's turn is starting, check what AI units are alive.
        if (m_AITurn == true)
        {
            // Prune the active units
            DisableUnits(UnitsManager.m_Instance.m_ActiveEnemyUnits.Where(u => u.GetCurrentHealth() <= 0).ToList());

            Debug.Log($"Taking AI Turn: {UnitsManager.m_Instance.m_ActiveEnemyUnits.Count} units");
        }
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
                        // Get the nodes within the furthest reaches of what the AI unit could reach with the casting distance, range, and unit's movement.
                        List<Node> nodesInRange =
                            Grid.m_Instance.GetNodesWithinRadius(
                                skill.m_AffectedRange + skill.m_CastableDistance + unit.GetCurrentMovement(),
                                Grid.m_Instance.GetNode(unit.transform.position),
                                true
                                );

                        List<Node> nodesWithUnits = nodesInRange.Where(n => n.unit != null).ToList();

                        // Find a node that has a player unit on it.
                        foreach (Node node in nodesWithUnits)
                        {
                            // If the node has a player unit on it, calculate score for damaging the player unit.
                            if (node.unit?.GetAllegiance() == Allegiance.Player)
                            {
                                // Get nodes in the area that the AI unit could hit the player unit from.
                                List<Node> nodes = Grid.m_Instance.GetNodesWithinRadius(skill.m_CastableDistance, node);

                                // Go through each of the nodes the AI unit could attack from and add the attack heuristic to them.
                                for (int j = 0; j < nodes.Count; j++)
                                {
                                    // Calculate the new damage for the node's damage heuristic.
                                    float newDamageH = Mathf.Max(node.GetDamage(), skill.m_DamageAmount);
                                    if (newDamageH < nodes[j].GetDamage())
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        if (nodes[j] != null)
                                        {
                                            Node currentDamageNode = nodes[j];
                                            currentDamageNode.SetDamage(newDamageH * (Vector3.Distance(node.worldPosition, nodes[j].worldPosition) * 0.1f));
                                            currentDamageNode.SetAITarget(node.unit);
                                            m_OptimalSkill = skill;
                                            m_ModifyNodes.Add(nodes[j]);
                                        }
                                    }
                                }

                                // Check for kill heuristic.
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
                                Grid.m_Instance.GetNode(unit.transform.position),
                                true
                                );

                        List<Node> nodesWithUnits = nodesInRange.Where(n => n.unit != null).ToList();

                        foreach (Node node in nodesWithUnits)
                        {
                            if (node.unit?.GetAllegiance() == Allegiance.Enemy)
                            {
                                Unit currentUnit = node.unit;
                                // Get nodes in the area that the AI unit could heal friendly units from.
                                List<Node> nodes = Grid.m_Instance.GetNodesWithinRadius(skill.m_CastableDistance, node);

                                // Go through each of the nodes the AI unit could heal from and add the heal heuristic to them.
                                for (int j = 0; j < nodes.Count; j++)
                                {
                                    // Calculate the new heal for the node's heal heuristic.
                                    float newHealH = skill.m_HealAmount + (currentUnit.GetStartingHealth() - currentUnit.GetCurrentHealth());
                                    if (newHealH < nodes[j].GetHealing())
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        if (nodes[j] != null)
                                        {
                                            Node currentHealNode = nodes[j];
                                            currentHealNode.SetHealing(newHealH * (Vector3.Distance(node.worldPosition, nodes[j].worldPosition) * 0.1f));
                                            currentHealNode.SetAITarget(node.unit);
                                            m_OptimalSkill = skill;
                                            m_ModifyNodes.Add(nodes[j]);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    break;

                case AIHeuristics.StatusEffect:
                    List<StatusSkill> statusSkills = m_CurrentAIUnit.GetSkills().OfType<StatusSkill>().ToList();

                    foreach(StatusSkill skill in statusSkills)
					{
                        // Make sure the skill isn't on cooldown.
                        if (skill.GetCurrentCooldown() <= 0)
						{
                            List<Node> nodesInRange =
                            Grid.m_Instance.GetNodesWithinRadius(
                                skill.m_AffectedRange + skill.m_CastableDistance + unit.GetCurrentMovement(),
                                Grid.m_Instance.GetNode(unit.transform.position),
                                true
                                );

                            List<Node> nodesWithUnits = nodesInRange.Where(n => n.unit != null).ToList();

                            foreach (Node node in nodesWithUnits)
                            {
                                // Targets AI units.
                                if (skill.targets == SkillTargets.Foes)
                                {
                                    // If unit is an AI unit.
                                    if (node.unit.m_Allegiance == Allegiance.Enemy)
                                    {
                                        Unit currentUnit = node.unit;

                                        // Get nodes in the area that the AI unit could inflict a status on units from.
                                        List<Node> nodes = Grid.m_Instance.GetNodesWithinRadius(skill.m_CastableDistance, node);

                                        // Go through each of the nodes the AI unit could inflict a status from and add the status heuristic to them.
                                        for (int j = 0; j < nodes.Count; j++)
                                        {
                                            // Try to put a status on the healthiest unit, to get the most value.
                                            float newStatusH = currentUnit.GetCurrentHealth();
                                            if (newStatusH < nodes[j].GetStatus())
                                                continue;
                                            else
                                            {
                                                if (nodes[j] != null)
                                                {
                                                    Node currentStatusNode = nodes[j];
                                                    currentStatusNode.SetHealing(newStatusH * (Vector3.Distance(node.worldPosition, nodes[j].worldPosition) * 0.1f));
                                                    currentStatusNode.SetAITarget(node.unit);
                                                    m_OptimalSkill = skill;
                                                    m_ModifyNodes.Add(nodes[j]);
                                                }
                                            }
                                        }
                                    }
                                    // Targets Player units.
                                    else if (skill.targets == SkillTargets.Allies)
                                    {
                                        // If unit is a Player unit.
                                        if (node.unit.m_Allegiance == Allegiance.Player)
                                        {
                                            Unit currentUnit = node.unit;

                                            // Get nodes in the area that the AI unit could inflict a status on units from.
                                            List<Node> nodes = Grid.m_Instance.GetNodesWithinRadius(skill.m_CastableDistance, node);

                                            // Go through each of the nodes the AI unit could inflict a status from and add the status heuristic to them.
                                            for (int j = 0; j < nodes.Count; j++)
                                            {
                                                // Try to put a status on the healthiest unit, to get the most value.
                                                float newStatusH = currentUnit.GetCurrentHealth();
                                                if (newStatusH < nodes[j].GetStatus())
                                                    continue;
                                                else
                                                {
                                                    if (nodes[j] != null)
                                                    {
                                                        Node currentStatusNode = nodes[j];
                                                        currentStatusNode.SetHealing(newStatusH * (Vector3.Distance(node.worldPosition, nodes[j].worldPosition) * 0.1f));
                                                        currentStatusNode.SetAITarget(node.unit);
                                                        m_OptimalSkill = skill;
                                                        m_ModifyNodes.Add(nodes[j]);
                                                    }
                                                }
                                            }
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
