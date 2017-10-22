using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using System.Reflection;
using System.Globalization;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Development
{
    [DisallowMultipleComponent]
    sealed public class Debug : MonoBehaviour
    {
        internal static Debug singleton;

        #region Values
        [SerializeField] private bool _buildTextOnly;
        /// <summary>
        /// Determines if output is being shown.
        /// </summary>
        [Space(10)]
        [SerializeField]
        private bool _showInputOutput = true;

        [SerializeField]
        private bool _showLabels = true;

        private static bool _logOnPrint = true;
        private static bool _debugLogPrint = false;

        private const int MAX_LOGS_DISPLAYED = 8;

        [SerializeField] private string _buildText = "PRE-ALPHA BUILD";
        [SerializeField] private float _buildInfoVerticalOffset = 60f;
        [HideInInspector] public string buildDate;
        [SerializeField] private string _buildDetails;

        private static List<Message> _messages = new List<Message>();
        private static List<Value> _values = new List<Value>();
        private static List<Button> _buttons = new List<Button>();


        [Space(10)]
        [SerializeField]
        private string[] _notes;
        [SerializeField] private string[] _fixes;

        [Space(15)]
        [SerializeField]
        private OutputSettings _outputSettings;


        [Space(15)]
        [SerializeField]
        private CompositionSettings _compositionSettings;

        [Space(15)]
        private Texture2D _whiteTexture;
        private Texture2D _redTexture;
        private Texture2D _yellowTexture;
        private Texture2D _cyanTexture;

        /// <summary>
        /// The action safe screen positions.
        /// </summary>
        private float _actionSafeTop;
        private float _actionSafeBottom;
        private float _actionSafeLeft;
        private float _actionSafeRight;

        #region FrameRate
        private float _accum = 0;
        private int _frames = 0;
        private float _timeleft;
        internal int frameRate;
        #endregion
        private class TimedGizmo
        {
            internal enum Shape
            {
                sphere,
                line
            }
            internal float lifespan;
            internal float creationTime;
            internal Shape shape;
            internal Color color;
            internal float radius;
            internal Vector3 start;
            internal Vector3 end;

            internal TimedGizmo(float lifeSpan, Color color, float radius, Vector3 position)
            {
                shape = Shape.sphere;
                lifespan = lifeSpan;
                creationTime = Time.time;
                this.color = color;
                this.radius = radius;
                start = position;
            }
            internal TimedGizmo(float lifeSpan, Color color, Vector3 start, Vector3 end)
            {
                shape = Shape.line;
                lifespan = lifeSpan;
                creationTime = Time.time;
                this.color = color;
                this.start = start;
                this.end = end;
            }
        }
        private static List<TimedGizmo> _timedGizmos = new List<TimedGizmo>();

        sealed private class HandleLabelGroup
        {
            internal GameObject gameObject;
            sealed internal class HandleLabel
            {
                internal MonoBehaviour source;
                sealed internal class Value
                {
                    internal string identifyingName;
                    internal string text;
                    internal Value(string identifyingName, string text)
                    {
                        this.identifyingName = identifyingName;
                        this.text = text;
                    }
                }
                internal List<Value> values = new List<Value>();
                internal HandleLabel(MonoBehaviour source)
                {
                    this.source = source;
                    values = new List<Value>();
                }
            }
            internal List<HandleLabel> handleLabels = new List<HandleLabel>();

            public HandleLabelGroup(GameObject gameObject)
            {
                this.gameObject = gameObject;
            }
        }
        private static List<HandleLabelGroup> _handleLabelGroups = new List<HandleLabelGroup>();

        private ulong _debugCounter;
        private ulong _debugCounter2 = 0x8534593495347656;
        #endregion

        #region Classes
        [Serializable]
        public enum MessagePriority
        {
            Information,
            Low,
            Medium,
            High
        }
        [Serializable]
        public class OutputSettings
        {
            /// <summary>
            /// Defines the font of the gui.
            /// </summary>
            [SerializeField] internal Font genericFont;
            /// <summary>
            /// Defines the default color for output gui elements text
            /// </summary>
            [SerializeField] internal Color genericTextColor = Color.white;
            /// <summary>
            /// Overrides the alpha of all Output Gui elements
            /// </summary>
            [Range(0, 1)]
            [SerializeField]
            internal float defaultAlpha = 0.6f;

            [SerializeField] internal Font buildFont;
            [SerializeField] internal Color buildTextColor = new Color(1, 1, 1, 0.25f);
            [Space(10)]
            [SerializeField]
            internal Color handleColor = Color.white;
            [SerializeField] internal int handleSize = 10;
            [SerializeField]  internal float displayHandleDistance = 30f;

            /// <summary>
            /// Defines the background color of input buttons
            /// </summary>
            [Space(10)]
            [SerializeField] internal Color buttonBackgroundColor = Color.black;
            /// <summary>
            /// Defines the text color of notes
            /// </summary>
            [Space(10)]
            [SerializeField]
            internal Color noteTextColor = Color.yellow;
            [Space(10)]
            [SerializeField]
            internal Color bugTextColor = Color.red;
            /// <summary>
            /// Define the color states of the fps counter
            /// </summary>
            [Space(10)]
            [SerializeField]
            internal Color lowFramerateTextColor = Color.red;
            [SerializeField] internal Color mediumFramerateTextColor = Color.yellow;
            [SerializeField] internal Color idealFramerateTextColor = Color.white;

            // Our log header colors.
            [Space(10)]
            [SerializeField]
            internal Color outHeaderColor = Color.cyan;
            [SerializeField] internal Color outTypeInfoColor = Color.white;
            [SerializeField] internal Color outTypeLowColor = Color.yellow;
            [SerializeField] internal Color outTypeMediumColor = Color.green;
            [SerializeField] internal Color outTypeHighColor = Color.red;


            /// <summary>
            /// Adds to all output input element font sizes
            /// </summary>
            [Space(10)]
            [SerializeField]
            internal int fontSizeAdditive = 0;
            [Space(5)]
            [SerializeField]
            internal int LogFontSize = 9;
            [SerializeField] internal int valueTextSize = 9;
            [SerializeField] internal int noteTextSize = 9;
            [SerializeField] internal int buttonTextSize = 9;
            [SerializeField] internal int framerateTextSize = 11;
            [SerializeField] internal int buildTextSize = 12;
            [SerializeField] internal int dateLabelSize = 200;
            [SerializeField] internal int dateTextSize = 9;
            [SerializeField] internal int debugCounterTextSize = 11;

            internal GUIStyle noteStyle = new GUIStyle();
            internal GUIStyle bugsStyle = new GUIStyle();
            internal GUIStyle valueStyle = new GUIStyle();
            internal GUIStyle logStyle = new GUIStyle();
            internal GUIStyle ButtonGuiStyle = new GUIStyle();
            internal GUIStyle framerateStyle = new GUIStyle();
            internal GUIStyle buildStyle = new GUIStyle();
            internal GUIStyle dateStyle = new GUIStyle();
            internal GUIStyle debugCounterStyle = new GUIStyle();
            internal GUIStyle handleStyle = new GUIStyle();
            internal Texture2D buttonTexture;

        }
        [Serializable]
        private class CompositionSettings
        {
            /// <summary>
            /// Determines if the rule of third markers will be shown.
            /// </summary>
            [SerializeField] internal bool showRuleOfThirds;
            /// <summary>
            /// Determines if the cross point markers will be shown.
            /// </summary>
            [SerializeField] internal bool showCrossPoint;
            /// <summary>
            /// Determines if the safe frame markers will be shown.
            /// </summary>
            [SerializeField] internal bool showSafeFrames;
            /// <summary>
            /// Defines the color of the rule of thirds lines.
            /// </summary>
            [Space(10)]
            [SerializeField]
            internal Color ruleOfThirdsColor;
            /// <summary>
            /// Defines the color of the cross-point lines.
            /// </summary>
            [SerializeField] internal Color crossPointColor;
            /// <summary>
            /// Defines the color of the action safe lines.
            /// </summary>
            [SerializeField] internal Color actionSafeColor;
            /// <summary>
            /// Defines the color of the title safe lines.
            /// </summary>
            [SerializeField] internal Color titleSafeColor;

            internal Texture2D actionSafeTexture;
            internal Texture2D titleSafeTexture;
            internal Texture2D ruleOfThirdsTexture;
            internal Texture2D crossPointTexture;
        }
        private class Message
        {
            internal float duration;
            internal string message;
            internal Message(float duration, string message)
            {
                this.duration = duration;
                this.message = message;
            }
        }
        private class Value
        {
            public string title;
            public string message;
            public float padding;
            public Value(string title, string message, float padding)
            {
                this.title = title;
                this.message = message;
                this.padding = padding;
            }
        }
        private class Button
        {
            public string text;
            public Action action;
            public object paramater;
            public Button(string text, Action action, object paramater)
            {
                this.text = text;
                this.action = action;
                this.paramater = paramater;
            }
        }
        #endregion


        #region Unity Functions
        private void Awake()
        {
            if (singleton != null && singleton != this)
            {
                Destroy(this.gameObject);
                return;
            }

            singleton = this;
            DontDestroyOnLoad(this.gameObject);

            ApplySettings();
        }

        private void Update()
        {
            // Define action safe bounds
            _actionSafeTop = (Screen.height - (Screen.height * 0.9f)) / 2;
            _actionSafeBottom = Screen.height * 0.95f;
            _actionSafeLeft = Screen.width - (Screen.width * 0.95f);
            _actionSafeRight = (Screen.width * 0.95f);

            UpdateFrameRate();


            if (_buildTextOnly)
                return;

            UpdateMessageDurations();
        }

        private void OnGUI()
        {
            DisplayLabels();
            DisplayBuildText();
            DisplayFrameRate();
            DisplayDebugCounters();

            if (_buildTextOnly)
                return;


            DisplayCompositionHelpers();

            if (_showInputOutput)
            {
                if (Application.platform != RuntimePlatform.XboxOne)
                    DisplayButtons();

                DisplayValues();
                DisplayLogs();
                DisplayNotesAndFixes();
            }
        }

        private void OnDrawGizmos()
        {
            DisplayGizmos();
        }
        private void OnDisable()
        {
            _timedGizmos.Clear();
        }
        private void OnDistroy()
        {
            _timedGizmos.Clear();
        }
        #endregion

        #region Functions
        private void ApplySettings()
        {

            if (Application.platform == RuntimePlatform.XboxOne)
                _outputSettings.fontSizeAdditive += 3;

            #region Color Textures
            _outputSettings.buttonTexture = new Texture2D(1, 1);
            _whiteTexture = new Texture2D(1, 1);
            _redTexture = new Texture2D(1, 1);
            _yellowTexture = new Texture2D(1, 1);
            _cyanTexture = new Texture2D(1, 1);

            _compositionSettings.actionSafeTexture = new Texture2D(1, 1);
            _compositionSettings.titleSafeTexture = new Texture2D(1, 1);
            _compositionSettings.ruleOfThirdsTexture = new Texture2D(1, 1);
            _compositionSettings.crossPointTexture = new Texture2D(1, 1);


            _outputSettings.buttonTexture.SetPixel(0, 0, _outputSettings.buttonBackgroundColor);
            _whiteTexture.SetPixel(0, 0, new Color(1, 1, 1, 0.25f));
            _redTexture.SetPixel(0, 0, Color.red);
            _yellowTexture.SetPixel(0, 0, Color.yellow);
            _cyanTexture.SetPixel(0, 0, Color.cyan);


            _compositionSettings.actionSafeTexture.SetPixel(0, 0, _compositionSettings.actionSafeColor);
            _compositionSettings.titleSafeTexture.SetPixel(0, 0, _compositionSettings.titleSafeColor);
            _compositionSettings.ruleOfThirdsTexture.SetPixel(0, 0, _compositionSettings.ruleOfThirdsColor);
            _compositionSettings.crossPointTexture.SetPixel(0, 0, _compositionSettings.crossPointColor);

            _outputSettings.buttonTexture.Apply();
            _whiteTexture.Apply();
            _redTexture.Apply();
            _yellowTexture.Apply();
            _cyanTexture.Apply();

            _compositionSettings.actionSafeTexture.Apply();
            _compositionSettings.titleSafeTexture.Apply();
            _compositionSettings.ruleOfThirdsTexture.Apply();
            _compositionSettings.crossPointTexture.Apply();
            #endregion

            if (_outputSettings.defaultAlpha > 0)
            {
                _outputSettings.noteTextColor.a = _outputSettings.defaultAlpha;
                _outputSettings.genericTextColor.a = _outputSettings.defaultAlpha;
                _outputSettings.idealFramerateTextColor.a = _outputSettings.defaultAlpha;
                _outputSettings.mediumFramerateTextColor.a = _outputSettings.defaultAlpha;
                _outputSettings.lowFramerateTextColor.a = _outputSettings.defaultAlpha;
                _outputSettings.buttonBackgroundColor.a = _outputSettings.defaultAlpha;
            }

            #region Gui Styles

            #region Notes
            _outputSettings.noteStyle.alignment = TextAnchor.MiddleRight;

            _outputSettings.noteStyle.font = _outputSettings.genericFont;
            _outputSettings.noteStyle.fontSize = (_outputSettings.noteTextSize + _outputSettings.fontSizeAdditive);
            _outputSettings.noteStyle.alignment = TextAnchor.UpperLeft;
            _outputSettings.noteStyle.normal.textColor = _outputSettings.noteTextColor;
            #endregion
            #region Bugs
            _outputSettings.bugsStyle.alignment = TextAnchor.MiddleRight;

            _outputSettings.bugsStyle.font = _outputSettings.genericFont;
            _outputSettings.bugsStyle.fontSize = (_outputSettings.noteTextSize + _outputSettings.fontSizeAdditive);
            _outputSettings.bugsStyle.alignment = TextAnchor.UpperLeft;
            _outputSettings.bugsStyle.normal.textColor = _outputSettings.bugTextColor;
            #endregion

            #region Values
            _outputSettings.valueStyle.normal.textColor = _outputSettings.genericTextColor;
            _outputSettings.valueStyle.alignment = TextAnchor.UpperRight;
            _outputSettings.valueStyle.font = _outputSettings.genericFont;
            _outputSettings.valueStyle.fontSize = (_outputSettings.valueTextSize + _outputSettings.fontSizeAdditive);
            #endregion
            #region Logs
            _outputSettings.logStyle.normal.textColor = _outputSettings.genericTextColor;
            _outputSettings.logStyle.alignment = TextAnchor.LowerLeft;
            _outputSettings.logStyle.font = _outputSettings.genericFont;
            _outputSettings.logStyle.fontSize = (_outputSettings.LogFontSize + _outputSettings.fontSizeAdditive);
            _outputSettings.logStyle.richText = true;
            #endregion
            #region FPS Counter
            _outputSettings.framerateStyle.normal.textColor = _outputSettings.genericTextColor;
            _outputSettings.framerateStyle.alignment = TextAnchor.LowerLeft;
            _outputSettings.framerateStyle.font = _outputSettings.genericFont;
            _outputSettings.framerateStyle.fontSize = (_outputSettings.framerateTextSize + _outputSettings.fontSizeAdditive);
            #endregion
            #region Build Text
            _outputSettings.buildStyle.alignment = TextAnchor.LowerCenter;
            _outputSettings.buildStyle.normal.textColor = _outputSettings.buildTextColor;
            _outputSettings.buildStyle.font = _outputSettings.buildFont;
            _outputSettings.buildStyle.fontSize = (_outputSettings.buildTextSize + _outputSettings.fontSizeAdditive);
            #endregion
            #region Date Text
            _outputSettings.dateStyle.alignment = TextAnchor.LowerCenter;
            _outputSettings.dateStyle.normal.textColor = _outputSettings.buildTextColor;
            _outputSettings.dateStyle.font = _outputSettings.buildFont;
            _outputSettings.dateStyle.fontSize = (_outputSettings.dateTextSize + _outputSettings.fontSizeAdditive);
            #endregion
            #region Button
            _outputSettings.ButtonGuiStyle.fontSize = (_outputSettings.buttonTextSize + _outputSettings.fontSizeAdditive);
            _outputSettings.ButtonGuiStyle.normal.textColor = Color.white;
            _outputSettings.ButtonGuiStyle.font = _outputSettings.genericFont;
            _outputSettings.ButtonGuiStyle.alignment = TextAnchor.MiddleCenter;
            #endregion
            #region DebugCounter
            _outputSettings.debugCounterStyle.alignment = TextAnchor.LowerRight;
            _outputSettings.debugCounterStyle.normal.textColor = _outputSettings.buildTextColor;
            _outputSettings.debugCounterStyle.font = _outputSettings.buildFont;
            _outputSettings.debugCounterStyle.fontSize = (_outputSettings.debugCounterTextSize + _outputSettings.fontSizeAdditive);
            #endregion

            _outputSettings.handleStyle.normal.textColor = _outputSettings.handleColor;
            _outputSettings.handleStyle.fontSize = _outputSettings.handleSize;
            #endregion

        }
        
        internal static void ReloadScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        internal static void ClearConsole()
        {
            Type logEntries = System.Type.GetType("UnityEditorInternal.LogEntries,UnityEditor.dll");
            MethodInfo clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            clearMethod.Invoke(null, null);
        }

        internal static void ShowTimedLineGizmo(Vector3 start, Vector3 end, Color color, float lifeSpan = 0)
        {
            _timedGizmos.Add(new TimedGizmo(lifeSpan, color, start, end));
        }
        internal static void ShowTimedSphereGizmo(Vector3 position, float radius, Color color, float lifeSpan = 0)
        {
            _timedGizmos.Add(new TimedGizmo(lifeSpan, color, radius, position));
        }

        internal static void ShowLabel(MonoBehaviour source, string identifyingName, string text)
        {
            if (!singleton._showLabels)
                return;
            HandleLabelGroup handleLabelGroup = _handleLabelGroups.Find(s => s.gameObject == source.gameObject);

            if (handleLabelGroup == null)
            {
                handleLabelGroup = new HandleLabelGroup(source.gameObject);
                _handleLabelGroups.Add(handleLabelGroup);
            }

            HandleLabelGroup.HandleLabel handleLabel = handleLabelGroup.handleLabels.Find(s => s.source == source);
            if (handleLabel == null)
            {
                handleLabel = new HandleLabelGroup.HandleLabel(source);
                handleLabel.values.Add(new HandleLabelGroup.HandleLabel.Value(identifyingName, text));
                handleLabelGroup.handleLabels.Add(handleLabel);
            }

            HandleLabelGroup.HandleLabel.Value value = handleLabel.values.Find(v => v.identifyingName == identifyingName);
            if (value == null)
            {
                value = new HandleLabelGroup.HandleLabel.Value(identifyingName, text);
                handleLabel.values.Add(value);
            }

            value.text = text;
        }

        internal static void ShowButton(string text, Action action)
        {
            foreach (Button button in _buttons)
            {
                if (button.text == text)
                {
                    button.action = action;
                    return;
                }
            }
            Button newButton = new Button(text, action, null);
            _buttons.Add(newButton);
        }

        internal static void ShowValue(string title, object message)
        {
            if (singleton == null)
                return;

            foreach (Value valueDisplay in _values)
            {
                if (valueDisplay.title == title)
                {
                    valueDisplay.message = message.ToString();
                    return;
                }
            }
            Value newValueDisplay = new Value(title, message.ToString(), 0);
            _values.Add(newValueDisplay);
        }
        internal static void ShowValue(string title, object message, float topPadding)
        {
            foreach (Value valueDisplay in _values)
            {
                if (valueDisplay.title == title)
                {
                    valueDisplay.message = message.ToString();
                    return;
                }
            }
            Value newValueDisplay = new Value(title, message.ToString(), topPadding);
            _values.Add(newValueDisplay);
        }

        internal static void Log(object message)
        {
            Log(message, 5);
        }
        internal static void Log(object message, float duration)
        {
            Log(message, string.Empty, duration);
        }
        internal static void Log(object message, string objectName, float duration)
        {
            string compiledMessage = message.ToString();
            if (!string.IsNullOrEmpty(objectName))
                compiledMessage += " (" + objectName + ")";
            Message newMessage = new Message(duration, compiledMessage);
            _messages.Insert(0, newMessage);
        }

        internal static void Out(object source, string message, MessagePriority messagePriority)
        {
            // If we're not outputting, stop.
            if (!_debugLogPrint && !_logOnPrint)
                return;

            // Determine our name from type.
            string name = source.GetType().ToString();

            // Print our message
            Color colorPriority = singleton._outputSettings.outTypeInfoColor;
            switch (messagePriority)
            {
                case MessagePriority.Information:
                    colorPriority = singleton._outputSettings.outTypeInfoColor;
                    break;
                case MessagePriority.Low:
                    colorPriority = singleton._outputSettings.outTypeLowColor;
                    break;
                case MessagePriority.Medium:
                    colorPriority = singleton._outputSettings.outTypeMediumColor;
                    break;
                case MessagePriority.High:
                    colorPriority = singleton._outputSettings.outTypeHighColor;
                    break;
            }
            Out(string.Format("<color='#{0}'>[{1}]: </color><color='#{2}'>{3}</color>", singleton._outputSettings.outHeaderColor.GetHashCode(), name, colorPriority.ToArgbString(), message));
        }
        private static void Out(string message)
        {
            if (_debugLogPrint)
                UnityEngine.Debug.Log(message);
            if (_logOnPrint)
                Log(message);
        }
        #endregion

        #region Display
        private void DisplayGizmos()
        {
            for (int i = 0; i < _timedGizmos.Count; i++)
            {
                TimedGizmo timedGizmo = _timedGizmos[i];
                if (timedGizmo.lifespan + timedGizmo.creationTime < Time.time)
                {
                    _timedGizmos.Remove(timedGizmo);
                    i--;
                    continue;
                }

                if (timedGizmo.shape == TimedGizmo.Shape.sphere)
                {
                    Gizmos.color = timedGizmo.color;
                    Gizmos.DrawWireSphere(timedGizmo.start, timedGizmo.radius);
                }
                else
                    UnityEngine.Debug.DrawLine(timedGizmo.start, timedGizmo.end, timedGizmo.color);
            }
        }

        private void DisplayLabels()
        {
            if (!_showLabels)
                return;
            for (int i = 0; i < _handleLabelGroups.Count; i++)
            {
                HandleLabelGroup handleLabelGroup = _handleLabelGroups[i];
                if (handleLabelGroup.gameObject == null)
                {
                    _handleLabelGroups.Remove(handleLabelGroup);
                    i--;
                    continue;
                }
                if (handleLabelGroup.gameObject.activeSelf == false)
                    continue;

                Vector3 screenPoint = Camera.current.WorldToScreenPoint(handleLabelGroup.gameObject.transform.position);
                float sqrMagnitude = (handleLabelGroup.gameObject.transform.position - Camera.current.transform.position).sqrMagnitude;
                float radius = _outputSettings.displayHandleDistance * _outputSettings.displayHandleDistance;
                if (sqrMagnitude > radius)
                    continue;

                string text = "<b>" +handleLabelGroup.gameObject.name + "</b>";

                foreach (HandleLabelGroup.HandleLabel handleLabel in handleLabelGroup.handleLabels)
                {
                    if (handleLabel.source != null)
                    {
                        try
                        {
                            text += "\n \n<b>" + handleLabel.source.GetType().ToString() + "</b>";
                            foreach (HandleLabelGroup.HandleLabel.Value value in handleLabel.values)
                                text += "\n<size=9> -  " + value.identifyingName + " : " + value.text + "</size>";

                        }
                        catch (System.Exception) { }
                    }
                }
                DisplayLabel(handleLabelGroup.gameObject.transform.position, text);
            }
        }
        private void DisplayLabel(Vector3 worldPosition, string text)
        {
            Vector3 screenPoint = Camera.current.WorldToScreenPoint(worldPosition);
            screenPoint.y = Screen.height - screenPoint.y;
            if (screenPoint.z < 0)
                return;
            GUI.Label(new Rect(screenPoint, new Vector2(1000, 1000)), text, _outputSettings.handleStyle);
        }
        
        private void DisplayLogs()
        {
            if (_messages.Count == 0)
                return;

            // Get messages in range of max display count
            List<Message> existingMessages;
            if (_messages.Count > MAX_LOGS_DISPLAYED)
                existingMessages = _messages.GetRange(0, MAX_LOGS_DISPLAYED);
            else
                existingMessages = _messages;

            // Print logs to gui
            for (int i = 0; i < existingMessages.Count; i++)
            {
                string message = existingMessages[i].message;
                if (message == string.Empty)
                    message = "...";

                GUI.Label(new Rect(new Vector2(_actionSafeLeft, (_actionSafeBottom - 80) + (-15 * i)), new Vector2(800, 50)), message, _outputSettings.logStyle);
            }
        }
        private void DisplayValues()
        {
            float valuePositon = _actionSafeTop - 15;

            // Display screen logs
            for (int i = 0; i < _values.Count; i++)
            {
                valuePositon += (15 + _values[i].padding);
                GUI.Label(new Rect(new Vector2(_actionSafeRight - 800, valuePositon), new Vector2(800, 50)), _values[i].title + " " + _values[i].message, _outputSettings.valueStyle);
            }
        }
        private void DisplayNotesAndFixes()
        {
            float valuePositon = _actionSafeTop - 15;
            if (_fixes.Length > 0)
            {
                valuePositon += 15;
                GUI.Label(new Rect(new Vector2(_actionSafeLeft, valuePositon), new Vector2(800, 50)), "FIXES:", _outputSettings.bugsStyle);
                for (int i = 0; i < _fixes.Length; i++)
                {
                    if (_fixes[i] == string.Empty)
                        continue;
                    valuePositon += 15;
                    GUI.Label(new Rect(new Vector2(_actionSafeLeft, valuePositon), new Vector2(800, 50)), _fixes[i], _outputSettings.bugsStyle);
                }
            }
            if (_notes.Length > 0)
            {
                valuePositon += _fixes.Length > 0 ? 30 : 15;
                GUI.Label(new Rect(new Vector2(_actionSafeLeft, valuePositon), new Vector2(800, 50)), "NOTES:", _outputSettings.noteStyle);

                for (int i = 0; i < _notes.Length; i++)
                {
                    if (_notes[i] == string.Empty)
                        continue;
                    valuePositon += 15;
                    GUI.Label(new Rect(new Vector2(_actionSafeLeft, valuePositon), new Vector2(800, 50)), _notes[i], _outputSettings.noteStyle);
                }
            }
        }
        private void DisplayButtons()
        {
            if (_buttons.Count == 0)
                return;

            // Display screen buttons
            for (int i = 0; i < _buttons.Count; i++)
            {
                // Black Background
                GUI.DrawTexture(new Rect(_actionSafeLeft, _actionSafeTop + (30 * i), 120, 20), _outputSettings.buttonTexture);
                // Button
                if (GUI.Button(new Rect(_actionSafeLeft, _actionSafeTop + (30 * i), 120, 20), _buttons[i].text, _outputSettings.ButtonGuiStyle))
                {
                    _buttons[i].action.Invoke();
                }
            }
        }
        private void DisplayFrameRate()
        {
            // Determine the color of the counter
            _outputSettings.framerateStyle.normal.textColor = (frameRate >= 50) ? _outputSettings.idealFramerateTextColor : ((frameRate > 30) ? _outputSettings.mediumFramerateTextColor : _outputSettings.lowFramerateTextColor);
            GUI.Label(new Rect(new Vector2(_actionSafeLeft, _actionSafeBottom - 50), new Vector2(800, 50)), "[ " + frameRate + " ]", _outputSettings.framerateStyle);
        }
        private void DisplayBuildText()
        {
            _buildText = _buildText.ToUpper();

            _buildDetails = _buildDetails.Replace(" ", "_");
            _buildDetails = _buildDetails.ToUpper();

            //        string platform = "UNDEFINED";
            //#if UNITY_EDITOR
            //        platform = "EDITOR";
            //#elif UNITY_STANDALONE_WIN
            //        platform = "WIN";
            //#elif UNITY_STANDALONE_OSX
            //        platform = "OSX";
            //#elif UNITY_STANDALONE_LINUX
            //        platform = "UNIX";
            //#elif UNITY_XBOXONE
            //        platform = "XBOX";
            //#endif


            //Display alpha build text at the bottom center of the screen
            GUI.Label(new Rect(new Vector2((Screen.width / 2) - 200, _actionSafeBottom - _buildInfoVerticalOffset - 12), new Vector2(400, 50)), _buildText, _outputSettings.buildStyle);

            GUI.Label(new Rect(new Vector2((Screen.width / 2) - 200, _actionSafeBottom - _buildInfoVerticalOffset), new Vector2(400, 50)), _buildDetails + "_" + buildDate, _outputSettings.dateStyle);
        }

        private void DisplayDebugCounters()
        {
            _debugCounter += 0x00723345654345A3;
            _debugCounter2 += 0x00723345654345A3;
            //Display alpha build text at the bottom center of the screen
            GUI.Label(new Rect(new Vector2(_actionSafeRight - 400, _actionSafeBottom - 50), new Vector2(400, 50)), _debugCounter.ToString("X16"), _outputSettings.debugCounterStyle);
            GUI.Label(new Rect(new Vector2(_actionSafeRight - 400, _actionSafeBottom - 65), new Vector2(400, 50)), _debugCounter2.ToString("X16"), _outputSettings.debugCounterStyle);
        }

        private void DisplayCompositionHelpers()
        {

            // TODO: Add horizontal reticle offset
            float reticleOffset = 0;

            #region Safe Frames
            if (_compositionSettings.showSafeFrames)
            {
                // Action safe Vertical
                GUI.DrawTexture(new Rect(_actionSafeLeft, _actionSafeTop + reticleOffset, 1, (Screen.height * 0.9f)), _compositionSettings.actionSafeTexture);
                GUI.DrawTexture(new Rect(_actionSafeRight, _actionSafeTop + reticleOffset, 1, (Screen.height * 0.9f)), _compositionSettings.actionSafeTexture);
                // Horizontal
                GUI.DrawTexture(new Rect(_actionSafeLeft, _actionSafeTop + reticleOffset, (Screen.width * 0.9f), 1), _compositionSettings.actionSafeTexture);
                GUI.DrawTexture(new Rect(_actionSafeLeft, _actionSafeBottom + reticleOffset, (Screen.width * 0.9f), 1), _compositionSettings.actionSafeTexture);

                // Title Safe Vertical
                GUI.DrawTexture(new Rect(Screen.width - (Screen.width * 0.9f), (Screen.height - (Screen.height * 0.8f)) / 2 + reticleOffset, 1, (Screen.height * 0.8f)), _compositionSettings.titleSafeTexture);
                GUI.DrawTexture(new Rect((Screen.width * 0.9f), (Screen.height - (Screen.height * 0.8f)) / 2 + reticleOffset, 1, (Screen.height * 0.8f)), _compositionSettings.titleSafeTexture);
                // Horizontal
                GUI.DrawTexture(new Rect(Screen.width - (Screen.width * 0.9f), (Screen.height - (Screen.height * 0.8f)) / 2 + reticleOffset, (Screen.width * 0.8f), 1), _compositionSettings.titleSafeTexture);
                GUI.DrawTexture(new Rect(Screen.width - (Screen.width * 0.9f), Screen.height * 0.9f + reticleOffset, (Screen.width * 0.8f), 1), _compositionSettings.titleSafeTexture);
            }
            #endregion


            // Rule of Thirds
            if (_compositionSettings.showRuleOfThirds)
                for (int i = 1; i < 3; i++)
                {
                    GUI.DrawTexture(new Rect(0, ((Screen.height / 3) * i) + reticleOffset, Screen.width, 1), _compositionSettings.ruleOfThirdsTexture);

                    GUI.DrawTexture(new Rect((Screen.width / 3) * i, 0, 1, Screen.height), _compositionSettings.ruleOfThirdsTexture);
                }

            // Cross point
            if (_compositionSettings.showCrossPoint)
            {
                GUI.DrawTexture(new Rect(0, (Screen.height / 2) + reticleOffset, Screen.width, 1), _compositionSettings.crossPointTexture);
                GUI.DrawTexture(new Rect((Screen.width / 2), 0, 1, Screen.height), _compositionSettings.crossPointTexture);
            }
        }
        #endregion

        #region Update Functions
        private void UpdateFrameRate()
        {
            _timeleft -= Time.deltaTime;
            _accum += Time.timeScale / Time.deltaTime;
            ++_frames;

            if (_timeleft <= 0.0)
            {
                float fps = _accum / _frames;

                if (0 > (int)fps)
                    fps = 0;

                frameRate = (int)fps;


                _timeleft = 0.1F;
                _accum = 0;
                _frames = 0;
            }
        }

        private void UpdateMessageDurations()
        {
            List<Message> frameMessages = new List<Message>();
            frameMessages.AddRange(_messages);

            foreach (Message message in frameMessages)
            {
                message.duration -= Time.deltaTime;
                if (message.duration <= 0)
                    _messages.Remove(message);
            }
        }
        #endregion

    }
}