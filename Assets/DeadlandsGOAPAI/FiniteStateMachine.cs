using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FiniteStateMachine
{
    public delegate void State(FiniteStateMachine fsm, GameObject gameObject);
    private Stack<State> stateStack = new Stack<State>();


    public void InvokeCurrentState(GameObject gameObject)
    {
        if (stateStack.Peek() != null)
            stateStack.Peek().Invoke(this, gameObject);
    }

    public void PushState(State state)
    {
        stateStack.Push(state);
    }

    public void PopState()
    {
        stateStack.Pop();
    }
}
