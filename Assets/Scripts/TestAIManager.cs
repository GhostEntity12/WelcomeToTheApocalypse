using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.UI;

public class TestAIManager : MonoBehaviour
{
    //Instance of the AIManager.
    public static TestAIManager m_Instance = null;

    //Unit's that track the closest Unit that is controlled by the player, and a Unit for the current AI Unit.
    private Unit m_CurrentAIUnit;

    //The path for the AI to walk on.
    public Stack<Node> m_Path = new Stack<Node>();

    private Node m_OptimalNode = new Node();

    private bool m_AITurn = false;

    private BaseSkill m_OptimalSkill = null;

    List<HeuristicResult> hrs = new List<HeuristicResult>();

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
            hrs.Clear();
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

                                                AddHeuristics(AIHeuristics.Attack, hValue, nodeWithUnit, unit);
                                                currentDamageNode.SetAITarget(nodeWithUnit.unit);
                                                m_OptimalSkill = skill;
                                            }
                                        }
                                    }

                                    // Check for kill heuristic.
                                    if (nodeWithUnit.unit.GetCurrentHealth() <= ds.m_DamageAmount)
                                    {
                                        AddHeuristics(AIHeuristics.Attack, unit.m_AIHeuristicCalculator.m_KillPoints, nodeWithUnit, unit);
                                    }
                                }
                            }
                            break;
                        case HealSkill hs:
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
                                                currentHealNode.SetHealing(newHealH * (Vector3.Distance(nodeWithUnit.worldPosition, nodesCastable[j].worldPosition) * 0.1f));
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
            HeuristicResult bestChoice = hrs.OrderByDescending(hr => hr.SumHeuristics()).First();
            m_CurrentAIUnit = bestChoice.m_Unit;
            GameManager.m_Instance.m_SelectedUnit = m_CurrentAIUnit;

            m_OptimalNode = bestChoice.m_Node;
            FindPathToOptimalNode();

        }
    }

    void DoMovementHeuristics(Unit enemyUnit)
    {
        foreach (Unit playerUnit in UnitsManager.m_Instance.m_PlayerUnits)
        {
            Stack<Node> path = new Stack<Node>();
            if (!Grid.m_Instance.FindPath(enemyUnit.transform.position, playerUnit.transform.position, out path, out int pathCost, allowBlocked: true))
            {
                Debug.LogError("Pathfinding couldn't find a path between AI unit " + enemyUnit.name + " and " + playerUnit.name + ".");
                continue;
            }

            // Go through path to closest unit, assign movement heuristic to normalized position on the stack of the path.
            // Will favour shortest path.

            int pathLength = path.Count - 1;

            for (int j = 0; j < pathLength; ++j)
            {
                Node n = path.Pop();
                AddHeuristics(AIHeuristics.Move, (float)j / pathLength, n, enemyUnit);
            }
        }
    }

    void AddHeuristics(AIHeuristics type, float value, Node n, Unit u, HealSkill healSkill = null, DamageSkill damageSkill = null)
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
            hr.m_HealSkill = healSkill;
            hr.m_DamageSkill = damageSkill;
        }
        else
        {
            //hrs.Add(new HeuristicResult(u, type, value, n, healSkill, damageSkill));
        }
    }

    HeuristicResult FindHeuristic(Node n, Unit u) 
    {
        if (hrs.Exists(e => e.m_Node == n && e.m_Unit == u))
        {
            return hrs.Find(e => e.m_Node == n && e.m_Unit == u);
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
