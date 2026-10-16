using System;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine<TState> where TState :State
{
    private readonly Dictionary<Type, TState> states = new();
    public TState Current { get; private set; }   
    public void Register(TState state)
    {
        states.Add(state.GetType(), state);
    }
    public T Get<T>() where T : State
    {
        return states[typeof(T)] as T;
    }
    public bool Has<T>() where T : State
    {
        return states.ContainsKey(typeof(T));
    }
    public void Start<T>() where T : TState
    {
        Current = Get<T>();
        Current.EnableChange();
        Current.Enter();
    }
    public void ChangeState<T>() where T : TState
    {
        if (!Has<T>())
        {
            Debug.LogError("Error");
            return;
        }
        Current.Exit();
        Current = Get<T>();
        Current.EnableChange();
        Current.Enter();
    }
}
