using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;

sealed internal class XboxInput : MonoBehaviour
{
    #region Values
    // Defines the instance of this class.
    private static XboxInput singleton;

    // Defines the quantity of connected controllers.
    internal static int connectedControllerCount;

    // Defines a section of dead space for each trigger to return false if its value is smaller.
    private const float TRIGGER_DEAD_SPACE = 0.1215f;

    public enum Button
    {
        Guide,
        Start,
        Back,
        A,
        B,
        X,
        Y,
        LB,
        RB,
        LeftStick,
        RightStick,
        DpadUp,
        DpadDown,
        DpadLeft,
        DpadRight,
        RightTrigger,
        LeftTrigger
    }
    public enum Direction
    {
        LeftStick,
        RightStick,
        Dpad
    }
    public enum Axis
    {
        LeftStickX,
        LeftStickY,
        RightStickX,
        RightStickY,
        LeftTrigger,
        RightTrigger
    }

    private class GamePadStates
    {
        public GamePadState PreviousState = new GamePadState();
        public GamePadState CurrentState = new GamePadState();
    }
    private GamePadStates[] _gamePadStates;
    private List<Vibration> _vibrations = new List<Vibration>();
    private float[] _controllerVibrations;
    #endregion

    #region Unity Functions
    private void Awake()
    {
        Initialize();
    }

    private void Update()
    {
        UpdateVibrations();
        UpdateStates();
    }
    #endregion

    #region Functions
    private void Initialize()
    {
        // Create instance, or destroy self if duplicate
        if (singleton != null)
        {
            Destroy(this.gameObject);
            return;
        }
        singleton = this;
        DontDestroyOnLoad(this.gameObject);

        _gamePadStates = new GamePadStates[] { new GamePadStates(), new GamePadStates(), new GamePadStates(), new GamePadStates() };
        _controllerVibrations = new float[] { 0, 0, 0, 0 };
    }

    private void UpdateStates()
    {
        connectedControllerCount = 0;
        for (int i = 0; i < 4; i++)
        {
            _gamePadStates[i].PreviousState = _gamePadStates[i].CurrentState;
            _gamePadStates[i].CurrentState = GamePad.GetState((PlayerIndex)i);

            if (_gamePadStates[i].CurrentState.IsConnected)
                connectedControllerCount += 1;

            float vibrationIntensity = Mathf.Min(_controllerVibrations[i], 1);
            GamePad.SetVibration((PlayerIndex)i, vibrationIntensity, vibrationIntensity);
        }
    }

    public static bool IsConnected(int gamePad)
    {
        return singleton._gamePadStates[gamePad].CurrentState.IsConnected;
    }

    public static bool GetButton(int gamePad, Button button)
    {
        return GetButtonState(singleton._gamePadStates[gamePad].CurrentState, gamePad, button);
    }
    public static bool GetButtonUp(int gamePad, Button button)
    {
        bool currentState = GetButtonState(singleton._gamePadStates[gamePad].CurrentState, gamePad, button);
        bool previousState = GetButtonState(singleton._gamePadStates[gamePad].PreviousState, gamePad, button);
        return previousState && !currentState;
    }
    public static bool GetButtonDown(int gamePad, Button button)
    {
        bool currentState = GetButtonState(singleton._gamePadStates[gamePad].CurrentState, gamePad, button);
        bool previousState = GetButtonState(singleton._gamePadStates[gamePad].PreviousState, gamePad, button);
        return !previousState && currentState;
    }
    public static bool GetButtonState(GamePadState gamePadState, int gamePad, Button button)
    {
        ButtonState currentState = ButtonState.Released;
        if (button == Button.Guide)
            currentState = gamePadState.Buttons.Guide;
        else if (button == Button.Start)
            currentState = gamePadState.Buttons.Start;
        else if (button == Button.Back)
            currentState = gamePadState.Buttons.Back;
        else if (button == Button.A)
            currentState = gamePadState.Buttons.A;
        else if (button == Button.B)
            currentState = gamePadState.Buttons.B;
        else if (button == Button.X)
            currentState = gamePadState.Buttons.X;
        else if (button == Button.Y)
            currentState = gamePadState.Buttons.Y;
        else if (button == Button.LB)
            currentState = gamePadState.Buttons.LeftShoulder;
        else if (button == Button.RB)
            currentState = gamePadState.Buttons.RightShoulder;
        else if (button == Button.LeftStick)
            currentState = gamePadState.Buttons.LeftStick;
        else if (button == Button.RightStick)
            currentState = gamePadState.Buttons.RightStick;
        else if (button == Button.DpadUp)
            currentState = gamePadState.DPad.Up;
        else if (button == Button.DpadDown)
            currentState = gamePadState.DPad.Down;
        else if (button == Button.DpadLeft)
            currentState = gamePadState.DPad.Left;
        else if (button == Button.DpadRight)
            currentState = gamePadState.DPad.Right;
        else if (button == Button.LeftTrigger)
            currentState = gamePadState.Triggers.Left > TRIGGER_DEAD_SPACE ? ButtonState.Pressed : ButtonState.Released;
        else if (button == Button.RightTrigger)
            currentState = gamePadState.Triggers.Right > TRIGGER_DEAD_SPACE ? ButtonState.Pressed : ButtonState.Released;

        return currentState == ButtonState.Pressed;
    }

    public static float GetAxis(int gamePad, Axis axis)
    {
        if (axis == Axis.LeftStickX)
            return singleton._gamePadStates[gamePad].CurrentState.ThumbSticks.Left.X;
        else if (axis == Axis.LeftStickY)
            return singleton._gamePadStates[gamePad].CurrentState.ThumbSticks.Left.Y;
        else if (axis == Axis.RightStickX)
            return singleton._gamePadStates[gamePad].CurrentState.ThumbSticks.Right.X;
        else if (axis == Axis.RightStickY)
            return singleton._gamePadStates[gamePad].CurrentState.ThumbSticks.Right.Y;
        else if (axis == Axis.LeftTrigger)
            return singleton._gamePadStates[gamePad].CurrentState.Triggers.Left;
        else if (axis == Axis.RightTrigger)
            return singleton._gamePadStates[gamePad].CurrentState.Triggers.Right;
        return 0;
    }
    public static Vector2 GetDirection(int gamePad, Direction direction)
    {
        if (direction == Direction.LeftStick)
            return new Vector2(singleton._gamePadStates[gamePad].CurrentState.ThumbSticks.Left.X, singleton._gamePadStates[gamePad].CurrentState.ThumbSticks.Left.Y);
        else if (direction == Direction.RightStick)
            return new Vector2(singleton._gamePadStates[gamePad].CurrentState.ThumbSticks.Right.X, singleton._gamePadStates[gamePad].CurrentState.ThumbSticks.Right.Y);
        else if (direction == Direction.Dpad)
        {
            float x = 0;
            if (singleton._gamePadStates[gamePad].CurrentState.DPad.Right == ButtonState.Pressed)
                x = 1;
            if (singleton._gamePadStates[gamePad].CurrentState.DPad.Left == ButtonState.Pressed)
                x = -1;

            float y = 0;
            if (singleton._gamePadStates[gamePad].CurrentState.DPad.Down == ButtonState.Pressed)
                y = 1;
            if (singleton._gamePadStates[gamePad].CurrentState.DPad.Up == ButtonState.Pressed)
                y = -1;
            return new Vector2(x, y);
        }


        return Vector2.zero;
    }

    private void UpdateVibrations()
    {
        _controllerVibrations = new float[] { 0, 0, 0, 0 };
        for (int i = 0; i < _vibrations.Count; i++)
        {
            Vibration vibration = _vibrations[i];
            float duration = vibration.killTime - vibration.startTime;
            float timeRemaining = vibration.killTime - Time.time;

            _controllerVibrations[vibration.controllerId] += vibration.intensity * (timeRemaining / duration);

            if (Time.time >= vibration.killTime)
            {
                i--;
                _vibrations.Remove(vibration);
            }
        }
    }

    public static void AddVibration(Vibration vibration)
    {
        singleton._vibrations.Add(vibration);
    }

    public static void ClearAllVibrations()
    {
        singleton._vibrations.Clear();
    }
    public static void ClearControllerVibrations(int controllerId)
    {
        singleton._vibrations.RemoveAll(v => v.controllerId == controllerId);
    }
    #endregion
}