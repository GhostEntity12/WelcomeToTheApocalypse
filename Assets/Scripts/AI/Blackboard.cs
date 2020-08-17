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
    Unit enemyUnit;
    Grid grid;
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
