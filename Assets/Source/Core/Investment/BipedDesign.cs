using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Represents a biped design, composed of a sprite and effect variant, from the investment manager.
/// </summary>
[Serializable]
sealed internal class BipedDesign
{
    [Tooltip("The name of the biped design.")]
    [SerializeField] private string _name;
    /// <summary>
    /// The name of the biped design.
    /// </summary>
    internal string Name { get { return _name; } set { _name = value; } }

    [Tooltip("The index of the sprite in the investment manager that represents this design.")]
    [SerializeField] private int _spriteIndex;
    /// <summary>
    /// The index of the sprite in the investment manager that represents this design.
    /// </summary>
    internal int SpriteIndex { get { return _spriteIndex; } set { _spriteIndex = Mathf.Clamp(value, 0, InvestmentManager.singleton.CostumeSprites.Length - 1); } }

    [Tooltip("The index of the spawn/despawn effect in the investment manager for this biped.")]
    [SerializeField] private int _effectIndex;
    /// <summary>
    /// The index of the spawn/despawn effect in the investment manager for this biped.
    /// </summary>
    internal int EffectIndex { get { return _effectIndex; } set { _effectIndex = Mathf.Clamp(value, 0, InvestmentManager.singleton.Effects.Length - 1); } }

    /// <summary>
    /// The default constructor
    /// </summary>
    /// <param name="name">The name of the biped design.</param>
    /// <param name="sprite">The index of the sprite in the investment manager that represents this design.</param>
    /// <param name="deathEffect">The index of the spawn/despawn effect in the investment manager for this biped.</param>
    internal BipedDesign(string name, int sprite, int deathEffect)
    {
        _name = name;
        _spriteIndex = sprite;
        _effectIndex = deathEffect;
    }
}