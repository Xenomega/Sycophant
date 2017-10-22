using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

sealed internal class UISignal : MonoBehaviour
{
    #region Values
    [Serializable]
    internal class Event
    {
        [SerializeField] internal SandboxEvent sandboxEvent;
        [SerializeField] internal UnityEvent unityEvent;
    }
    [SerializeField] private List<Event> _events;

    [Serializable]
    internal class Value
    {
        [SerializeField] internal SandboxValue sandboxValue;
        [SerializeField] internal string stringFormat;
        [SerializeField] internal Text text;
    }
    [SerializeField] private List<Value> _values;
    #endregion

    #region Unity Functions
    private void Awake()
    {
    }
    #endregion

    #region Functions
    internal void Invoke(SandboxEvent sandboxEvent)
    {
        Event aEvent = _events.Find(e => e.sandboxEvent == sandboxEvent);
        if (aEvent != null)
            aEvent.unityEvent.Invoke();
    }

    internal void Output(SandboxValue sandboxValue, string value)
    {
        Value aValue = _values.Find(e => e.sandboxValue == sandboxValue);
        if (aValue != null)
        {
            if (aValue.text == null)
                return;
            string valueString = aValue.stringFormat == string.Empty ? value : String.Format(aValue.stringFormat, value);
            aValue.text.text = valueString;
        }
    }
    #endregion
}
