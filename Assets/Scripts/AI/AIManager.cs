using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class DamageSkillTarget
{
    public DamageSkill m_Skill;
    public Node m_TargetNode;

    public DamageSkillTarget(DamageSkill damageSkill, Node node)
    {
        m_Skill = damageSkill;
        m_TargetNode = node;
    }
}
[Serializable]
public class HealSkillTarget
{
    public HealSkill m_Skill;
    public Node m_TargetNode;

    public HealSkillTarget(HealSkill healSkill, Node node)
    {
        m_Skill = healSkill;
        m_TargetNode = node;
    }
}
[Serializable]
public class StatusSkillTarget
{
    public StatusSkill m_Skill;
    public Node m_TargetNode;

    public StatusSkillTarget(StatusSkill statusSkill, Node node)
    {
        m_Skill = statusSkill;
        m_TargetNode = node;
    }
}

[Serializable]
public class HeuristicResult
{
    public Unit m_Unit;
    public Node m_Node;

    public float m_MovementValue = 0;
    public float m_DamageValue = 0;
    public float m_HealValue = 0;
    public float m_StatusValue = 0;

    public int m_MoveDistance;
    public DamageSkillTarget m_DamageSkill;
    public HealSkillTarget m_HealSkill;
    public StatusSkillTarget m_StatusSkill;

    // Constructors
    public HeuristicResult(Unit u, Node n, float hv, int d)
    {
        m_MovementValue += hv;
        m_Unit = u;
        m_Node = n;
        m_MoveDistance = d;
    }
    public HeuristicResult(Unit u, Node n, float hv, DamageSkillTarget ds)
    {
        m_DamageValue = hv;
        m_Unit = u;
        m_Node = n;
        m_DamageSkill = ds;
    }
    public HeuristicResult(Unit u, Node n, float hv, HealSkillTarget hs)
    {
        m_DamageValue = hv;
        m_Unit = u;
        m_Node = n;
        m_HealSkill = hs;
    }
    public HeuristicResult(Unit u, Node n, float hv, StatusSkillTarget ss)
    {
        m_DamageValue = hv;
        m_Unit = u;
        m_Node = n;
        m_StatusSkill = ss;
    }

    public float SumHeuristics() => m_MovementValue + m_DamageValue + m_HealValue + m_StatusValue;
    public float SumSkillHeuristics() => m_DamageValue + m_HealValue + m_StatusValue;
}

public class AIManager : MonoBehaviour
{
    //Instance of the AIManager.
    public static AIManager m_Instance = null;

    //Unit's that track the closest Unit that is controlled by the player, and a Unit for the current AI Unit.
    private Unit m_CurrentAIUnit;

    //The path for the AI to walk on.
    public Stack<Node> m_Path = new Stack<Node>();

    private bool m_AITurn = false;

    private List<HeuristicResult> m_HeuristicResults = new List<HeuristicResult>();

    public HeuristicResult m_BestOption;

    List<Unit> m_UnitCloseList = new List<Unit>();

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
        if (!m_CurrentAIUnit) // If no AI unit is currently taking their turn
        {
            m_HeuristicResults.Clear(); // Get rid of the cached heuristics
            // Calculate all the new heuristics for each unit
            foreach (Unit unit in UnitsManager.m_Instance.m_ActiveEnemyUnits)
            {
                // Only calculate movement heuristics if the unit can move
                if (unit.GetCurrentMovement() > 0)
                {
                    // Movement Heuristics
                    DoMovementHeuristics(unit);
                }

                // Only calculate skill heuristics if the unit has actions
                if (unit.GetActionPoints() > 0)
                {
                    foreach (BaseSkill skill in unit.GetSkills())
                    {
                        // Make sure the skill isn't on cooldown.
                        if (skill.GetCurrentCooldown() > 0) continue;

                        List<Node> nodesWithUnits = Grid.m_Instance.GetNodesWithinRadius(
                            skill.m_AffectedRange + skill.m_CastableDistance + unit.GetCurrentMovement(),
                            Grid.m_Instance.GetNode(unit.transform.position),
                            true
                        ).Where(n => n.unit != null).ToList();

                        switch (skill)
                        {
                            case StatusSkill ss:
                                DoStatusHeuristic(nodesWithUnits, ss, unit);
                                break;
                            case DamageSkill ds:
                                DoAttackHeuristic(nodesWithUnits, ds, unit);
                                break;
                            case HealSkill hs:
                                DoHealHeuristic(nodesWithUnits, hs, unit);
                                break;
                            default:
                                Debug.LogError("Bad skill!", skill);
                                break;
                        }
                    }
                }
            }

            if (m_HeuristicResults.Count == 0)
            {
                // No AI moves left, end your turn.
                GameManager.m_Instance.EndCurrentTurn();
                return;
            }

            #region James
            /*
            // Sort all the moves regardless of whether it's reachable
            List<HeuristicResult> sortedChoices = m_HeuristicResults.OrderByDescending(hr => hr.SumHeuristics()).ToList();

            // Get all the nodes the unit could move to.
            List<Node> nodesInMovement = Grid.m_Instance.GetNodesWithinRadius(m_CurrentAIUnit.GetCurrentMovement(), Grid.m_Instance.GetNode(m_CurrentAIUnit.transform.position));

            // Filter results to only reachable
            m_HeuristicResults = m_HeuristicResults.Where(hr => nodesInMovement.Contains(hr.m_Node)).ToList();

            if (m_HeuristicResults.Count == 0)
            {
                // No AI moves left, end your turn.
                GameManager.m_Instance.EndCurrentTurn();
                return;
            }

            m_BestOption = m_HeuristicResults.OrderByDescending(hr => hr.SumHeuristics()).First();
            m_CurrentAIUnit = m_BestOption.m_Unit;
            GameManager.m_Instance.m_SelectedUnit = m_CurrentAIUnit;

            // Check if the current best node is within the movement range of the unit.
            if (nodesInMovement.Contains(m_BestOption.m_Node))
            {
                m_CurrentAIUnit = m_BestOption.m_Unit;
                GameManager.m_Instance.m_SelectedUnit = m_CurrentAIUnit;
                m_CurrentAIUnit.DecreaseCurrentMovement(m_BestOption.m_MoveDistance);
                FindPathToOptimalNode();
                return;
            }
            if (!m_CurrentAIUnit)
            {
                // Assume no more units left.
                GameManager.m_Instance.EndCurrentTurn();
                return;
            }
            */
            #endregion

            #region Grant
            
            List<HeuristicResult> sortedChoices = m_HeuristicResults.OrderByDescending(hr => hr.SumHeuristics()).ToList();

            for (int i = 0; i < sortedChoices.Count; ++i)
            {
                m_BestOption = sortedChoices[i];

                // Make sure we don't do an AI unit's turn twice.
                if (m_UnitCloseList.Contains(m_BestOption.m_Unit)) continue;

                Unit currentAI = m_BestOption.m_Unit;

                // Get all the nodes the unit could move to.
                List<Node> nodesInMovement = Grid.m_Instance.GetNodesWithinRadius(currentAI.GetCurrentMovement(), Grid.m_Instance.GetNode(currentAI.transform.position));

                // Check if the current best node is within the movement range of the unit.
                if (nodesInMovement.Contains(m_BestOption.m_Node))
                {
                    m_CurrentAIUnit = m_BestOption.m_Unit;
                    GameManager.m_Instance.m_SelectedUnit = m_CurrentAIUnit;
                    m_CurrentAIUnit.DecreaseCurrentMovement(m_BestOption.m_MoveDistance);
                    FindPathToOptimalNode();
                    m_UnitCloseList.Add(currentAI);
                    return;
                }
            }
            if (!m_CurrentAIUnit)
            {
                // Assume no more units left.
                GameManager.m_Instance.EndCurrentTurn();
                return;
            }
            
            #endregion
        }
    }

    void DoMovementHeuristics(Unit aiUnit)
    {
        // Find path to each player character
        foreach (Unit playerUnit in UnitsManager.m_Instance.m_PlayerUnits)
        {
            if (!Grid.m_Instance.FindPath(aiUnit.transform.position, playerUnit.transform.position, out Stack<Node> path, out int pathCost, allowBlocked: true))
            {
                Debug.LogError("Pathfinding couldn't find a path between AI unit " + aiUnit.name + " and " + playerUnit.name + ".");
                continue;
            }

            // Go through path to closest unit, assign movement heuristic to normalized position on the stack of the path.
            // Will favour shortest path.

            int pathLength = path.Count - 1;

            for (int j = 0; j < pathLength; ++j)
            {
                Node n = path.Pop();
                AddOrUpdateHeuristic(
                    (float)j / pathLength,
                    n,
                    aiUnit,
                    pathLength);
            }
        }
    }

    void DoStatusHeuristic(List<Node> nodesWithUnits, StatusSkill ss, Unit aiUnit)
    {
        foreach (Node nodeWithUnit in nodesWithUnits)
        {
            // Targets AI units.
            if (ss.targets == SkillTargets.Allies)
            {
                // If unit is an AI unit.
                if (nodeWithUnit.unit.m_Allegiance == aiUnit.GetAllegiance())
                {
                    Unit currentUnit = nodeWithUnit.unit;

                    // Get nodes in the area that the AI unit could inflict a status on units from.
                    List<Node> nodesCastable = Grid.m_Instance.GetNodesWithinRadius(ss.m_CastableDistance, nodeWithUnit);

                    // Go through each of the nodes the AI unit could inflict a status from and add the status heuristic to them.
                    for (int j = 0; j < nodesCastable.Count; j++)
                    {
                        Node castNode = nodesCastable[j];

                        // Try to put a status on the healthiest unit, to get the most value.
                        float newStatusH = currentUnit.GetCurrentHealth();
                        if (newStatusH < FindHeuristic(castNode, aiUnit)?.m_StatusValue)
                            continue;
                        else
                        {
                            if (castNode != null)
                            {

                                AddOrUpdateHeuristic(
                                    newStatusH * Vector3.Distance(nodeWithUnit.worldPosition, castNode.worldPosition),
                                    castNode,
                                    aiUnit,
                                    new StatusSkillTarget(ss, nodeWithUnit)); ;
                            }
                        }
                    }
                }
            }
            // Targets Player units.
            else if (ss.targets == SkillTargets.Foes)
            {
                // If unit is a Player unit.
                if (nodeWithUnit.unit.m_Allegiance != aiUnit.GetAllegiance())
                {
                    Unit currentUnit = nodeWithUnit.unit;

                    // Get nodes in the area that the AI unit could inflict a status on units from.
                    List<Node> nodesCastable = Grid.m_Instance.GetNodesWithinRadius(ss.m_CastableDistance, nodeWithUnit);

                    // Go through each of the nodes the AI unit could inflict a status from and add the status heuristic to them.
                    for (int j = 0; j < nodesCastable.Count; j++)
                    {
                        Node castNode = nodesCastable[j];

                        // Try to put a status on the healthiest unit, to get the most value.
                        float newStatusH = currentUnit.GetCurrentHealth();
                        if (newStatusH < FindHeuristic(castNode, aiUnit)?.m_StatusValue)
                            continue;
                        else
                        {
                            if (nodesCastable[j] != null)
                            {
                                AddOrUpdateHeuristic(
                                    newStatusH * Vector3.Distance(nodeWithUnit.worldPosition, castNode.worldPosition),
                                    castNode,
                                    aiUnit,
                                    new StatusSkillTarget(ss, nodeWithUnit));
                            }
                        }
                    }
                }
            }
        }
    }

    void DoAttackHeuristic(List<Node> nodesWithUnits, DamageSkill ds, Unit aiUnit)
    {
        foreach (Node nodeWithUnit in nodesWithUnits)
        {
            // Only accept units that are the other allegiance to attack
            if (nodeWithUnit.unit?.GetAllegiance() != aiUnit.GetAllegiance())
            {
                // Get nodes in the area that the AI unit could hit the player unit from.
                List<Node> nodesCastable = Grid.m_Instance.GetNodesWithinRadius(ds.m_CastableDistance, nodeWithUnit);

                // Go through each of the nodes the AI unit could attack from and add the attack heuristic to them.
                for (int j = 0; j < nodesCastable.Count; j++)
                {
                    Node castNode = nodesCastable[j];

                    // Calculate the new damage for the node's damage heuristic.
                    float hValue = ds.m_DamageAmount * (Vector3.Distance(nodeWithUnit.worldPosition, castNode.worldPosition) * 0.1f);

                    if (castNode != null)
                    {
                        AddOrUpdateHeuristic(
                            hValue,
                            castNode,
                            aiUnit,
                            new DamageSkillTarget(ds, nodeWithUnit));
                    }
                }

                // Check for kill heuristic.
                if (nodeWithUnit.unit.GetCurrentHealth() <= ds.m_DamageAmount)
                {
                    AddOrUpdateHeuristic(
                        aiUnit.m_AIHeuristicCalculator.m_KillPoints,
                        nodeWithUnit,
                        aiUnit,
                        new DamageSkillTarget(ds, nodeWithUnit));
                }
            }
        }
    }

    void DoHealHeuristic(List<Node> nodesWithUnits, HealSkill hs, Unit aiUnit)
    {
        foreach (Node nodeWithUnit in nodesWithUnits)
        {
            // Only accept units that are the same allegiance to heal
            if (nodeWithUnit.unit?.GetAllegiance() == aiUnit.GetAllegiance())
            {
                Unit currentUnit = nodeWithUnit.unit;
                // Get nodes in the area that the AI unit could heal friendly units from.
                List<Node> nodesCastable = Grid.m_Instance.GetNodesWithinRadius(hs.m_CastableDistance, nodeWithUnit);

                // Go through each of the nodes the AI unit could heal from and add the heal heuristic to them.
                for (int j = 0; j < nodesCastable.Count; j++)
                {
                    Node castNode = nodesCastable[j];

                    // Calculate the value for the node's heal heuristic.
                    float newHealH = hs.m_HealAmount + (currentUnit.GetStartingHealth() - currentUnit.GetCurrentHealth());

                    if (castNode != null)
                    {
                        AddOrUpdateHeuristic(
                            newHealH,
                            castNode,
                            aiUnit,
                            new HealSkillTarget(hs, nodeWithUnit));
                    }
                }
            }
        }
    }

    /// <summary>
    /// Updates the movement values of a heuristic result or adds one if it doesn't exist
    /// </summary>
    /// <param name="value"></param>
    /// <param name="n"></param>
    /// <param name="u"></param>
    /// <param name="distance"></param>
    private void AddOrUpdateHeuristic(float value, Node node, Unit unit, int distance)
    {
        HeuristicResult hr = FindHeuristic(node, unit);

        if (hr != null) // If the heuristic already exists, update it
        {
            // If the existing heuristic already a better score, move on
            if (hr.m_MovementValue >= value) return;

            // Otherwise set values
            hr.m_MovementValue += value;
            hr.m_MoveDistance += distance;
        }
        else // Otherwise create a new heuristic with the values
        {
            m_HeuristicResults.Add(new HeuristicResult(unit, node, value, distance));
        }
    }

    /// <summary>
    /// Updates the damage values of a heuristic result or adds one if it doesn't exist
    /// </summary>
    /// <param name="value"></param>
    /// <param name="node"></param>
    /// <param name="unit"></param>
    /// <param name="damageSkill"></param>
    private void AddOrUpdateHeuristic(float value, Node node, Unit unit, DamageSkillTarget damageSkill)
    {
        HeuristicResult hr = FindHeuristic(node, unit);

        if (hr != null) // If the heuristic already exists, update it
        {
            // If the existing heuristic already a better score, move on
            if (hr.m_DamageValue >= value) return;

            // Otherwise set values
            hr.m_DamageValue += value;
            hr.m_DamageSkill = damageSkill;
        }
        else // Otherwise create a new heuristic with the values
        {
            m_HeuristicResults.Add(new HeuristicResult(unit, node, value, damageSkill));
        }
    }

    /// <summary>
    /// Updates the heal values of a heuristic result or adds one if it doesn't exist
    /// </summary>
    /// <param name="value"></param>
    /// <param name="node"></param>
    /// <param name="unit"></param>
    /// <param name="healSkill"></param>
    private void AddOrUpdateHeuristic(float value, Node node, Unit unit, HealSkillTarget healSkill)
    {
        HeuristicResult hr = FindHeuristic(node, unit);

        if (hr != null) // If the heuristic already exists, update it
        {
            // If the existing heuristic already a better score, move on
            if (hr.m_HealValue >= value) return;

            // Otherwise set values
            hr.m_HealValue += value;
            hr.m_HealSkill = healSkill;
        }
        else // Otherwise create a new heuristic with the values
        {
            m_HeuristicResults.Add(new HeuristicResult(unit, node, value, healSkill));
        }
    }

    /// <summary>
    /// Updates the skill values of a heuristic result or adds one if it doesn't exist
    /// </summary>
    /// <param name="value"></param>
    /// <param name="node"></param>
    /// <param name="unit"></param>
    /// <param name="statusSkill"></param>
    private void AddOrUpdateHeuristic(float value, Node node, Unit unit, StatusSkillTarget statusSkill)
    {
        HeuristicResult hr = FindHeuristic(node, unit);

        if (hr != null) // If the heuristic already exists, update it
        {
            // If the existing heuristic already a better score, move on
            if (hr.m_StatusValue >= value) return;

            // Otherwise set values
            hr.m_StatusValue += value;
            hr.m_StatusSkill = statusSkill;
        }
        else  // Otherwise create a new heuristic with the values
        {
            m_HeuristicResults.Add(new HeuristicResult(unit, node, value, statusSkill));
        }
    }

    public HeuristicResult FindHeuristic(Node n, Unit u)
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
        print("finding path");
        if (Grid.m_Instance.FindPath(m_CurrentAIUnit.transform.position, m_BestOption.m_Node.worldPosition, out m_Path, out int pathCost))
        {
            m_CurrentAIUnit.SetMovementPath(m_Path);
            Debug.Log(m_CurrentAIUnit.name + ": " + string.Join(", ", m_CurrentAIUnit.GetMovementPath().ToList().Select(n => n.m_NodeHighlight.name)));
            // Have the unit check what to do when it reaches its destination.
            m_CurrentAIUnit.m_ActionOnFinishPath = OnFinishMoving;
        }
    }

    public void OnFinishMoving()
    {
        CheckSkills();
        // TODO move this so it resets when all the unit has *finished* their turn, including damage calcs
        m_CurrentAIUnit = null;
    }

    void CheckSkills()
    {
        // If the best option contains a skill
        if (m_BestOption.SumSkillHeuristics() > 0)
        {
            if (m_BestOption.m_HealValue >= m_BestOption.m_DamageValue && m_BestOption.m_HealValue >= m_BestOption.m_StatusValue)
            {
                m_CurrentAIUnit.ActivateSkill(m_BestOption.m_HealSkill.m_Skill, m_BestOption.m_HealSkill.m_TargetNode);
            }
            else if (m_BestOption.m_StatusValue >= m_BestOption.m_DamageValue)
            {
                m_CurrentAIUnit.ActivateSkill(m_BestOption.m_StatusSkill.m_Skill, m_BestOption.m_StatusSkill.m_TargetNode);
            }
            else
            {
                m_CurrentAIUnit.ActivateSkill(m_BestOption.m_DamageSkill.m_Skill, m_BestOption.m_DamageSkill.m_TargetNode);
            }
        }
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

        // Set them onto the visible layer
        foreach (Unit unit in newUnits)
        {
            foreach (Transform t in unit.GetComponentsInChildren<Transform>(true))
            {
                t.gameObject.layer = 9;
            }
            if (unit.m_SummonParticle)
            {
                unit.m_SummonParticle.Play();
            }
        }

        // In case of units already added being in the list, remove dupes.
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
            // Clear the unit closed list, to be able to go through all the units now.
            m_UnitCloseList.Clear();

            // Prune the active units
            DisableUnits(UnitsManager.m_Instance.m_ActiveEnemyUnits.Where(u => u.GetCurrentHealth() <= 0).ToList());

            Debug.Log($"Taking AI Turn: {UnitsManager.m_Instance.m_ActiveEnemyUnits.Count} units");
        }
    }


    [ContextMenu("Print Best Node")]
    void PrintNode() => Debug.Log(m_BestOption.m_Node.m_NodeHighlight.name);
}
