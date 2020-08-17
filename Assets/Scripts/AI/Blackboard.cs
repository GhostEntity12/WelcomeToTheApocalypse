using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blackboard : MonoBehaviour
{
    //THE WAY OTHER SCRIPTS SHOULD CALL ON THE BLACKBOARD TO UPDATE WILL BE AS FOLLOWS.
    //Blackboard.GetInstance().*FunctionX*;

    //The instance of the blackboard to be created. This object will handle other outside actions.
    private static Blackboard instance = null;

    //Here will go most of the things that the AI will need to be a functioning player.
    List<Unit> enemies;
    List<Unit> playerUnits;
    Unit enemySelectedUnit;
    Unit playerSelectedUnit;
    Grid grid;
    Stack<Node> path;
    int pathCost;
    
    //Eventually more stuff.

    private State state;

    private enum State
    { 
        PlayerTurn,
        EndingPlayerTurn,
        EnemyTurn
    }


    //On Awake
    //If there is no instance of a blackboard, create one. If there already was one, destroy it.
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        state = State.PlayerTurn;
    }

    private void Update()
    {
        //If it is not the player's turn.
        if (state != State.PlayerTurn)
        {
            //Calculate which enemy unit is closest to a player unit.
            foreach (Unit enemy in enemies)
            {
                if (enemy.m_Allegiance == Allegiance.Enemy)
                {
                    enemySelectedUnit = enemy;
                }

                foreach (Unit playerUnit in playerUnits)
                {
                    if (playerUnit.m_Allegiance == Allegiance.Player)
                    {
                        playerSelectedUnit = playerUnit;
                    }

                    grid.FindPath(enemySelectedUnit.transform.position, playerSelectedUnit.transform.position, ref path, out pathCost);
                }
            }
        }
    }

    //Returns the current blackboard instance.
    Blackboard GetInstance()
    {
        return instance;
    }

    /*THESE FOLLOWING FUNCTIONS ARE EXAMPLES OF FUTURE ADDITIONS TO WHAT THE AI SHOULD BE IN CONTROL OF*/

    Unit GetUnit(Unit unit)
    {
        return unit;
    }

    Grid GetGrid(Grid grid)
    {
        return grid;
    }
}
