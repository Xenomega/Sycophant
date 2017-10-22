using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(RectTransform))]
sealed internal class UIListItemUnlockable : UIListItem
{
    private const string LOCKED_TEXT = "Use Coins";
    [SerializeField] private Color _unlockedTextColor;
    [SerializeField] private Color _lockedTextColor;
    private Unlockable _unlockable;
    private Action<Unlockable, UIListItemUnlockable> _unlockableCallback;
    private UIListUnlockable _uIListUnlockable;

    internal void Populate(Unlockable unlockable, UIListUnlockable uIListUnlockable, Action<Unlockable, UIListItemUnlockable> callback)
    {
        _unlockable = unlockable;
        _uIListUnlockable = uIListUnlockable;
        _unlockableCallback = callback;
        Display();
    }
    private void Display()
    {
        _rawImage.texture = _unlockable.Preview.texture;
        _text.text = _unlockable.Unlocked ? _unlockable.Name : LOCKED_TEXT;
        _text.color = _unlockable.Unlocked ? _unlockedTextColor : _lockedTextColor;
    }

    internal void Refresh()
    {
        Display();
    }
    public override void CallbackClick()
    {
        _unlockableCallback.Invoke(_unlockable, this);
    }
}
