using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]

sealed internal class Machine : MonoBehaviour
{
    #region Values
    private bool _allowMove;
    private float _nextMoveTime;


    private bool _pushing;
    private Vector3 _startingPosition;
    private Vector3 _pushedPosition;
    [Header("Pushing")]
    [SerializeField] private float _pushDistance;
    [SerializeField] private float _pushStartDelay;
    [SerializeField] private float _pushMovementSpeed;
    [SerializeField] private GameObject _fullyPushedEffect;
    [SerializeField] private float _fullyPushedEffectDistanceAdditve;
    [SerializeField] private AudioSource _pushSound;
    [SerializeField] private AudioSource _fullyPushedSound;
    private const float MOVEMENT_DIRECTION_CHANGE_SQUARE_MAGNITUDE = 0.1f;

    [Header("Pulling")]
    [SerializeField] private float pullStartDelay;
    [SerializeField] private float _pullMovementSpeed;
    [SerializeField] private AudioSource _pullSound;
    [SerializeField] private AudioSource _fullyPulledSound;

    [Header("Rotation")]
    [SerializeField] private float _rotationSpeed;

    private SpriteRenderer _spriteRenderer;
    #endregion

    #region Unity Functions
    private void Start()
    {
        Initialize();
    }

    private void Update()
    {
        UpdateMovement();
        UpdateRotation();
    }

    private void OnDrawGizmos()
    {
        Vector3 gizmoPosition = _startingPosition == Vector3.zero ? this.transform.position : _startingPosition;
        float gizmoDistance = _pushDistance + _fullyPushedEffectDistanceAdditve;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(gizmoPosition, gizmoPosition + (-this.transform.up * gizmoDistance));
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(gizmoPosition + (-this.transform.up * _pushDistance), Vector3.one);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(gizmoPosition + (-this.transform.up * gizmoDistance), Vector3.one);
    }
    #endregion

    #region Functions
    private void Initialize()
    {
        this.gameObject.layer = Globals.MACHINE_LAYER;
        _spriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();

        _startingPosition = this.transform.position;
        _pushedPosition = this.transform.position + (-this.transform.up * _pushDistance);
    }

    private void UpdateMovement()
    {
        if (_pullMovementSpeed == 0 && _pushMovementSpeed == 0)
            return;

        if (!_spriteRenderer.isVisible)
            return;

        if (Time.time > _nextMoveTime)
        {
            if (!_allowMove)
            {
                if(!_pushing)
                    PlayAudioSource(_pushSound);
                else
                    PlayAudioSource(_pullSound);
            }
            _allowMove = true;
        }

        if (!_allowMove)
            return;

        Vector3 target = _pushing ? _startingPosition : _pushedPosition;
        float speed = _pushing ? _pushMovementSpeed : _pullMovementSpeed;
        this.transform.position = Vector3.MoveTowards(this.transform.position, target, speed * Time.deltaTime);
        float positionTargetSquarMagnitude = (this.transform.position - target).sqrMagnitude;

        if (positionTargetSquarMagnitude < MOVEMENT_DIRECTION_CHANGE_SQUARE_MAGNITUDE)
        {
            if (!_pushing)
            {
                Vector3 fullyPushedPosition = _startingPosition + (-this.transform.up * (_pushDistance + _fullyPushedEffectDistanceAdditve));
                CreateEffect(_fullyPushedEffect, fullyPushedPosition);
                PlayAudioSource(_fullyPushedSound);
            }
            else
            {
                PlayAudioSource(_fullyPulledSound);
            }
            _pushing = !_pushing;
            _allowMove = false;
            _nextMoveTime = Time.time + (_pushing ? pullStartDelay : _pushStartDelay);
        }
    }

    private void UpdateRotation()
    {
        if (!_spriteRenderer.isVisible)
            return;

        this.transform.Rotate(this.transform.forward, _rotationSpeed * Time.deltaTime);
    }

    private void PlayAudioSource(AudioSource audioSource)
    {
        if(audioSource != null)
            audioSource.Play();
    }

    private void CreateEffect(GameObject gameObject, Vector3 position)
    {
        if (gameObject == null)
            return;
        GameObject newEffectGameObject = Instantiate(gameObject, position, this.transform.rotation, Globals.singleton.containers.effects);
    }
    #endregion
}
