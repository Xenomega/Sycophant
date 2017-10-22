using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

sealed internal class Inputter : MonoBehaviour
{
    #region Values
    private Controller _controller;

    [SerializeField] internal bool allowInput;

    private int _inputId;
    private bool _invertVerticalLook;

    private bool _touchedLastFrame;

    public enum GameButton
    {
        Jump,
        Rotate
    }
    public enum GameAxis
    {
        MovementX
    }

    internal class Cascade
    {
        [SerializeField] internal string pcName;
        [SerializeField] internal string playstationName;
    }

    private int _activeButtonSet;
    [Serializable]
    internal class ButtonSet
    {
        [SerializeField] string name;
        [Serializable]
        sealed internal class ButtonCascade : Cascade
        {
            [SerializeField] internal UIInput.Button uIButton;
            [SerializeField] internal XboxInput.Button xboxButton;
            internal enum DownType
            {
                Button,
                ButtonDown,
                ButtonUp
            }
            [SerializeField] internal DownType downType;
            [SerializeField] internal GameButton gameButton;
            [Header("NOTE: Hold duration requires a down type of button.")]
            [SerializeField]
            internal float holdDuration;
            internal float heldDuration;
            internal bool upSinceLastHold;
        }

        [SerializeField] internal List<ButtonCascade> cascades;

    }
    [SerializeField] private List<ButtonSet> _buttonSets;

    private int _activeAxisSet;
    [Serializable]
    internal class AxisSet
    {
        [SerializeField] string name;
        [Serializable]
        internal class AxisCascade : Cascade
        {
            [SerializeField] internal UIInput.Axis uIAxis;
            [SerializeField] internal XboxInput.Axis xboxAxis;
            [SerializeField] internal GameAxis gameAxis;
        }
        [SerializeField] internal List<AxisCascade> cascades;

    }
    [SerializeField] private List<AxisSet> _axisSets;
    #endregion

    #region Unity Functions
    private void Update()
    {
        _controller.ClearValues();
        UpdateButtons();
        UpdateAxes();
        UpdateTouch();
    }
    #endregion

    #region Functions
    internal void SetAspects(Player player)
    {
        _controller = player.Controller;
    }

    private void UpdateAxes()
    {
        if (_axisSets.Count == 0)
            return;

        foreach (AxisSet.AxisCascade axisCascade in _axisSets[_activeAxisSet].cascades)
        {
            if (axisCascade.gameAxis == GameAxis.MovementX)
                _controller.Move(GetAxis(axisCascade));
        }
    }
    private void UpdateButtons()
    {
        if (_buttonSets.Count == 0)
            return;
        foreach (ButtonSet.ButtonCascade buttonCascade in _buttonSets[_activeButtonSet].cascades)
        {
            if (buttonCascade.gameButton == GameButton.Jump)
            {
                if (GetButtonState(buttonCascade))
                    _controller.Jump();
            }
            if (buttonCascade.gameButton == GameButton.Rotate)
            {
                if (GetButtonState(buttonCascade))
                    _controller.Rotate();
            }
        }
    }
    private void UpdateTouch()
    {
        if (GetTouching())
        {
            _controller.Touch(GetTouchPosition());
            if (!_touchedLastFrame)
                _controller.TouchDown();

            _touchedLastFrame = true;
        }
        else if (_touchedLastFrame)
        {
            _controller.TouchUp();

            _touchedLastFrame = false;
        } 
    }

    private float GetAxis(AxisSet.AxisCascade axisCascade)
    {
        float value = 0;
        try
        {
            value += UnityEngine.Input.GetAxis(axisCascade.pcName);
        }
        catch (System.Exception)
        {
            try
            {
                value += UnityEngine.Input.GetButton(axisCascade.pcName) ? 1 : 0;
            }
            catch (System.Exception)
            {

            }
        }

        value += XboxInput.GetAxis(_inputId, axisCascade.xboxAxis);
        value += UIInput.GetAxis(axisCascade.uIAxis);

        return Mathf.Clamp(value, -1, 1);
    }

    private bool GetButton(ButtonSet.ButtonCascade buttonCascade)
    {
        try
        {
            if (UnityEngine.Input.GetButton(buttonCascade.pcName))
                return true;
        }
        catch (System.Exception) { }

        if (XboxInput.GetButton(_inputId, buttonCascade.xboxButton))
            return true;
        if (UIInput.GetButton(buttonCascade.uIButton))
            return true;

        return false;
    }
    private bool GetButtonUp(ButtonSet.ButtonCascade buttonCascade)
    {
        try
        {
            if (UnityEngine.Input.GetButtonUp(buttonCascade.pcName))
                return true;
        }
        catch (System.Exception) { }

        if (XboxInput.GetButtonUp(_inputId, buttonCascade.xboxButton))
            return true;
        if (UIInput.GetButtonUp(buttonCascade.uIButton))
            return true;
        return false;
    }
    private bool GetButtonDown(ButtonSet.ButtonCascade buttonCascade)
    {
        try
        {
            if (UnityEngine.Input.GetButtonDown(buttonCascade.pcName))
                return true;
        }
        catch (System.Exception) { }

        if (XboxInput.GetButtonDown(_inputId, buttonCascade.xboxButton))
            return true;
        if (UIInput.GetButtonDown(buttonCascade.uIButton))
            return true;
        return false;
    }
    private bool GetButtonState(ButtonSet.ButtonCascade buttonCascade)
    {
        if (buttonCascade.downType == ButtonSet.ButtonCascade.DownType.Button)
        {
            bool down = GetButton(buttonCascade);
            if (down)
            {
                if (buttonCascade.upSinceLastHold)
                    buttonCascade.heldDuration += Time.deltaTime;

                if (buttonCascade.heldDuration >= buttonCascade.holdDuration)
                {
                    buttonCascade.heldDuration = 0;
                    buttonCascade.upSinceLastHold = false;
                }
                else
                    down = false;
            }
            else
            {
                buttonCascade.upSinceLastHold = true;
                buttonCascade.heldDuration = 0;
            }
            return down;
        }
        if (buttonCascade.downType == ButtonSet.ButtonCascade.DownType.ButtonDown)
        {
            return GetButtonDown(buttonCascade);
        }
        if (buttonCascade.downType == ButtonSet.ButtonCascade.DownType.ButtonUp)
        {
            return GetButtonUp(buttonCascade);
        }
        return false;
    }

    private bool GetTouching()
    {
#if UNITY_EDITOR
        return Input.GetMouseButton(0);
#else
        return Input.touchCount > 0;
#endif
    }
    private Vector2 GetTouchPosition()
    {
        if (Input.touchCount > 0)
            return Input.touches[0].position;
        else
            return Input.mousePosition;
    }
    #endregion
}
