using UnityEngine;
using System;

public abstract class AStateContext
{
    private IState _currentState;
    public readonly Character _current;

    public AStateContext(Character current)
    {
        _current = current;
    }

    public virtual void Init(IState state)
    {
        _currentState = state;
        _currentState.EnterState();
    }

    public virtual void ChangeState(IState state)
    {
        if (state == null)
        {

            return;    
        }

        _currentState.ExitState();
        _currentState = state;
        _currentState.EnterState();
    }

    public IState GetState() { return _currentState; }
}
