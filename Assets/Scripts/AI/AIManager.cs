using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    Unit enemyAI;

    private void Update()
    {
        if (GetCurrentTurn() == Allegiance.Enemy)
        {
            GetClosestPlayerUnit();
        }
    }

    //Gets whose turn it is.
    public Allegiance GetCurrentTurn()
    {
        return GameManager.m_Instance.GetCurrentTurn();
    }

    //Finds the closest player unit.
    public void GetClosestPlayerUnit()
    {
        foreach (Unit units in GameManager.m_Instance.m_PlayerUnits)
        {
            
        }
    }

    //Calculates a path to the closest unit.
    public void CalculatePath()
    {
        
    }

    //Attacks the unit.
    public void Attack()
    {
        
    }
}
