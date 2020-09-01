using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    //Instance of the AIManager.
    public static AIManager m_Instance = null;

    //Unit's that track the closest Unit that is controlled by the player, and a Unit for the current AI Unit.
    private Unit m_ClosestPlayerUnit;
    private Unit m_CurrentAIUnit;

    //The path for the AI to walk on.
    public Stack<Node> m_Path = new Stack<Node>();

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
        Debug.Log($"Taking AI Turn: {UnitsManager.m_Instance.m_ActiveEnemyUnits.Count} units");

        // Prune the active units
        DisableUnits(UnitsManager.m_Instance.m_ActiveEnemyUnits.Where(u => u.GetCurrentHealth() <= 0).ToList());

        // For each AI unit currently alive.
        foreach (Unit unit in UnitsManager.m_Instance.m_ActiveEnemyUnits)
        {
            // The current AI unit is assigned
            m_CurrentAIUnit = unit;
            GameManager.m_Instance.m_SelectedUnit = unit;

            // Perform the actions on their turn.
            m_ClosestPlayerUnit = FindClosestPlayerUnit();

            if (m_ClosestPlayerUnit)
            {
                FindPathToPlayerUnit();
            }
        }

        //Tell the game manager it is not our turn anymore.
        GameManager.m_Instance.EndCurrentTurn();
    }

    /// <summary>
    /// Returns the closest player controlled unit to the AI.
    /// </summary>
    public Unit FindClosestPlayerUnit()
    {
        Dictionary<Unit, int> unitDistances = new Dictionary<Unit, int>();

        // Find out how far away each unit is and store it in the Dictionary
        foreach (Unit playerUnit in UnitsManager.m_Instance.m_PlayerUnits)
        {
            Stack<Node> refPath = new Stack<Node>();
            if (Grid.m_Instance.FindPath(m_CurrentAIUnit.transform.position, playerUnit.transform.position, ref refPath, out int dist))
            {
                unitDistances.Add(playerUnit, dist);
                Debug.Log($"{playerUnit.name}: {dist} tiles from {m_CurrentAIUnit.name}");
            }
        }

        if (unitDistances.Count == 0) return null;

        // If you want to select randomly from the closest characters.
        // The current implementation returns the first character on the list in the case of a tie.
        //var closestUnits = unitDistances.Where(ud1 => ud1.Value == unitDistances.Min(ud2 => ud2.Value));
        //return closestUnits.ElementAt(Random.Range(0, closestUnits.Count())).Key;

        // See https://stackoverflow.com/questions/2805703/good-way-to-get-the-key-of-the-highest-value-of-a-dictionary-in-c-sharp
        // for a description of what this is
        return unitDistances.Aggregate((next, lowest) => next.Value < lowest.Value ? next : lowest).Key;
    }

    //Finds the path from the two units and sets the AI movement path.
    // Could probably be rewritten
    public void FindPathToPlayerUnit()
    {
        if (Grid.m_Instance.FindPath(m_CurrentAIUnit.transform.position, m_ClosestPlayerUnit.transform.position, ref m_Path, out int pathCost))
        {
            // Duct tape and hot glue gun code to get it working
            Stack<Node> path = new Stack<Node>(m_Path.Intersect(Grid.m_Instance.GetNodesWithinRadius(m_CurrentAIUnit.GetCurrentMovement(), Grid.m_Instance.GetNode(m_CurrentAIUnit.transform.position))).Reverse());
            m_CurrentAIUnit.SetMovementPath(path);
            //m_CurrentAIUnit.SetMovementPath(new Stack<Node>(m_Path.Take(Mathf.Min(m_CurrentAIUnit.GetCurrentMovement() + 1, m_Path.Count)).Reverse()));
            Debug.Log(string.Join(", ", m_CurrentAIUnit.GetMovementPath().Select(n => n.m_NodeHighlight.name)));
            m_CurrentAIUnit.m_ActionOnFinishPath = CheckAttackRange;
        }
    }

    // Checks adjacent nodes of the AI unit to see if they are able to attack and hit the player.
    // This is not expandable for other units. consider loking at all the nodes in range like in GameManager.cs - James L
    public void CheckAttackRange()
    {
        for (int i = 0; i < 4; i++)
        {
            Node node = Grid.m_Instance.GetNode(m_CurrentAIUnit.transform.position).adjacentNodes[i];
            if (node.unit?.m_Allegiance == Allegiance.Player)
            {
                Attack(node);
                break;
            }
        }
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
        foreach (Unit deadUnit in deadUnits)
        {
            UnitsManager.m_Instance.m_ActiveEnemyUnits.Remove(deadUnit);
        }
    }
}
