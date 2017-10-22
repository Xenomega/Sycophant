using System.Collections;
using System.Collections.Generic;
using UnityEngine;

sealed public class Chamber : DifficultyRatable
{
    #region Values
    private const string VARIATIONS_GAME_OBJECT_NAME = "variations";
    private const string VARIATION_GAME_OBJECT_NAME = "variation";

    [SerializeField] private Bounds _bounds;
    public Bounds Bounds
    {
        get
        {
            return _bounds;
        }
        set
        {
            _bounds = value;
        }
    }
    
    private bool _containsWater;
    internal bool ContainsWater { get { return _containsWater; } }

    [SerializeField] private Vector2 _entryPointA;
    internal Vector2 EntryPointA { get { return _entryPointA; } }
    internal Vector2 WorldEntryPointA { get { return this.transform.TransformPoint(_entryPointA); } }

    [SerializeField] private Direction _entryDirectionA;
    internal Direction EntryDirectionA { get { return _entryDirectionA; } }

    [SerializeField] private Vector2 _entryPointB;
    internal Vector2 EntryPointB { get { return _entryPointB; } }
    internal Vector2 WorldEntryPointB { get { return this.transform.TransformPoint(_entryPointB); } }

    [SerializeField] private Direction _entryDirectionB;
    internal Direction EntryDirectionB { get { return _entryDirectionB; } }

    internal ChamberDirection ChamberDirection
    {
        get
        {
            if (EntryDirectionA == EntryDirectionB || EntryDirectionA == EntryDirectionB.Inverse())
                return ChamberDirection.Straight;
            else
                return ChamberDirection.Turn;
        }
    }

    [SerializeField] private Vector2[] _powerupPoints;
    internal Vector2[] PickupPoints
    {
        get { return _powerupPoints; }
    }
    internal bool IsStartingChamber { get { return this.GetComponent<CameraStartingPosition>() != null; } }

    #endregion

    #region Unity Functions
    private void Awake()
    {
        SetWaterContainment(this.transform);
    }

    private void OnDrawGizmos()
    {
        Matrix4x4 gizmoMatrixBackup = Gizmos.matrix;
        Gizmos.matrix = this.transform.localToWorldMatrix;

        Gizmos.DrawWireCube(Bounds.center, Bounds.extents * 2);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(_entryPointA, Vector3.one * 0.5f);
        Gizmos.DrawWireCube(_entryPointB, Vector3.one * 0.5f);

        Gizmos.color = Color.cyan;
        if (_powerupPoints.Length > 0)
        {
            foreach (Vector2 powerupPoint in _powerupPoints)
                Gizmos.DrawWireSphere(powerupPoint, 0.5f);
        }

        Gizmos.matrix = gizmoMatrixBackup;
    }
    #endregion

    #region Functions
    private void SetWaterContainment(Transform transform)
    {
        foreach (Transform childTransform in transform)
        {
            if (childTransform.GetComponent<Water>())
            {
                _containsWater = true;
                return;
            }
            SetWaterContainment(childTransform);
        }
    }

    internal void SetWeaponsActive(bool state)
    {
        Weapon[] weapons = this.gameObject.GetComponentsInChildren<Weapon>();
        foreach (Weapon weapon in weapons)
        {
            if (weapon != null)
                weapon.AllowFire = state;
        }
    }
    #endregion
}
