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

    //Returns the current blackboard instance.
    public static Blackboard GetInstance()
    {
        return instance;
    }

    public List<Unit> GetEnemyUnits()
    {
        return enemies;
    }

    public List<Unit> GetPlayerUnits()
    {
        return playerUnits;
    }

    public Unit GetEnemySelectedUnit()
    {
        return enemySelectedUnit;
    }

    public Unit GetPlayerSelectedUnit()
    {
        return playerSelectedUnit;
    }

    public Grid GetGrid()
    {
        return grid;
    }

    public Stack<Node> GetPath()
    {
        return path;
    }

    public Allegiance GetTurn()
    {
        return GameManager.m_Instance.GetCurrentTurn();
    }
}
