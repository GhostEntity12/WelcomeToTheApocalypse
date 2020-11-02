using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Events;
using UnityEngine.Rendering.UI;

public class HeuristicResult
{
    public Unit m_Unit;
    public float m_MovementValue = 0;
    public float m_DamageValue = 0;
    public float m_HealingValue = 0;
    public float m_StatusValue = 0;
    public Node m_Node;

    public DamageSkill m_DamageSkill;
    public HealSkill m_HealSkill;

    public int m_MoveDistance;

    public HeuristicResult(Unit u, AIHeuristics t, float hv, Node n, int d, HealSkill hs, DamageSkill ds)
    {
        switch (t)
        {
            case AIHeuristics.Move:
                m_MovementValue += hv;
                break;
            case AIHeuristics.Attack:
                m_DamageValue += hv;
                break;
            case AIHeuristics.Heal:
                m_HealingValue += hv;
                break;
            case AIHeuristics.StatusEffect:
                m_StatusValue += hv;
                break;
            default:
                break;
        }
        m_Unit = u;
        m_Node = n;
        m_MoveDistance = d;
        m_HealSkill = hs;
        m_DamageSkill = ds;
    }

    public float SumHeuristics() => m_MovementValue + m_DamageValue + m_HealingValue + m_StatusValue;
}

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

    private BaseSkill m_OptimalSkill = null;

    List<HeuristicResult> m_HeuristicResults = new List<HeuristicResult>();

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
        if (!m_CurrentAIUnit)
        {
            m_HeuristicResults.Clear();
            foreach (Unit unit in UnitsManager.m_Instance.m_ActiveEnemyUnits)
            {
                // Movement Heuristics
                DoMovementHeuristics(unit);

                foreach (BaseSkill skill in unit.GetSkills())
                {
                    List<Node> nodesWithUnits = Grid.m_Instance.GetNodesWithinRadius(
                            skill.m_AffectedRange + skill.m_CastableDistance + unit.GetCurrentMovement(),
                            Grid.m_Instance.GetNode(unit.transform.position),
                            true
                        ).Where(n => n.unit != null).ToList();

                    switch (skill)
                    {
                        case StatusSkill ss:
                            if (ss == null) { break; }
                            // Make sure the skill isn't on cooldown.
                            if (skill.GetCurrentCooldown() <= 0)
                            {
                                foreach (Node nodeWithUnit in nodesWithUnits)
                                {
                                    // Targets AI units.
                                    if (skill.targets == SkillTargets.Foes)
                                    {
                                        // If unit is an AI unit.
                                        if (nodeWithUnit.unit.m_Allegiance == Allegiance.Enemy)
                                        {
                                            Unit currentUnit = nodeWithUnit.unit;

                                            // Get nodes in the area that the AI unit could inflict a status on units from.
                                            List<Node> nodesCastable = Grid.m_Instance.GetNodesWithinRadius(skill.m_CastableDistance, nodeWithUnit);

                                            // Go through each of the nodes the AI unit could inflict a status from and add the status heuristic to them.
                                            for (int j = 0; j < nodesCastable.Count; j++)
                                            {
                                                // Try to put a status on the healthiest unit, to get the most value.
                                                float newStatusH = currentUnit.GetCurrentHealth();
                                                if (newStatusH < FindHeuristic(nodesCastable[j], unit)?.m_StatusValue)
                                                    continue;
                                                else
                                                {
                                                    if (nodesCastable[j] != null)
                                                    {
                                                        Node currentStatusNode = nodesCastable[j];
                                                        AddHeuristics(AIHeuristics.StatusEffect, newStatusH * (Vector3.Distance(nodeWithUnit.worldPosition, nodesCastable[j].worldPosition)), nodesCastable[j], unit);
                                                        currentStatusNode.SetAITarget(nodeWithUnit.unit);
                                                        m_OptimalSkill = skill;
                                                    }
                                                }
                                            }
                                        }
                                        // Targets Player units.
                                        else if (skill.targets == SkillTargets.Allies)
                                        {
                                            // If unit is a Player unit.
                                            if (nodeWithUnit.unit.m_Allegiance == Allegiance.Player)
                                            {
                                                Unit currentUnit = nodeWithUnit.unit;

                                                // Get nodes in the area that the AI unit could inflict a status on units from.
                                                List<Node> nodesCastable = Grid.m_Instance.GetNodesWithinRadius(skill.m_CastableDistance, nodeWithUnit);

                                                // Go through each of the nodes the AI unit could inflict a status from and add the status heuristic to them.
                                                for (int j = 0; j < nodesCastable.Count; j++)
                                                {
                                                    // Try to put a status on the healthiest unit, to get the most value.
                                                    float newStatusH = currentUnit.GetCurrentHealth();
                                                    if (newStatusH < FindHeuristic(nodesCastable[j], unit)?.m_StatusValue)
                                                        continue;
                                                    else
                                                    {
                                                        if (nodesCastable[j] != null)
                                                        {
                                                            Node currentStatusNode = nodesCastable[j];
                                                            AddHeuristics(AIHeuristics.StatusEffect, newStatusH * (Vector3.Distance(nodeWithUnit.worldPosition, nodesCastable[j].worldPosition)), nodesCastable[j], unit);
                                                            currentStatusNode.SetAITarget(nodeWithUnit.unit);
                                                            m_OptimalSkill = skill;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        case DamageSkill ds:
                            // Make sure the skill is a damage skill.
                            if (ds == null) { break; }
                            // Find a node that has a player unit on it.
                            foreach (Node nodeWithUnit in nodesWithUnits)
                            {
                                // If the node has a player unit on it, calculate score for damaging the player unit.
                                if (nodeWithUnit.unit?.GetAllegiance() == Allegiance.Player)
                                {
                                    // Get nodes in the area that the AI unit could hit the player unit from.
                                    List<Node> nodesCastable = Grid.m_Instance.GetNodesWithinRadius(ds.m_CastableDistance, nodeWithUnit);

                                    // Go through each of the nodes the AI unit could attack from and add the attack heuristic to them.
                                    for (int j = 0; j < nodesCastable.Count; j++)
                                    {
                                        // Calculate the new damage for the node's damage heuristic.
                                        float hValue = ds.m_DamageAmount * (Vector3.Distance(nodeWithUnit.worldPosition, nodesCastable[j].worldPosition) * 0.1f);
                                        if (hValue < FindHeuristic(nodesCastable[j], unit)?.m_DamageValue)
                                        {
                                            continue;
                                        }
                                        else
                                        {
                                            if (nodesCastable[j] != null)
                                            {
                                                Node currentDamageNode = nodesCastable[j];
                                                AddHeuristics(AIHeuristics.Attack, hValue, currentDamageNode, unit, damageSkill:ds);
                                                currentDamageNode.SetAITarget(nodeWithUnit.unit);
                                                m_OptimalSkill = skill;
                                            }
                                        }
                                    }

                                    // Check for kill heuristic.
                                    if (nodeWithUnit.unit.GetCurrentHealth() <= ds.m_DamageAmount)
                                    {
                                        AddHeuristics(AIHeuristics.Attack, unit.m_AIHeuristicCalculator.m_KillPoints, nodeWithUnit, unit, damageSkill: ds);
                                    }
                                }
                            }
                            break;
                        case HealSkill hs:
                            if (hs == null) { break; }
                            foreach (Node nodeWithUnit in nodesWithUnits)
                            {
                                if (nodeWithUnit.unit?.GetAllegiance() == Allegiance.Enemy)
                                {
                                    Unit currentUnit = nodeWithUnit.unit;
                                    // Get nodes in the area that the AI unit could heal friendly units from.
                                    List<Node> nodesCastable = Grid.m_Instance.GetNodesWithinRadius(skill.m_CastableDistance, nodeWithUnit);

                                    // Go through each of the nodes the AI unit could heal from and add the heal heuristic to them.
                                    for (int j = 0; j < nodesCastable.Count; j++)
                                    {
                                        // Calculate the new heal for the node's heal heuristic.
                                        float newHealH = hs.m_HealAmount + (currentUnit.GetStartingHealth() - currentUnit.GetCurrentHealth());
                                        if (newHealH < FindHeuristic(nodesCastable[j], unit)?.m_HealingValue)
                                        {
                                            continue;
                                        }
                                        else
                                        {
                                            if (nodesCastable[j] != null)
                                            {
                                                Node currentHealNode = nodesCastable[j];
                                                AddHeuristics(AIHeuristics.Heal, newHealH, currentHealNode, unit, healSkill: hs);
                                                currentHealNode.SetAITarget(nodeWithUnit.unit);
                                                m_OptimalSkill = skill;
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            List<HeuristicResult> bestChoices = m_HeuristicResults.OrderByDescending(hr => hr.SumHeuristics()).ToList();

            for (int i = 0; i < bestChoices.Count; ++i)
            {
                Unit currentAI = bestChoices[i].m_Unit;
                List<Node> nodesInMovement = Grid.m_Instance.GetNodesWithinRadius(currentAI.GetCurrentMovement(), Grid.m_Instance.GetNode(currentAI.transform.position));
                foreach(Node n in nodesInMovement)
				{
                    if (n == bestChoices[i].m_Node)
					{
                        m_CurrentAIUnit = bestChoices[i].m_Unit;
                        GameManager.m_Instance.m_SelectedUnit = m_CurrentAIUnit;

                        m_OptimalNode = bestChoices[i].m_Node;
                        m_CurrentAIUnit.DecreaseCurrentMovement(bestChoices[i].m_MoveDistance);
                        FindPathToOptimalNode();
                        return;
                    }
				}
            }
            Debug.LogError("No node found for " + m_CurrentAIUnit + " to move to!");
                //HeuristicResult bestChoice = m_HeuristicResults.OrderByDescending(hr => hr.SumHeuristics()).First();
                /*m_CurrentAIUnit = bestChoice.m_Unit;
                GameManager.m_Instance.m_SelectedUnit = m_CurrentAIUnit;

                m_OptimalNode = bestChoice.m_Node;
                m_CurrentAIUnit.DecreaseCurrentMovement(bestChoice.m_MoveDistance);*/
            //FindPathToOptimalNode();

            /*print($"{bestChoice.m_Unit} is attempting to move to {bestChoice.m_Node.m_NodeHighlight.name} with a cost of {bestChoice.m_MoveDistance}\n" +
                $"Damage: {bestChoice.m_DamageValue}/{bestChoice.m_DamageSkill}\n" +
                $"Heal: {bestChoice.m_HealingValue}/{bestChoice.m_HealSkill}");*/
        }
    }

    void DoMovementHeuristics(Unit unit)
	{
        foreach (Unit playerUnit in UnitsManager.m_Instance.m_PlayerUnits)
        {
            Stack<Node> path = new Stack<Node>();
            if (!Grid.m_Instance.FindPath(unit.transform.position, playerUnit.transform.position, out path, out int pathCost, allowBlocked: true))
            {
                Debug.LogError("Pathfinding couldn't find a path between AI unit " + unit.name + " and " + playerUnit.name + ".");
                continue;
            }

            // Go through path to closest unit, assign movement heuristic to normalized position on the stack of the path.
            // Will favour shortest path.

            int pathLength = path.Count - 1;

            for (int j = 0; j < pathLength; ++j)
            {
                Node n = path.Pop();
                AddHeuristics(AIHeuristics.Move, (float)j / pathLength, n, unit, pathLength);
            }
        }
    }

    void AddHeuristics(AIHeuristics type, float value, Node n, Unit u, int d = 0, HealSkill healSkill = null, DamageSkill damageSkill = null)
    {
        HeuristicResult hr = FindHeuristic(n, u);
        if (hr != null)
        {
            switch (type)
            {
                case AIHeuristics.Move:
                    hr.m_MovementValue += value;
                    break;
                case AIHeuristics.Attack:
                    hr.m_DamageValue += value;
                    break;
                case AIHeuristics.Heal:
                    hr.m_HealingValue += value;
                    break;
                case AIHeuristics.StatusEffect:
                    hr.m_StatusValue += value;
                    break;
                default:
                    break;
            }
            if (healSkill)
            {
                hr.m_HealSkill = healSkill;
            }
            if (damageSkill)
            {
                hr.m_DamageSkill = damageSkill;
            }
            if (d > 0)
            {
                hr.m_MoveDistance += d;
            }
        }
        else
        {
            m_HeuristicResults.Add(new HeuristicResult(u, type, value, n, d, healSkill, damageSkill));
        }
    }

    HeuristicResult FindHeuristic(Node n, Unit u)
    {
        if (m_HeuristicResults.Exists(e => e.m_Node == n && e.m_Unit == u))
        {
            return m_HeuristicResults.Find(e => e.m_Node == n && e.m_Unit == u);
        }
        else return null;
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
                m_CurrentAIUnit.m_ActionOnFinishPath = OnFinishMoving;
        }
    }

    public void OnFinishMoving(Unit u)
    {
        if (m_OptimalNode.GetDamage() > 0 || m_OptimalNode.GetHealing() > 0)
            CheckAttackRange(u);
        m_CurrentAIUnit = null;
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
}
