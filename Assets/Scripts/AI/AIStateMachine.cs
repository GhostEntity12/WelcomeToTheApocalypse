using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIStateMachine : MonoBehaviour
{
    //3 different states for what the AI would be doing at any given time.
    public enum State
    { 
        WaitingForTurn,
        Attacking,
        Fleeing
    }

    //Initialize the first state of the game to be "waiting".
    State state = State.WaitingForTurn;

    //Changes the current state to a new state.
    public void ChangeState(State newState)
    {
        //Check which state we are currently in.
        switch (state)
        {
            case State.WaitingForTurn:
                break;

            case State.Attacking:
                break;

            case State.Fleeing:
                break;
        }

        //Check what new state we are in.
        switch (newState)
        {
            case State.WaitingForTurn:
                state = State.WaitingForTurn;
                break;

            case State.Attacking:
                state = State.Attacking;
                break;

            case State.Fleeing:
                state = State.Fleeing;
                break;
        }
    }

    //Get the current state.
    public State GetState()
    {
        return state;
    }
}
