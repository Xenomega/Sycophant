using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
sealed internal class GameCamera : Controllable
{
    #region Values
    [Header("Rotation")]
    [SerializeField] private float _rotationCorrectionRate;
    private float _targetAngle;
    private float _recentMoveDirection;
    internal bool SuggestWorldRotationHalt
    {
        get
        {
            return _nauseous || !HasFollowTarget;
        }
    }

    [Header("Translation")]
    [SerializeField] private Vector3 _followOffset;
    [SerializeField] private float _positionCorrectionRate;
    private Biped _followTarget;
    internal Biped FollowTarget
    {
        get { return _followTarget; }
        set { _followTarget = value; }
    }
    internal bool HasFollowTarget
    {
        get { return _followTarget != null && !_followTarget.Dead; }
    }
    internal Vector2 DepthlessPosition
    {
        get
        {
            return this.transform.position;
        }
    }

    [Header("Nausea")]
    [SerializeField] private float _nauseaThreshold;
    [SerializeField] private float _nauseaAdditionPerRotation;
    [SerializeField] private float _nauseaCoolRate;
    [SerializeField] private float _nauseatedRotationAddition;
    private bool _nauseous;
    private float _nauseaLevel;

    [Header("Shake")]
    [SerializeField] private float _shakeDuration;
    [SerializeField] private float _shakeIntensity;
    private float _shakeKillTime;

    private Camera _camera;
    #endregion

    #region Unity Functions
    private void Awake()
    {
        Initialize();
    }

    private void Update()
    {
        UpdatePosition();
        UpdateRotation();
        UpdateNausea();
        UpdateShake();
    }
    #endregion

    #region Functions
    private void Initialize()
    {
        _camera = this.gameObject.GetComponent<Camera>();

        this.transform.position = _followOffset;
    }
    internal override bool SetInputEvents(bool state)
    {
        if (base.SetInputEvents(state))
        {
            if (state)
            {
                _controller.OnRotate += Rotate;
            }
            else
            {
                _controller.OnRotate -= Rotate;
            }
        }
        return true;
    }
    internal override void SetAspects(Player player, Controller controller)
    {
        base.SetAspects(player, controller);
        SetInputEvents(true);
    }

    internal void PlaceWithOffset(Vector3 position)
    {
        this.transform.position = position + _followOffset;
    }
    internal Vector2 ScreenToWorldPoint(Vector2 screenPosition)
    {
        Vector3 screenPosition3 = screenPosition;
        screenPosition3.z = -_followOffset.z;
        return _camera.ScreenToWorldPoint(screenPosition3);
    }

    private void UpdateRotation()
    {
        if (_nauseous)
        {
            StopShake();
            Vector3 rotationTarget = this.transform.eulerAngles;
            rotationTarget.z += (_nauseatedRotationAddition * _recentMoveDirection) * Time.deltaTime;
            this.transform.eulerAngles = rotationTarget;
        }
        else
        {
            Quaternion rotationTarget = Quaternion.Euler(0, 0, _targetAngle);
            this.transform.rotation = Quaternion.Lerp(this.transform.rotation, rotationTarget, _rotationCorrectionRate * Time.deltaTime);
        }
    }
    private void Rotate(float direction)
    {
        _targetAngle += 90 * direction;
        _recentMoveDirection = direction;
        AddNausea();
    }

    private void UpdatePosition()
    {
        if (_followTarget == null)
            return;
        Vector3 followOffset = this.transform.rotation * _followOffset;
        Vector3 targetPosition = _followTarget.transform.position + followOffset;
        this.transform.position = Vector3.Lerp(this.transform.position, targetPosition, _positionCorrectionRate * Time.deltaTime);
    }

    private void UpdateNausea()
    {
        if (_nauseaLevel == 0)
            return;

        _nauseaLevel = Mathf.Max(_nauseaLevel -= (_nauseaCoolRate * Time.deltaTime), 0);
        if (_nauseaLevel == 0)
        {
            _nauseous = false;
            _controller.AllowMovement = true;
        }
    }
    private void AddNausea()
    {
        _nauseaLevel += _nauseaAdditionPerRotation;
        if (_nauseaLevel >= _nauseaThreshold)
        {
            _nauseous = true;
            _controller.AllowMovement = false;
            _player.UIEventDelegate.Invoke(SandboxEvent.Nauseous);
        }
    }

    private void UpdateShake()
    {
        if (_nauseous)
            return;
        if (Time.time > _shakeKillTime)
            return;

        // Define scale
        float timeRemaining = _shakeKillTime - Time.time;
        float scale = _shakeDuration == 0 ? 0 : timeRemaining / _shakeDuration;
        float direction = 1;
        direction = direction.RandomDirection();
        float shake = (_shakeIntensity * scale) * direction;
        this.transform.eulerAngles += new Vector3(0, 0, shake);
    }
    internal void StartShake()
    {
        _shakeKillTime = Time.time + _shakeDuration;
    }
    internal void StopShake()
    {
        _shakeKillTime = 0;
    }

    #endregion
}
