﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    public static AIManager m_Instance = null;

    //Lists of units that track which AI unit is alive and which is dead.
    public List<Unit> aliveUnits;
    public List<Unit> deadUnits;

    //A list of the player's units.
    public List<Unit> playerUnits;

    public Unit closestPlayerUnit;
    public Unit currentAIUnit;

    public Stack<Node> path = new Stack<Node>();
    public int pathCost;

    public BaseSkill skill;

    //I guess this is needed to tell whos turn it is?
    public bool isTurn;

    public bool canAttack;

    public float distance;

    private void Awake()
    {
        m_Instance = this;
    }

    //Init the turn to not begin with the AI.
    private void Start()
    {
        pathCost = 0;
        isTurn = false;
        canAttack = false;
    }

    private void Update()
    {
        if (isTurn)
        {
            //A quick check to see who is alive.
            CheckWhoRemains(aliveUnits);

            //For each AI unit currently alive.
            foreach (Unit unit in aliveUnits)
            {
                //Perform the actions on their turn.
                FindClosestPlayerUnit(playerUnits);
                currentAIUnit = unit;
                FindPathToPlayerUnit();
                CheckAttackRange();

                if (CheckAttackRange() == true)
                {
                    Attack();
                }
            }

            //End the turn.
            isTurn = false;

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
        for (int i = 0; i < playersUnits.Count; i++)
        {
            //The distance is done through transform matrix for now instead of node distance.
            distance = Vector3.Distance(playersUnits[i].transform.position, transform.position);

            closestPlayerUnit = playersUnits[i];

            //If the distance of the next one is less than the one just found and the next element in the list is not null. Assign this to be the closest unit.
            if (Vector3.Distance(playersUnits[i].transform.position, transform.position) < distance && playersUnits[i] != null)
            {
                distance = Vector3.Distance(playersUnits[i].transform.position, transform.position);
                closestPlayerUnit = playersUnits[i];
            }
        }

        //Return the closest player controlled unit to that AI.
        return closestPlayerUnit;
    }

    //Finds the path from the two units and sets the AI movement path.
    public void FindPathToPlayerUnit()
    {
        if (Grid.m_Instance.FindPath(currentAIUnit.transform.position, closestPlayerUnit.transform.position, ref path, out pathCost))
        {
            currentAIUnit.SetMovementPath(path);
        }
    }

    //Checks adjacent nodes of the AI unit to see if they are able to attack and hit the player.
    public bool CheckAttackRange()
    {
        for (int i = 0; i < 4; i++)
        {
            if (Grid.m_Instance.GetNode(currentAIUnit.transform.position).adjacentNodes[i].unit?.m_Allegiance != Allegiance.Enemy)
            {
                canAttack = true;
            }
            else
            {
                canAttack = false;
            }
        }

        return false;
    }

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

    public void AICurrentTurn()
    {
        isTurn = true;
    }
}
