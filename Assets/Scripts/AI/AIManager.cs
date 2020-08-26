using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    //Instance of the AIManager.
    public static AIManager m_Instance = null;

    //Lists of units that track which AI unit is alive and which is dead.
    public List<Unit> aliveUnits;
    public List<Unit> deadUnits;

    //A list of the player's units.
    public List<Unit> playerUnits;

    //Unit's that track the closest Unit that is controlled by the player, and a Unit for the current AI Unit.
    public Unit closestPlayerUnit;
    public Unit currentAIUnit;

    //The path for the AI to walk on.
    public Stack<Node> path = new Stack<Node>();
    public int pathCost;

    //The AI's basic melee attack.
    public BaseSkill skill;

    //I guess this is needed to tell whos turn it is?
    public bool isTurn;

    //Can the AI Attack.
    public bool canAttack;

    //Float used to measure the distance between two units.
    public float distance;

    //On Awake, initialise the instance of this manager.
    private void Awake()
    {
        m_Instance = this;
    }

    //Init the turn to not begin with the AI.
    //It also can't attack yet.
    private void Start()
    {
        pathCost = 0;
        isTurn = false;
        canAttack = false;
    }

    private void Update()
    {
        //If it is the AI's turn.
        if (isTurn)
        {
            //A quick check to see who is alive.
            CheckWhoRemains(aliveUnits);

            //For each AI unit currently alive.
            foreach (Unit unit in aliveUnits)
            {
                //Perform the actions on their turn.
                FindClosestPlayerUnit(playerUnits);

                //The current AI unit is assigned
                currentAIUnit = unit;

                //Find a path to the player controlled unit and check if we're in attacking range.
                FindPathToPlayerUnit();

                //HERE WE NEED TO FIND A WAY TO MAKE THE SCRIPT NOT CALL THE NEXT FUNCTION UNTIL THE AI UNIT HAS REACHED WALKED ITS PATH.

                CheckAttackRange();

                //If we can attack. Attack. TEMP
                if (canAttack)
                {
                    print("In range");
                    Attack();
                }
            }

            //End the turn.
            isTurn = false;

            //Tell the game manager it is not our turn anymore.
            GameManager.m_Instance.EndCurrentTurn();
        }
    }

    //When a unit dies, remove it from the list of alive units and add it to a list of dead units.
    public void UnitDeath(Unit unit)
    {
        aliveUnits.Remove(unit);
        deadUnits.Add(unit);
    }

    //Returns the closest player controlled unit to the AI.
    public Unit FindClosestPlayerUnit(List<Unit> playersUnits)
    {
        //Iterate through the list of player controlled units.
        for (int i = 0; i < playersUnits.Count - 1; i++)
        {
            //The distance is done through transform matrix for now instead of node distance.
            distance = Vector3.Distance(playersUnits[i].transform.position, transform.position);

            closestPlayerUnit = playersUnits[i];

            //If the distance of the next one is less than the one just found and the next element in the list is not null. Assign this to be the closest unit.
            if (Vector3.Distance(playersUnits[i + 1].transform.position, transform.position) < distance && playersUnits[i] != null)
            {
                distance = Vector3.Distance(playersUnits[i + 1].transform.position, transform.position);
                closestPlayerUnit = playersUnits[i + 1];
            }
        }

        //Return the closest player controlled unit to that AI.
        return closestPlayerUnit;
    }

    //Finds the path from the two units and sets the AI movement path.
    public void FindPathToPlayerUnit()
    {
        if (Grid.m_Instance.FindAIPath(currentAIUnit.transform.position, closestPlayerUnit.transform.position, ref path, out pathCost))
        {
            currentAIUnit.SetMovementPath(path);

        }
    }

    //Checks adjacent nodes of the AI unit to see if they are able to attack and hit the player.
    public void CheckAttackRange()
    {
        for (int i = 0; i < 4; i++)
        {
            if (Grid.m_Instance.GetNode(currentAIUnit.transform.position).adjacentNodes[i].unit?.m_Allegiance == Allegiance.Player)
            {
                canAttack = true;
            }
            else
            {
                canAttack = false;
            }
        }
    }

    //Function to activate the unit's attack.
    public void Attack()
    {
        if (canAttack)
        {
            currentAIUnit.ActivateSkill(skill);
        }
    }

    //At the beginning of the AI turn, check who is alive, sort out any dead member who may be in the alive list. (Just in case)
    public void CheckWhoRemains(List<Unit> units)
    {
        foreach (Unit unit in units)
        {
            if (unit.GetCurrentHealth() <= 0)
                UnitDeath(unit);
        }
    }

    //Make it our turn.
    public void AICurrentTurn()
    {
        isTurn = true;
    }
}
