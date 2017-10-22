using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
sealed internal class Projectile : MonoBehaviour
{
    #region Values
    private bool _stuck;

    private enum VelocityType
    {
        Constant,
        Initial
    }
    [Header("Velocity")]
    [SerializeField] private VelocityType _velocityType;

    [SerializeField] private float _velocity;

    private enum ImpactAction
    {
        Stick,
        Destroy
    }
    [Header("Impact")]
    [SerializeField] private GameObject _impactEffect;
    [SerializeField] private ImpactAction _impactAction;
    [SerializeField] private Vector2 _impactStickRotationRange;

    private Rigidbody2D _rigidbody2D;
    #endregion

    #region Unity Functions
    private void Awake()
    {
        Initialize();
    }

    private void Update()
    {
        if (_velocityType == VelocityType.Constant)
            ApplyVelocity();
    }

    private void OnCollisionEnter2D(Collision2D collision2D)
    {
        Impact(collision2D);
    } 
    #endregion

    #region Functions
    private void Initialize()
    {
        this.gameObject.layer = Globals.PROJECTILE_LAYER;
        _rigidbody2D = this.GetComponent<Rigidbody2D>();
        ApplyVelocity();
    }

    private void ApplyVelocity()
    {
        _rigidbody2D.velocity = this.transform.right * _velocity;
    }

    private void Impact(Collision2D collision2D)
    {
        if (_stuck)
            return;

        Vector3 impactPoint = collision2D.contacts[0].point;
        CreateEffect(_impactEffect, impactPoint);

        bool allowStick = TryAffectBiped(collision2D);

        if (_impactAction == ImpactAction.Stick)
        {
            if (allowStick)
            {
                _stuck = true;
                _rigidbody2D.constraints = RigidbodyConstraints2D.FreezeAll;
                _rigidbody2D.simulated = false;
                this.transform.RotateAround(collision2D.contacts[0].point, Vector3.forward, Random.Range(_impactStickRotationRange.x, _impactStickRotationRange.y));
            }
        }
        else if (_impactAction == ImpactAction.Destroy)
        {
            Destroy(this.gameObject);
        }
    }

    private bool TryAffectBiped(Collision2D collision2D)
    {
        if (collision2D.gameObject.layer == Globals.BIPED_LAYER)
        {
            Biped biped = collision2D.gameObject.GetComponent<Biped>();
            biped.Die();
            return false;
        }
        return true;
    }

    private void CreateEffect(GameObject gameObject, Vector3 position)
    {
        if (gameObject == null)
            return;
        GameObject newImpactEffectGameObject = Instantiate(_impactEffect, position, Quaternion.identity, Globals.singleton.containers.effects);

    } 
    #endregion
}
