using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    //Lists of units that track which AI unit is alive and which is dead.
    private List<Unit> aliveUnits;
    private List<Unit> deadUnits;

    //A list of the player's alive units.
    private List<Unit> playerUnits;

    private Unit closestPlayerUnit;

    //I guess this is needed to tell whos turn it is?
    public bool isTurn;

    private float distance;

    //Init the turn to not begin with the AI.
    private void Start()
    {
        isTurn = false;
    }

    private void Update()
    {
        //A quick check to see who is alive.
        CheckWhoRemains(aliveUnits);

        //For each AI unit currently alive.
        foreach (Unit unit in aliveUnits)
        {
            //Perform the actions on their turn.
            FindClosestPlayerUnit(playerUnits);
            FindPathToPlayerUnit();
            CheckAttackRange();
        }

        //End the turn.
        isTurn = false;
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
        for (int i = 0; i < playersUnits.Count; i++)
        {
            //The distance is done through transform matrix for now instead of node distance.
            distance = Vector3.Distance(playersUnits[i].transform.position, transform.position);

            closestPlayerUnit = playersUnits[i];

            //If the distance of the next one is less than the one just found and the next element in the list is not null. Assign this to be the closest unit.
            if (Vector3.Distance(playersUnits[i + 1].transform.position, transform.position) < distance && playersUnits[i + 1] != null)
            {
                distance = Vector3.Distance(playersUnits[i + 1].transform.position, transform.position);
                closestPlayerUnit = playersUnits[i + 1];
            }
        }

        //Return the closest player controlled unit to that AI.
        return closestPlayerUnit;
    }

    public void FindPathToPlayerUnit()
    {
        Grid.m_Instance.
    }

    public void CheckAttackRange()
    {

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
}
