using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerStatesEnum
{
    Idle,
    Chase,
    Patrol
}

public class StateMachine
{
    IState _currentState = new BlankState();
    Dictionary<PlayerStatesEnum, IState> _allStates = new Dictionary<PlayerStatesEnum, IState>();

    public void OnStart()
    {
        _currentState.OnStart();
    }

    public void OnUpdate()
    {
        _currentState.OnUpdate();
    }

    public void AddState(PlayerStatesEnum id, IState state)
    {
        if (_allStates.ContainsKey(id)) return;

        _allStates.Add(id, state);
    }

    public void ChangeState(PlayerStatesEnum id)
    {
        if (!_allStates.ContainsKey(id)) return;
        _currentState.OnExit();
        _currentState = _allStates[id];
        _currentState.OnStart();
    }
}
