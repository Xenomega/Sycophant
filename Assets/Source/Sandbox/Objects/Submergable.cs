using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]

sealed internal class Submergable : MonoBehaviour
{
    #region Values
    private bool _submerged;
    internal bool Submerged
    {
        get { return _submerged; }
    }

    [Range(0, 1)]
    [SerializeField] private float _entryVelocityScale;
    [SerializeField] private float _entryGravityScale;
    [SerializeField] private float _entryDrag;
    [SerializeField] private float _entryAngularDrag;
    [SerializeField] private float _exitGravityScale;
    [SerializeField] private float _exitDrag;
    [SerializeField] private float _exitAngularDrag;


    [Header("NOTE: Entry effect is instantiated upon entry.")]
    [SerializeField]
    private GameObject _entryEffect;
    [SerializeField] private float _entryVelocityDisplacementDistance;
    [Header("NOTE: Entry effect is instantiated upon exit.")]
    [SerializeField]
    private GameObject _exitEffect;
    [SerializeField] private float _exitVelocityDisplacementDistance;
    [SerializeField] private float _velocityMagnitudeMinimum;

    [Space(10)]
    [SerializeField]
    private float _nextEffectDelay;
    private float _nextEffectTime;

    [Header("NOTE: Stay effect activates a user instantated game object upon entry, and deactivates upon exit.")]
    [SerializeField]
    private ParticleSystem _stayEffect;

    private BoxCollider2D _boxCollider2D;
    private Rigidbody2D _rigidbody2D;
    ParticleSystem.EmissionModule _stayEffectEmissionModule;
    #endregion

    #region Unity Functions
    private void Awake()
    {
        Initialize();
    }
    #endregion

    #region Functions
    private void Initialize()
    {
        _boxCollider2D = this.GetComponent<BoxCollider2D>();
        _rigidbody2D = this.GetComponent<Rigidbody2D>();
        if (_stayEffect != null)
            _stayEffectEmissionModule = _stayEffect.emission;
    }

    internal void Submerge(bool state)
    {
        _submerged = state;
        ProcessEffects(state);
    }

    private void ProcessEffects(bool state)
    {

        if (_rigidbody2D != null)
        {
            if (state)
                _rigidbody2D.velocity *= _entryVelocityScale;
            _rigidbody2D.drag = state ? _entryDrag : _exitDrag;
            _rigidbody2D.angularDrag = state ? _entryAngularDrag : _exitAngularDrag;
            _rigidbody2D.gravityScale = state ? _entryGravityScale : _exitGravityScale;
        }

        if (_stayEffect != null)
        {
            _stayEffectEmissionModule.enabled = state;
            if (!state)
                _stayEffect.SetParticles(new ParticleSystem.Particle[0], 0);
        }

        if (Time.time < _nextEffectTime)
            return;
        _nextEffectTime = Time.time + _nextEffectDelay;

        Vector3 velocityDisplacement = _rigidbody2D != null ? (Vector3)_rigidbody2D.velocity.normalized : Vector3.zero;
        if (_rigidbody2D != null && _rigidbody2D.velocity.magnitude <= _velocityMagnitudeMinimum)
            return;

        velocityDisplacement *= state ? _entryVelocityDisplacementDistance : _exitVelocityDisplacementDistance;
        CreateEffect(state ? _entryEffect : _exitEffect, this.transform.position + velocityDisplacement);
    }

    private void CreateEffect(GameObject gameObject, Vector3 position)
    {
        if (gameObject == null)
            return;
        GameObject newEffectGameObject = Instantiate(gameObject, position, this.transform.rotation, Globals.singleton.containers.effects);
    } 
    #endregion
}
