using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

sealed internal class UIInput : MonoBehaviour
{
    internal static UIInput singleton;

    #region Values
    internal enum Button
    {
        A,
        B
    }
    internal enum Axis
    {
        Movement
    }

    sealed private class State
    {
        internal bool a;
        internal bool b;
        internal float movement;
    }

    private State _previousState = new State();
    private State _currentState = new State();
    #endregion

    #region Unity Functions
    private void Awake()
    {
        singleton = this;
    }

    private void LateUpdate()
    {
        _previousState = _currentState;
        _currentState = new State();
    }
    #endregion

    #region Functions
    public void InputPress(int button)
    {
        InputPress((Button)button);
    }
    public void InputPress(Button button)
    {
        switch (button)
        {
            case Button.A:
                _currentState.a = true;
                break;
            case Button.B:
                _currentState.b = true;
                break;
        }
    }

    public void InputMoveForward(int axis)
    {
        InputMove(axis, 1);
    }
    public void InputMoveBackward(int axis)
    {
        InputMove(axis, -1);
    }
    public void InputMove(int axis, float direction)
    {
        InputMove((Axis)axis, direction);
    }
    public void InputMove(Axis axis, float direction)
    {
        switch (axis)
        {
            case Axis.Movement:
                _currentState.movement = direction;
                break;
        }
    }


    internal static bool GetButton(Button button)
    {
        return GetStateButton(singleton._currentState, button);
    }
    internal static bool GetButtonUp(Button button)
    {
        bool previous = GetStateButton(singleton._previousState, button);
        bool current = GetStateButton(singleton._currentState, button);

        return previous && !current;
    }
    internal static bool GetButtonDown(Button button)
    {
        bool previous = GetStateButton(singleton._previousState, button);
        bool current = GetStateButton(singleton._currentState, button);

        return !previous && current;
    }
    private static bool GetStateButton(State state, Button button)
    {
        switch (button)
        {
            case Button.A:
                return state.a;
            case Button.B:
                return state.b;
        }
        return false;
    }

    internal static float GetAxis(Axis axis)
    {
        return GetStateAxis(singleton._currentState, axis);
    }
    private static float GetStateAxis(State state, Axis axis)
    {
        switch (axis)
        {
            case Axis.Movement:
                return state.movement;
        }
        return 0;
    } 
    #endregion
}
