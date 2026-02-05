using UnityEngine;
using System;

public abstract class AStateContext
{
    private IState _currentState;
    public readonly Character _current;
    private Animator _animator;

    public AStateContext(Character current)
    {
        _current = current;
        _animator = current.GetComponentInChildren<Animator>();
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
    public Animator GetAnimator() { return _animator; }
}
