using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Represents an unlockable item and its properties.
/// </summary>
[Serializable]
sealed internal class Unlockable
{
    [Tooltip("The name of the unlockable object.")]
    [SerializeField] private string _name;
    /// <summary>
    /// The name of the unlockable object.
    /// </summary>
    internal string Name { get { return _name; } }

    [Tooltip("The price of the unlockable object.")]
    [SerializeField] private int _price;
    /// <summary>
    /// The price of the unlockable object.
    /// </summary>
    internal int Price { get { return _price; } set { _price = value; } }

    [Tooltip("Indicates whether this item is unlocked.")]
    [SerializeField] private bool _unlocked;
    /// <summary>
    /// Indicates whether this item is unlocked.
    /// </summary>
    internal bool Unlocked { get { return _unlocked; } set { _unlocked = value; } }

    [Tooltip("Indicates if this item can be gifted to others.")]
    [SerializeField] private bool _allowGift;
    /// <summary>
    /// Indicates if this item can be gifted to others.
    /// </summary>
    internal bool AllowGift { get { return _allowGift; } set { _allowGift = value; } }

    [Tooltip("The preview image for the unlockable object.")]
    [SerializeField] private Sprite _preview;
    /// <summary>
    /// The preview image for the unlockable object.
    /// </summary>
    internal Sprite Preview { get { return _preview; } }
}
