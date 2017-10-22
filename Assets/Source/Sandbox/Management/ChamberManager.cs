using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

sealed internal class ChamberManager : MonoBehaviour
{
    #region Values
    // Constants / Static
    internal static ChamberManager singleton;
    private const int GENERATION_LOOK_AHEAD = 2;

    // Inspector
    [SerializeField]
    [Header("Chamber RNG")]
    [Tooltip("A serialized random state to begin with. If set, uses an initial RNG state. If blank, it creates a random one. Sets on game start.")]
    private string _initialChamberRNGState;

    [SerializeField]
    [Header("Chamber Autoload")]
    [Tooltip("The amount of chambers that are loaded at once. Once reaching the midpoint, chambers will be loaded/unloaded progressively.")]
    private ushort _chambersAtOnce;

    [SerializeField]
    [Tooltip("A game object used to determine collision/bounds entry.")]
    private Trigger _chamberTriggerPrefab;
    internal Trigger ChamberTriggerPrefab
    {
        get { return _chamberTriggerPrefab; }
    }

    [SerializeField]
    [Header("Chamber Generation Properties")]
    [Tooltip("The minimum amount of consecutive straight chambers required before considering turns.")]
    private ushort _minimumConsecutiveStraightChambers;

    [SerializeField]
    [Range(0, 1)]
    [Tooltip("The probability of a turn occurring if we are free to place turns.")]
    private float _probabilityOfTurn;

    [SerializeField]
    [Header("Starting Chambers")]
    [Tooltip("The chambers where the level begins. Must have a valid StartingCameraPosition component, entrance and exit point.")]
    private Chamber[] _startingChambers;

    [SerializeField]
    [Header("Continuous Chambers")]
    [Tooltip("The chambers responsible for continuing flow of the game. It must have a valid entrance and exit point.")]
    private Chamber[] _continuousChambers;

    [SerializeField]
    [Header("Progressive Cap Chambers")]
    [Tooltip("The chambers which replace the oldest unloaded chamber, restricting going backward.")]
    private Chamber[] _progressiveCapChambers;

    [SerializeField]
    [Header("Pickups")]
    [Range(0, 1)]
    [Tooltip("The probability of a pickup spawning that returns you to a normal state if you are under the influence of another pickup.")]
    private float _reversalPickupProbability;

    [SerializeField]
    [Tooltip("The minimum amount of chambers after a coin has been spawned before considering spawning another.")]
    private ushort _minChambersBetweenCoins;

    [SerializeField]
    [Range(0, 1)]
    [Tooltip("The probability of a coin pickup spawning, once available.")]
    private float _coinSpawnProbability;

    [SerializeField]
    private Pickup[] _pickups;


    // Runtime
    private RandomGen _chamberRNG;
    private RandomGen _pickupRNG;
    private int _currentChamberNumber;
    private int _nextChamberNumber;
    private int _lastCoinChamberNumber;
    private List<DifficultyRatable> _spawnedDifficults = new List<DifficultyRatable>();
    private List<ChamberPlacement> _placedChambers = new List<ChamberPlacement>();
    private ChamberPlacement _placedProgressiveCapChamber;
    private Dictionary<ChamberDirection, List<Chamber>> _continuousChambersByDirection;
    private Dictionary<PickupType, List<Pickup>> _pickupsByType;
    private PickupType _currentPickup;
    public PickupType CurrentPickup
    {
        get
        {
            return _currentPickup;
        }
        set
        {
            // Only track pickup states that are continuous so we can spawn reversal pickups, etc.
            if (value != PickupType.Coin && value != PickupType.New)
                _currentPickup = value;
        }
    }
    #endregion

    #region Unity Functions
    private void Awake()
    {
        Initialize();

        // Spawn our initial chambers, triggers will take over to spawn from here.
        GenerateChambers(_chambersAtOnce);
    }

    private void Update()
    {
        Development.Debug.ShowValue("Current Chamber", _currentChamberNumber + 1);
    }
    #endregion

    #region Functions
    public void Initialize()
    {
        // Set the singleton object.
        singleton = this;

        // Set some default values
        CurrentPickup = PickupType.Normal;
        _currentChamberNumber = -1;
        _lastCoinChamberNumber = short.MinValue; // must be further than minimum coin distance from 0, but not too small that it'd overflow on distance calculation.

        // Create our random number generators, and store/restore the chamber generator state.
        _pickupRNG = new RandomGen();
        _chamberRNG = new RandomGen();
        if (!string.IsNullOrEmpty(_initialChamberRNGState))
            _chamberRNG.SetStateJson(_initialChamberRNGState);
        else
            _initialChamberRNGState = _chamberRNG.GetStateJson();

        // Sort our chambers for quick lookups.
        _continuousChambersByDirection = new Dictionary<ChamberDirection, List<Chamber>>();
        _continuousChambersByDirection[ChamberDirection.Straight] = new List<Chamber>();
        _continuousChambersByDirection[ChamberDirection.Turn] = new List<Chamber>();
        foreach (Chamber chamber in _continuousChambers)
                _continuousChambersByDirection[chamber.ChamberDirection].Add(chamber);

        // Sort our pickups for quick lookups.
        _pickupsByType = new Dictionary<PickupType, List<Pickup>>();
        foreach (Pickup pickup in _pickups)
        {
            if(!_pickupsByType.ContainsKey(pickup.PickupType))
                _pickupsByType[pickup.PickupType] = new List<Pickup>();
            _pickupsByType[pickup.PickupType].Add(pickup);
        }
    }

    private Chamber GetRandomChamber(ChamberType chamberType)
    {
        Chamber[] chamberList;
        if (chamberType == ChamberType.StartingCap)
            chamberList = _startingChambers;
        else if (chamberType == ChamberType.ProgressiveCap)
            chamberList = _progressiveCapChambers;
        else
            chamberList = _continuousChambers;

        int randomIndex = _chamberRNG.Range(0, chamberList.Length);
        return chamberList[randomIndex];
    }
    
    private Chamber GetRandomChamberWithDirection(ChamberDirection chamberDirection)
    {
        int randomIndex = _chamberRNG.Range(0, _continuousChambersByDirection[chamberDirection].Count);
        return _continuousChambersByDirection[chamberDirection].ElementAt(randomIndex);
    }
    private Pickup GetRandomPickup()
    {
        int randomIndex = 0;

        // Check if what state our play is in.
        if (CurrentPickup != PickupType.Normal)
        {
            // If we've been affected by a pickup, then determine if we should spawn a reversal pickup.
            if (_pickupsByType.ContainsKey(PickupType.Normal) &&_pickupRNG.Range(0, 1.0f) <= _reversalPickupProbability)
            {
                // Select a random reversal pickup and return it.
                randomIndex = _pickupRNG.Range(0, _pickupsByType[PickupType.Normal].Count);
                return _pickupsByType[PickupType.Normal][randomIndex];
            }
        }

        // If we passed our minimum coin spawn distance..
        if (_currentChamberNumber - _lastCoinChamberNumber >= _minChambersBetweenCoins)
        {
            // Determine if we will place a coin.
            if (_pickupsByType.ContainsKey(PickupType.Coin) && _pickupRNG.Range(0, 1.0f) <= _coinSpawnProbability)
            {
                // Set our last coin spawned chamber as this one.
                _lastCoinChamberNumber = _currentChamberNumber;

                // Select a random reversal pickup and return it.
                randomIndex = _pickupRNG.Range(0, _pickupsByType[PickupType.Coin].Count);
                return _pickupsByType[PickupType.Coin][randomIndex];
            }
        }

        // Otherwise, we'll select a non-normal, random pickup.
        List<PickupType> spawnablePickupTypes = _pickupsByType.Keys.Where(x => x != PickupType.Normal && x != PickupType.Coin).ToList();

        // Determine if we should allow new ninjas (when the amount the powerup restores
        if (SandboxManager.singleton.Player.TotalBipedCount - SandboxManager.singleton.Player.ActiveBipedCount < Globals.singleton.bipeds.newPowerupBipedCount)
            spawnablePickupTypes.Remove(PickupType.New);

        // Obtain a random pickup of a random pickup type.
        randomIndex = _pickupRNG.Range(0, spawnablePickupTypes.Count);
        PickupType randomType = spawnablePickupTypes[randomIndex];
        randomIndex = _pickupRNG.Range(0, _pickupsByType[randomType].Count);
        return _pickupsByType[randomType][randomIndex];
    }
    private ChamberDirection GetRandomChamberDirection()
    {
        float random = _chamberRNG.Range(0, 1.0f);
        if (random <= _probabilityOfTurn)
            return ChamberDirection.Turn;
        else
            return ChamberDirection.Straight;
    }

    private void SpawnChambers(ChamberPlacement[] chamberPlacements, int index, int count)
    {
        // Loop through all items to spawn.
        for (int i = index; i < count; i++)
        {
            // Spawn the indexed chamber.
            Chamber chamber = (Chamber)SpawnDifficultyRatedObject(chamberPlacements[i].ChamberPrefab, chamberPlacements[i].Position, chamberPlacements[i].Rotation);
            chamberPlacements[i].RuntimeChamber = chamber;

            // Create the corresponding trigger for autoloading.
            Bounds chamberBounds = chamberPlacements[i].Bounds;
            Trigger chamberTrigger = Instantiate<Trigger>(_chamberTriggerPrefab, chamberBounds.center, Quaternion.identity, chamber.transform);
            chamberTrigger.GetComponent<BoxCollider2D>().size = new Vector2(chamberBounds.size.x, chamberBounds.size.y);
            chamberTrigger.Chamber = chamber;
            chamberTrigger.ChamberNumber = _nextChamberNumber++;
            chamberPlacements[i].RuntimeTrigger = chamberTrigger;

            // Add our chamber placement to the placed chambers list.
            _placedChambers.Add(chamberPlacements[i]);
        }
    }
    private void SpawnPickup(ChamberPlacement chamberPlacement)
    {
        // Obtain our chamber from this placement
        Chamber chamber = chamberPlacement.RuntimeChamber;

        // If we have pickup spawn points
        if (chamber.PickupPoints.Length > 0)
        {
            // Obtain a spawn point and pickup, and spawn it.
            Pickup randomPickup = GetRandomPickup();
            Vector3 randomPickupPoint = chamber.transform.TransformPoint(chamber.PickupPoints.Random(_pickupRNG));
            Pickup pickup = (Pickup)SpawnDifficultyRatedObject(randomPickup, randomPickupPoint, Quaternion.identity);
            chamberPlacement.RuntimePickup = pickup;
        }
    }
    private DifficultyRatable SpawnDifficultyRatedObject(DifficultyRatable difficult, Vector3 position, Quaternion rotation)
    {
        GameObject newnewDifficultGameObject = Instantiate(difficult.gameObject, position, rotation, Globals.singleton.containers.chambers);
        DifficultyRatable newDifficult = newnewDifficultGameObject.GetComponent<DifficultyRatable>();
        _spawnedDifficults.Add(newDifficult);
        return newDifficult;
    }

    private int GetStraightChamberCount(ChamberPlacement[] incompletePlacements, int currentIndex)
    {
        // Obtain the number of straight chambers before the current.
        int straightCount = 0;
        bool hitTurn = false;

        // First check incomplete placements before this one.
        for (int x = currentIndex - 1; x >= 0; x--)
        {
            if (incompletePlacements[x].ChamberPrefab.ChamberDirection == ChamberDirection.Straight)
            {
                straightCount++;
            }
            else
            {
                hitTurn = true;
                break;
            }
        }

        // And if we didn't hit a turn yet, keep searching in our completed placements.
        if (!hitTurn)
        {
            for (int x = _placedChambers.Count - 1; x >= 0; x--)
            {
                if (_placedChambers[x].ChamberPrefab.ChamberDirection == ChamberDirection.Straight)
                {
                    straightCount++;
                }
                else
                {
                    hitTurn = true;
                    break;
                }
            }
        }
        return straightCount;
    }
    private void GenerateChambers(int count)
    {
        // Create an array of chamber placements to decide.
        ChamberPlacement[] chamberPlacements = new ChamberPlacement[count + GENERATION_LOOK_AHEAD];

        // Figure out placement for every chamber to create.
        bool hitConflict = false;
        for (int i = 0; i < chamberPlacements.Length; i++)
        {
            // Obtain the appropriate chamber prefab.
            Chamber chamberPrefab = null;
            if(i == 0 && _placedChambers.Count == 0)
            {
                // If we have not created any chambers yet, this chamber will need to be a starting chamber.
                chamberPrefab = GetRandomChamber(ChamberType.StartingCap);
            }
            else if (hitConflict)
            {
                // If we hit a conflict, we'll allow for any chamber here to minimize the chances.
                chamberPrefab = GetRandomChamber(ChamberType.Tunnel);
            }
            else
            {
                // Obtain our count of consecutive straight chambers beforehand.
                int straightCount = GetStraightChamberCount(chamberPlacements, i);

                // Obtain our chamber direction, making sure we hit the minimum amount of straights.
                ChamberDirection targetDirection = ChamberDirection.Straight;
                if(straightCount >= _minimumConsecutiveStraightChambers)
                    targetDirection = GetRandomChamberDirection();

                // Obtain our chamber with the given direction.
                chamberPrefab = GetRandomChamberWithDirection(targetDirection);
            }

            // Determine the positioning of our next chamber.
            Direction targetEntranceDirection = Direction.Left;
            Vector2 targetEntrancePoint = Vector2.zero;
            if (i > 0)
            {
                // We have determined placements in this run already, so look at the previous placement.
                ChamberPlacement lastChamberPlacement = chamberPlacements[i - 1];
                targetEntranceDirection = lastChamberPlacement.ExitDirection.Inverse();
                targetEntrancePoint = lastChamberPlacement.ExitPoint;
            }
            else if (_placedChambers.Count > 0)
            {
                // We have no placements in this run, look to the spawned chambers.
                ChamberPlacement lastChamberPlacement = _placedChambers[_placedChambers.Count - 1];
                targetEntranceDirection = lastChamberPlacement.ExitDirection.Inverse();
                targetEntrancePoint = lastChamberPlacement.ExitPoint;
            }
            else
            {
                // It's our first chamber, find a random orientation to simulate entry from.
                targetEntranceDirection = (Direction)_chamberRNG.Range(0, 2);
            }


            // Create our corresponding chamber placement to create and verify the validity of placements.
            chamberPlacements[i] = new ChamberPlacement(_chamberRNG, chamberPrefab, targetEntranceDirection);
            chamberPlacements[i].SetEntrancePoint(targetEntrancePoint);

            // Loop backward through the chamber placements to check for overlaps (skip immediate previous one since it will touch)
            bool overlapped = false;
            Bounds boundsA = chamberPlacements[i].Bounds;
            for (int x = i - 2; x >= 0; x--)
            {
                Bounds boundsB = chamberPlacements[x].Bounds;
                if (boundsA.Intersects(boundsB))
                {
                    // There was an overlap with chamberPlacement[i] and chamberPlacement[x], we'll want to regenerate.
                    i = x - 1;
                    overlapped = true;
                    hitConflict = true;
                    break;
                }
            }
            // If we overlapped already, we'll want to start our iteration from the new point we set.
            if (overlapped)
                continue;

            // Now we'll want to loop backwards through spawned chambers, and check for overlaps as well.
            overlapped = false;
            // Define the last chamber to start from, if we're generating our first placement, we'll want to skip the last spawned chamber since it will touch it.
            int lastChamber = (i == 0) ? _placedChambers.Count - 2 : _placedChambers.Count - 1;
            for (int x = lastChamber; x >= 0; x--)
            {
                Bounds boundsB = _placedChambers[x].Bounds;
                if (boundsA.Intersects(boundsB))
                {
                    // There was an overlap with chamberPlacement[i] and chamberPlacement[x], we'll want to regenerate.
                    i = -1;
                    overlapped = true;
                    hitConflict = true;
                    break;
                }
            }
            // If we overlapped already, we'll want to start our iteration from the new point we set.
            if (overlapped)
                continue;

            // Reset our conflict flag.
            hitConflict = false;
        }

        // Spawn all of the placed chambers.
        SpawnChambers(chamberPlacements, 0, count);
    }
    internal void UpdateAutoGeneration(int enteredChamberNumber)
    {
        // Obtain our first available chamber number.
        int firstChamberNumber = _nextChamberNumber - _chambersAtOnce;
        int enteredChamberIndex = enteredChamberNumber - firstChamberNumber;

        // Generate all pickups for the entered chamber.
        SpawnPickup(_placedChambers[enteredChamberIndex]);

        // Verify we're supposed to load more chambers at this point.
        int middleChamberIndex = _chambersAtOnce / 2;
        if (enteredChamberIndex > middleChamberIndex)
        {
            // Figure out how many chambers are to be generated.
            int generationCount = enteredChamberIndex - middleChamberIndex;

            // Generate new chambers.
            GenerateChambers(generationCount);

            // Destroy old chambers.
            for (int i = 0; i < generationCount; i++)
            {
                _placedChambers[0].DestroyRuntimeObjects();
                _placedChambers.RemoveAt(0);
            }


            // Now we'll want to create a corresponding cap chamber to restrict the player from backpedalling where the old chambers were destroyed.

            // If the cap chamber placement already exists, destroy the runtime objects.
            if (_placedProgressiveCapChamber != null)
                _placedProgressiveCapChamber.DestroyRuntimeObjects();

            // Next we'll want to figure out how to place our new cap chamber.
            Chamber progressiveCapPrefab = GetRandomChamber(ChamberType.ProgressiveCap);
            Direction exitDirection = _placedChambers[0].EntranceDirection.Inverse();
            Vector2 exitPoint = _placedChambers[0].EntrancePoint;

            // Create our chamber placement with respect to our determined placement properties.
            _placedProgressiveCapChamber = new ChamberPlacement(_chamberRNG, progressiveCapPrefab, exitDirection);
            _placedProgressiveCapChamber.SetEntrancePoint(exitPoint);

            // Spawn the cap chamber.
            _placedProgressiveCapChamber.RuntimeChamber = (Chamber)SpawnDifficultyRatedObject(_placedProgressiveCapChamber.ChamberPrefab, _placedProgressiveCapChamber.Position, _placedProgressiveCapChamber.Rotation);

        }
        _currentChamberNumber = Math.Max(_currentChamberNumber, enteredChamberNumber);
    }


    #endregion

    #region Classes
    private class ChamberPlacement
    {
        #region Values
        // Fields
        private Chamber _chamberPrefab;
        private bool _entranceIsA;
        private QuarterlyRotation _rotation;

        // Properties
        public Bounds Bounds
        {
            get
            {
                Vector3 position = ChamberPrefab.Bounds.center.RotateAround(ChamberPrefab.transform.position, Rotation) + (Vector3)Position;
                Vector3 size = Rotation * ChamberPrefab.Bounds.size;
                size = size.Abs();
                return new Bounds(position, size);
            }
        }
        public Chamber ChamberPrefab
        {
            get
            {
                return _chamberPrefab;
            }
        }
        public Vector2 Position
        {
            get; set;
        }
        public Quaternion Rotation
        {
            get
            {
                return Quaternion.identity.Rotate(-90 * (float)_rotation, Vector3.forward);
            }
        }
        public Vector2 EntrancePoint
        {
            get
            {
                return Position + (_entranceIsA ? ChamberPrefab.EntryPointA : ChamberPrefab.EntryPointB).RotateClockwise((float)_rotation * 90);
            }
        }
        public Direction EntranceDirection
        {
            get
            {
                return (_entranceIsA ? ChamberPrefab.EntryDirectionA : ChamberPrefab.EntryDirectionB).RotateClockwise(_rotation);
            }
        }
        public Vector2 ExitPoint
        {
            get
            {
                return Position + (!_entranceIsA ? ChamberPrefab.EntryPointA : ChamberPrefab.EntryPointB).RotateClockwise((float)_rotation * 90);
            }
        }
        public Direction ExitDirection
        {
            get
            {
                return (!_entranceIsA ? ChamberPrefab.EntryDirectionA : ChamberPrefab.EntryDirectionB).RotateClockwise(_rotation);
            }
        }

        public Chamber RuntimeChamber
        {
            get; set;
        }
        public Pickup RuntimePickup
        {
            get; set;
        }
        public Trigger RuntimeTrigger
        {
            get; set;
        }
        #endregion

        #region Constructors
        public ChamberPlacement(RandomGen randomGen, Chamber chamberPrefab)
        {
            _chamberPrefab = chamberPrefab;
            _entranceIsA = randomGen.Range(0, 2) == 0;
        }
        public ChamberPlacement(RandomGen randomGen, Chamber chamberPrefab, Direction entranceDirection) : this(randomGen, chamberPrefab)
        {
            _rotation = EntranceDirection.GetClockwiseRotation(entranceDirection);
        }
        #endregion

        #region Functions
        public void SetEntrancePoint(Vector2 position)
        {
            // Set our entrance position to be at the given position.
            Position = Position + (position - EntrancePoint);
        }
        public void DestroyRuntimeObjects()
        {
            // If we have any runtime objects, destroy them.
            if (RuntimeChamber != null)
                Destroy(RuntimeChamber.gameObject);
            if (RuntimeTrigger != null)
                Destroy(RuntimeTrigger.gameObject);
            if (RuntimePickup != null)
                Destroy(RuntimePickup.gameObject);
        }
        #endregion
    }
    #endregion
}
