using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CircleCollider2D))]
sealed internal class Pickup : DifficultyRatable
{
    [SerializeField] private PickupType _pickupType;
    internal PickupType PickupType
    {
        get { return _pickupType; }
    }
    private SpriteRenderer _spriteRenderer;
    #region Unity Functions
    private void Awake()
    {
        Initialize();
    }

    private void OnTriggerEnter2D(Collider2D collider2D)
    {
        ProcessEntry(collider2D);
    }
    #endregion

    #region Functions
    private void Initialize()
    {
        _spriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();
    }

    private void ProcessEntry(Collider2D collider2D)
    {
        if (collider2D.gameObject.layer != Globals.BIPED_LAYER)
            return;
        if (!_spriteRenderer.isVisible)
            return;

        Biped biped = collider2D.gameObject.GetComponent<Biped>();
        bool applied = TryApply(biped);
        if(applied)
            Destroy(this.gameObject);
    }

    private bool TryApply(Biped biped)
    {
        // Attempt to apply the powerup to all bipeds.
        bool applied = biped.Player.TryPickupAllBipeds(_pickupType, biped);
        if (!applied)
            return false;

        // If we applied the powerup, update our chamber manager which uses this to determine powerup spawns.
        if (ChamberManager.singleton != null)
            ChamberManager.singleton.CurrentPickup = _pickupType;
        return true;
    } 
    #endregion
}
