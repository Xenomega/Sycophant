using System;
using UnityEngine;

sealed internal class Globals : MonoBehaviour
{
    internal static Globals singleton;

    #region Values
    internal const int BIPED_LAYER = 8;
    internal const int PROJECTILE_LAYER = 9;
    internal const int MACHINE_LAYER = 10;
    internal const int WATER_LAYER = 4;

    private ulong _physicsUpdateCount;
    public ulong PhysicsUpdateCount
    {
        get { return _physicsUpdateCount; }
    }
    private ulong _updateCount;
    public ulong UpdateCount
    {
        get { return _updateCount; }
    }
    [SerializeField] internal Player player;

    [Serializable]
    internal struct Bipeds
    {
        [SerializeField] internal Biped biped;
        [Header("Spawning")]
        [SerializeField] internal float bipedSpawnNextDelay;
        [SerializeField] internal float bipedSpawnCheckRadius;
        [SerializeField] internal LayerMask bipedSpawnCheckIgnoredLayers;
        [SerializeField] internal float disallowTouchBorderWeight;

        [Header("Grouping")]
        [SerializeField] internal float groupingFarThresholdSqrd;
        [SerializeField] internal float groupingBoostFraction;

        [Header("Powerups")]
        [SerializeField] internal int newPowerupBipedCount;
        [SerializeField] internal float nextPowerupPickupDelay;

        [Header("Strays")]
        [SerializeField] internal int killStraysFrameRate;
        [SerializeField] internal float strayBipedDistance;
    }
    [SerializeField] internal Bipeds bipeds;

    [Serializable]
    internal struct Containers
    {
        [SerializeField] internal Transform players;
        [SerializeField] internal Transform cameras;
        [SerializeField] internal Transform bipeds;
        [SerializeField] internal Transform chambers;
        [SerializeField] internal Transform effects;
        [SerializeField] internal Transform projectiles;
    }
    [SerializeField] internal Containers containers;
    
    #endregion

    #region Unity Functions
    private void Awake()
    {
        singleton = this;
    }
    private void FixedUpdate()
    {
        _physicsUpdateCount++;
    }
    private void Update()
    {
        _updateCount++;
    }
    #endregion
}
