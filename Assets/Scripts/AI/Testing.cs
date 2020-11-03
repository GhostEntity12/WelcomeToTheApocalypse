using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour
{
    private bool aiTurn = false;
    private List<HeuristicResult> heuristicResults = new List<HeuristicResult>();

    //Perhaps we may be able to simplify the update function to something as simply as this.
    private void Update()
    {
        if (aiTurn)
            AITurn();
    }

    //Everything should go in here.
    private void AITurn()
    {
        //As soon as its the AI's turn, clear all the previous turn's heuristics. 
        heuristicResults.Clear();

        foreach (Unit unit in UnitsManager.m_Instance.m_ActiveEnemyUnits)
        {
            CalculateHeuristics(unit);
        }
    }

    void CalculateHeuristics(Unit unit)
    {
        //Maybe something like this.
        /*switch (unit.GetHeuristicCalculator().attribute)
        { 
            case: Damage
                
            case: Heal

            case: Status

        }
        */
    }
}
