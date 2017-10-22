using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

sealed internal class Player : Controllable
{
    #region Values
    [SerializeField] private Profile _profile = new Profile();
    internal Profile Profile { get { return _profile; }}
    
    [SerializeField] private Inputter _inputter;
    internal Inputter Inputter { get { return _inputter; } }

    [SerializeField] private GameCamera _cameraController;
    internal GameCamera GameCamera { get { return _cameraController; } }

    [SerializeField] private UISignal _uISignal;
    internal UISignal UIEventDelegate { get { return _uISignal; } }

    [SerializeField] private List<Biped> _bipeds = new List<Biped>();

    private float _nextAllowBipedSpawnTime;
    private int _deadBipedCount;
    private int _killStrayStepCount;
    private Vector2 _bipedsCentroid;
    internal Vector2 BipedsCentroid
    {
        get { return _bipedsCentroid; }
    }
    private Biped ClosestActiveBipedToCamera
    {
        get
        {
            Biped closestBiped = null;
            foreach (Biped biped in _bipeds)
            {
                float closestMagnitude = Mathf.Infinity;
                if (biped.gameObject.activeSelf)
                {
                    float sqrMagnitude = (_cameraController.DepthlessPosition - (Vector2)biped.transform.position).sqrMagnitude;
                    if (sqrMagnitude < closestMagnitude)
                        closestBiped = biped;
                }
            }
            return closestBiped;
        }
    }
    private bool AllBipedsSpawned
    {
        get{ return _bipeds.Count == TotalBipedCount; }
    }
    internal int ActiveBipedCount
    {
        get
        {
            int count = 0;
            foreach (Biped biped in _bipeds)
            {
                if (biped.gameObject.activeSelf)
                    count++;
            }
            return count;
        }
    }
    internal int TotalBipedCount
    {
        get
        {
            return _profile.bipedDesigns.Length;
        }
    }

    private float _nextAllowPickup;
    #endregion

    #region Unity Functions
    private void Awake()
    {
        Initialize();
    }

    private void Update()
    {
        UpdateBipedsCentroid();
        UpdateKillStrayBipeds();
    }
    #endregion

    #region Functions
    private void Initialize()
    {
        if (GameManager.singleton == null)
            GameManager.ForceReturnToMenu();

        _profile = GameManager.singleton.Profile;
        SetAspects(this, new Controller());
        SetInputEvents(true);

        _inputter.SetAspects(this);
        _cameraController.SetAspects(this);
    }
    internal override bool SetInputEvents(bool state)
    {
        if (base.SetInputEvents(state))
        {
            if (state)
            {
                _controller.OnRotate += Rotate;
                _controller.OnTouch += Touch;
            }
            else
            {
                _controller.OnRotate -= Rotate;
                _controller.OnTouch -= Touch;
            }
        }
        return true;
    }

    private void Rotate(float direction)
    {
        if (_cameraController.SuggestWorldRotationHalt)
            return;
        SandboxManager.singleton.RotateWorld(direction);
    }
    internal void Touch(Vector2 position)
    {
        if (!AllowTouch(position))
            return;

        Vector2 worldTouchPosition = _cameraController.ScreenToWorldPoint(position);
        TrySpawnBiped(Globals.singleton.bipeds.biped, worldTouchPosition);
    }
    private bool AllowTouch(Vector2 position)
    {
        float borderWeight = Globals.singleton.bipeds.disallowTouchBorderWeight;
        bool left = position.x >= borderWeight;
        if (!left)
            return false;
        bool right = position.x <= ((float)Screen.width - borderWeight);
        if (!right)
            return false;
        bool top = position.y <= ((float)Screen.height - borderWeight);
        if (!top)
            return false;
        bool bottom = position.y >= borderWeight;
        if (!bottom)
            return false;
        return true;
    }

    private void TrySetCameraTarget()
    {
        if (!_cameraController.HasFollowTarget)
            _cameraController.FollowTarget = ClosestActiveBipedToCamera;
    }

    private void UpdateBipedsCentroid()
    {
        Vector2 centroid = Vector2.zero;
        Biped[] bipeds = Globals.singleton.containers.bipeds.GetComponentsInChildren<Biped>();
        int count = 0;
        for (int i = 0; i < bipeds.Length; i++)
            if (!bipeds[i].Dead)
            {
                centroid += (Vector2)bipeds[i].transform.position;
                count++;
            }
        centroid /= count;
        _bipedsCentroid = centroid;
    }
    private void UpdateKillStrayBipeds()
    {
        if (!AllBipedsSpawned)
            return;

        _killStrayStepCount++;
        if (_killStrayStepCount != Globals.singleton.bipeds.killStraysFrameRate)
            return;
        _killStrayStepCount = 0;

        Vector3 cameraPosition = _cameraController.DepthlessPosition;
        float distance = Globals.singleton.bipeds.strayBipedDistance * Globals.singleton.bipeds.strayBipedDistance;
        foreach (Biped biped in _bipeds)
        {
            if (!biped.gameObject.activeSelf)
                continue;
            if (biped.RendererVisible)
                continue;
            float sqrMagnitude = (cameraPosition - biped.transform.position).sqrMagnitude;
            if (sqrMagnitude > distance)
                biped.Die();
        }
    }

    internal void BipedDied(Biped biped)
    {
        _deadBipedCount++;
        _cameraController.StartShake();
        OutputBipedCounts();

        if (AllBipedsSpawned)
        {
            TrySetCameraTarget();
            if (ActiveBipedCount == 0)
                AllBipedsDead();
        }
    }
    private void TryRespawnNinjas(int count, Biped biped)
    {
        int spawnedCount = 0;
        foreach (Biped existingBiped in _bipeds)
        {
            if (!existingBiped.gameObject.activeSelf)
            {
                existingBiped.Respawn(biped.transform.position, biped.transform.rotation);
                spawnedCount++;
                if (spawnedCount == count)
                    break;
            }
        }
        OutputBipedCounts();
    }
    private void CreateBiped(Biped biped, Vector3 position)
    {
        GameObject newBipedGameObject = Instantiate(biped.gameObject, position, Quaternion.identity, Globals.singleton.containers.bipeds);
        Biped newBiped = newBipedGameObject.GetComponent<Biped>();
        _bipeds.Add(newBiped);
        newBiped.SetAspects(this);
        newBiped.SetDesign(_profile.bipedDesigns[_bipeds.Count - 1]);
        OutputBipedCounts();
    }
    internal void TrySpawnBiped(Biped biped, Vector3 position)
    {
        if (AllBipedsSpawned)
            return;
        if (Physics2D.OverlapCircle(position, Globals.singleton.bipeds.bipedSpawnCheckRadius, ~Globals.singleton.bipeds.bipedSpawnCheckIgnoredLayers) != null)
            return;
        if (Time.time < _nextAllowBipedSpawnTime)
            return;
        _nextAllowBipedSpawnTime = Time.time + Globals.singleton.bipeds.bipedSpawnNextDelay;

        CreateBiped(biped, position);

        if (AllBipedsSpawned)
        {
            _controller.AllowMovement = true;

            TrySetCameraTarget();
            UIEventDelegate.Invoke(SandboxEvent.AllBipedsSpawned);
        }
    }
    
    internal void AddCoin(PointType pointType)
    {
        _profile.Coins += 1;
    }
    internal void AddPoints(PointType pointType)
    {
        _profile.Score += ActiveBipedCount;
        _uISignal.Output(SandboxValue.Score, _profile.Score.ToString());
    }
    internal bool TryPickupAllBipeds(PickupType pickupType, Biped biped)
    {
        if (!AllBipedsSpawned)
            return false;

        if (pickupType != PickupType.Coin)
        {
            if (_nextAllowPickup > Time.time)
                return false;
            _nextAllowPickup = Time.time + Globals.singleton.bipeds.nextPowerupPickupDelay;
            AddPoints(PointType.Powerup);
        }

        switch (pickupType)
        {
            case PickupType.Nitro:
                _uISignal.Invoke(SandboxEvent.Nitro);
                break;
            case PickupType.Normous:
                _uISignal.Invoke(SandboxEvent.Normous);
                break;
            case PickupType.Normal:
                _uISignal.Invoke(SandboxEvent.Normal);
                break;
            case PickupType.New:
                TryRespawnNinjas(Globals.singleton.bipeds.newPowerupBipedCount, biped);
                _uISignal.Invoke(SandboxEvent.New);
                break;
            case PickupType.Coin:
                _profile.Coins += 1;
                GameManager.singleton.SaveProfile();
                break;
            default:
                break;
        }
        foreach (Biped existingBiped in _bipeds)
            existingBiped.Powerup(pickupType);
        return true;
    }

    private void AllBipedsDead()
    {
        _cameraController.SetInputEvents(false);
        _uISignal.Invoke(SandboxEvent.GameOver);
    }
    private void OutputBipedCounts()
    {
        _uISignal.Output(SandboxValue.NinjaCount, ActiveBipedCount.ToString("00"));
        _uISignal.Output(SandboxValue.DeadNinjaCount, _deadBipedCount.ToString());
    }
    #endregion
}
