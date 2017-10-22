using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

sealed internal class Controller
{
    #region Values
    internal bool AllowMovement { get; set; }

    private Vector2 _lastFrameTouchPosition;
    internal bool TouchHasMoved { get { return _touchPosition != _lastFrameTouchPosition; } }

    private Vector2 _touchPosition;
    internal Vector2 TouchPosition { get { return _touchPosition; } }
    private Vector2 _touchDownPosition;
    internal Vector2 TouchDownPosition { get { return _touchDownPosition; } }
    private Vector2 _touchUpPosition;
    internal Vector2 TouchUpPosition { get { return _touchUpPosition; } }
    internal Vector2 TouchDirectionFromOrigin
    {
        get
        {
            return (_touchPosition - _touchDownPosition).normalized;
        }
    }
    
    internal event UnityAction<Vector2> OnTouch;
    internal void Touch(Vector3 position)
    {
        _touchPosition = position;
        if (OnTouch == null)
            return;
        OnTouch.Invoke(TouchPosition);
    }
    
    internal event UnityAction<Vector2> OnTouchDown;
    internal void TouchDown()
    {
        _touchDownPosition = _touchPosition;
        if (OnTouchDown == null)
            return;
        OnTouchDown.Invoke(TouchPosition);
    }
    
    internal event UnityAction<Vector2> OnTouchUp;
    internal void TouchUp()
    {
        _touchUpPosition = _touchPosition;
        if (OnTouchUp == null)
            return;
        OnTouchUp.Invoke(TouchPosition);
    }
    
    internal event UnityAction<float> OnRotate;
    internal void Rotate()
    {
        if (OnRotate == null)
            return;
        if (!AllowMovement)
            return;

        OnRotate.Invoke(_recentMoveDirection);
    }

    internal event UnityAction OnJump;
    internal void Jump()
    {
        if (OnJump == null)
            return;
        if (!AllowMovement)
            return;

            OnJump.Invoke();
    }

    private float _recentMoveDirection = 1;
    internal event UnityAction<float> OnMove;
    internal void Move(float direction)
    {
        if (OnMove == null)
            return;
        if (!AllowMovement)
            return;
        
        if (direction != 0)
            _recentMoveDirection = direction > 0 ? 1 : -1;

        OnMove.Invoke(direction);
    }
    #endregion

    #region Functions
    internal void ClearValues()
    {
        _lastFrameTouchPosition = _touchPosition;
        _touchPosition = Vector2.zero;
    } 
    #endregion
}
