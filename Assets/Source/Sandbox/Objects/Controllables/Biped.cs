using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles all biped/character characteristics for a presumed biped object, including movement, death, positional/rotational constraints, effects, etc.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CollisionList))]
sealed internal class Biped : Controllable
{
    #region Values
    // Inspector
    [Header("Powerups")]
    [Tooltip("The scale by which to change our biped once a normous powerup has been activated.")]
    [SerializeField] private float _normousScale;
    [Tooltip("The amount of speed to add to our biped's movement once a nitro powerup has been activated.")]
    [SerializeField] private float _nitroMovementSpeedAddition;

    [Header("Grounding")]
    [Tooltip("Indicates the maximum angle which the biped will still be grounded on.")]
    [SerializeField] private float _maxGroundAngle;

    [Header("Horizontal Movement")]
    [Tooltip("Indicates the speed at which movement occurs when the movement input is maximized. (Grounded)")]
    [SerializeField] private float _movementSpeed;
    [Tooltip("Indicates the speed at which movement occurs when the movement input is maximized. (Airborne)")]
    [SerializeField] private float _airborneMovementSpeed;
    [Tooltip("Indicates the default movement speed scale.")]
    [SerializeField] private float _defaultMovementScale;
    [Tooltip("Indicates the movement speed scale when the biped is submerged.")]
    [SerializeField] private float _submergedMovementScale;

    [Header("Vertical Movement")]
    [Tooltip("Indicates the initial upward velocity upon biped spawn.")]
    [SerializeField] private float _spawnUpwardVelocity;
    [Tooltip("TODO: Gravity at this point indicates both half of decay per second for jump/bounce velocity, and the maximum gravitational influenced speed when no other forces are applied.")]
    [SerializeField] private float _gravity;
    [Tooltip("Indicates a range to randomly select jump force between when the biped attempts to jump.")]
    [SerializeField] private Vector2 _jumpForceRange;

    // Runtime (Components)
    private string _name;
    private GameObject _effect;
    private CollisionList _collisionList;
    private BoxCollider2D _boxCollider2D;
    private Rigidbody2D _rigidbody2D;
    private SpriteRenderer _spriteRenderer;
    private Submergable _submergable;

    // Runtime (States)
    private bool _grounded;
    private bool _dead;
    private Vector2 _unappliedVelocity;
    private ulong _groundingCheckedTime;
    private bool _useNitroSpeed;
    private float _jumpVelocity;
    private float _bounceVelocity;

    // Properties
    /// <summary>
    /// Indicates the maximum movement speed in the biped's current state.
    /// </summary>
    private float CurrentMovementSpeed
    {
        get
        {
            float movementSpeed = _grounded ? _movementSpeed : _airborneMovementSpeed;
            return _useNitroSpeed ? movementSpeed + _nitroMovementSpeedAddition : movementSpeed;
        }
    }
    /// <summary>
    /// Indicates the movement scale in the biped's current state.
    /// </summary>
    private float CurrentMovementScale
    {
        get { return Submerged ? _submergedMovementScale : _defaultMovementScale; }
    }
    /// <summary>
    /// Indicates whether the biped has died or not.
    /// </summary>
    internal bool Dead
    {
        get { return _dead; }
    }
    /// <summary>
    /// Indicates the velocity applied this cycle, with respect to biped rotation.
    /// </summary>
    private Vector2 RelativeVelocity
    {
        get
        {
            float vertical = Vector2.Dot(this.transform.up, _unappliedVelocity);
            float horizontal = Vector2.Dot(this.transform.right, _unappliedVelocity);
            return new Vector2(horizontal, vertical);
        }
        set
        {
            _unappliedVelocity = this.transform.rotation * value;
            _rigidbody2D.velocity = _unappliedVelocity;
        }
    }
    /// <summary>
    /// Indicates whether the model/renderer is visible.
    /// </summary>
    internal bool RendererVisible
    {
        get { return _spriteRenderer.isVisible; }
    }
    /// <summary>
    /// Indicates whether the biped is currently submerged.
    /// </summary>
    private bool Submerged
    {
        get { return _submergable.Submerged; }
    }
    #endregion

    #region Unity Functions
    private void Awake()
    {
        Initialize();
    }

    private void OnEnable()
    {
        ApplySpawnEffects();
    }
    private void OnDisable()
    {
        ApplyDespawnEffect();
    }

    private void Update()
    {
        UpdateVerticalVelocity();
    }
    private void LateUpdate()
    {
        UpdateGroupingVelocity();
    }

    private void FixedUpdate()
    {
        UpdateGrounding();   
    }
    private void OnDrawGizmos()
    {
        // Draw a box around this biped, color dependent on it's grounded status.
        Gizmos.color = _grounded ? Color.green : Color.blue;
        Gizmos.DrawWireCube(_spriteRenderer.bounds.center, _spriteRenderer.bounds.extents);
    }
    #endregion

    #region Functions
    // Initialization / Setup
    /// <summary>
    /// Initializes the biped object, obtians all relevant components.
    /// </summary>
    private void Initialize()
    {
        // Set our layer, obtain appropriate components.
        this.gameObject.layer = Globals.BIPED_LAYER;
        _submergable = this.GetComponent<Submergable>();
        _rigidbody2D = this.GetComponent<Rigidbody2D>();
        _boxCollider2D = this.GetComponent<BoxCollider2D>();
        _spriteRenderer = this.GetComponent<SpriteRenderer>();
        _collisionList = this.GetComponent<CollisionList>();
    }
    /// <summary>
    /// Sets the controllable aspects of this object given a player and a corresponding controller.
    /// </summary>
    /// <param name="player">The player to associate to this biped.</param>
    /// <param name="controller">The controller to associate to this biped.</param>
    internal override void SetAspects(Player player, Controller controller)
    {
        // Set our controllable aspects, enable input events.
        base.SetAspects(player, controller);
        SetInputEvents(true);
    }
    /// <summary>
    /// Sets the design of this biped given a biped design descriptor.
    /// </summary>
    /// <param name="bipedDesign">The biped design descriptor used to indicate the biped design to derive.</param>
    internal void SetDesign(BipedDesign bipedDesign)
    {
        // Set our design aspect values accordingly.
        _name = bipedDesign.Name;
        _spriteRenderer.sprite = InvestmentManager.singleton.CostumeSprites[bipedDesign.SpriteIndex];
        _effect = InvestmentManager.singleton.Effects[bipedDesign.EffectIndex];
        ApplySpawnEffects();
    }
    /// <summary>
    /// Sets or unsets input events for the controller associated to this biped, given a state.
    /// </summary>
    /// <param name="state">Adds event handlers to the associated controller events if true, otherwise removes them.</param>
    /// <returns></returns>
    internal override bool SetInputEvents(bool state)
    {
        // Depending on the state, remove or add events appropriately.
        if (base.SetInputEvents(state))
        {
            if (state)
            {
                _controller.OnMove += Move;
                _controller.OnJump += Jump;
                _controller.OnRotate += Rotate;
            }
            else
            {
                _controller.OnMove -= Move;
                _controller.OnJump -= Jump;
                _controller.OnRotate -= Rotate;
            }
        }
        return true;
    }


    // Movement / Movement States
    /// <summary>
    /// Constrains horizontal user movement if the state is set.
    /// </summary>
    /// <param name="state">If set, constrains horizontal user movement.</param>
    private void ConstrainHorizontalVelocity(bool state)
    {
        // Depending on our constraining state, set the appropriate movement constraint flags.
        if (state)
            _rigidbody2D.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        else
            _rigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation;
    }
    /// <summary>
    /// Moves the biped horizontally given direction input.
    /// </summary>
    /// <param name="direction">Indicates the direction scale to apply movement in. Ranges from [-1, 1], where negative values imply leftward movement, and positive imply rightward movement.</param>
    private void Move(float direction)
    {
        // If we are applying horizontal force, we'll want to calculate the scale accordingly to flip the biped.
        if (direction != 0)
        {
            // Determine the direction, and scale.
            bool flip = direction > 0 ? false : true;
            Vector3 scale = this.transform.localScale;

            // Flip our x-component negative if we are to, otherwise ensure it is positive.
            if (flip && scale.x > 0)
                scale.x *= -1;
            else if (!flip)
                scale.x = Mathf.Abs(scale.x);

            // Set the calculated scale accordingly.
            this.transform.localScale = scale;
        }

        // Apply directional movement velocity.
        Vector2 relativeVelocity = RelativeVelocity;
        relativeVelocity.x = (CurrentMovementSpeed * CurrentMovementScale) * direction;
        RelativeVelocity = relativeVelocity;
    }
    /// <summary>
    /// Initiates a jump by the biped if the current state allows for it to do so.
    /// </summary>
    private void Jump()
    {
        // If we're airborne and not in water, we can't jump.
        if (!_grounded && !Submerged)
            return;

        // Obtain a random jump force.
        float jumpForce = UnityEngine.Random.Range(_jumpForceRange.x, _jumpForceRange.y);

        // If our bounce velocity exceeds our jump velocity, do not apply any jump velocity.
        if (_bounceVelocity > jumpForce)
            return;

        // Otherwise set our jump velocity accordingly.
        _jumpVelocity = jumpForce + _gravity;
    }
    /// <summary>
    /// Rotates the biped, given a direction scale which is multiplied by 90 degrees.
    /// </summary>
    /// <param name="direction">The direction scale to rotate, which is multiplied by 90 degrees. This can be negative to rotate counter clockwise.</param>
    private void Rotate(float direction)
    {
        // If we are to rotate, rotate by the direction, as a multiple of 90 degrees.
        this.transform.Rotate(transform.forward, 90 * direction);
    }
    /// <summary>
    /// Updates vertical velocity with respect to jump/bounce velocity and gravitational force.
    /// </summary>
    private void UpdateVerticalVelocity()
    {
        // If we're grounded and have no velocity to process, we don't need gravity or any other vertical force so we can stop.
        if (_grounded && _jumpVelocity == 0 && _bounceVelocity == 0)
            return;

        // If we have applicable jump velocity..
        if (_jumpVelocity > 0)
        {
            // Take away from it with time.
            _jumpVelocity -= Time.deltaTime * (_gravity * 2);

            // Ensure it does not drop below 0.
            _jumpVelocity = Mathf.Max(0, _jumpVelocity);
        }

        // If we have applicable bounce velocity..
        if (_bounceVelocity > 0)
        {
            // Take away from it with time.
            _bounceVelocity -= Time.deltaTime * (_gravity * 2);

            // Ensure it does not drop below 0.
            _bounceVelocity = Mathf.Max(0, _bounceVelocity);
        }

        // Set our vertical component of our velocity accordingly.
        Vector2 relativeVelocity = RelativeVelocity;
        relativeVelocity.y = (_jumpVelocity - _gravity) + _bounceVelocity;
        RelativeVelocity = relativeVelocity * CurrentMovementScale;
    }
    /// <summary>
    /// Updates grounded status for this biped and any biped it may rely on (if this biped rests on another)
    /// </summary>
    internal void UpdateGrounding()
    {
        // If the biped is dead, stop.
        if (Dead)
            return;

        // Check that we haven't updated in this cycle already and update state and continue if we haven't.
        if (_groundingCheckedTime == Globals.singleton.PhysicsUpdateCount)
            return;
        else
            _groundingCheckedTime = Globals.singleton.PhysicsUpdateCount;

        // Check all of our collisions to verify we are grounded.
        bool grounded = false;
        bool adoptedGrounded = false;
        bool adoptedGroundedValue = false;
        Collision2D[] collisions = new Collision2D[_collisionList.Collisions2D.Values.Count];
        _collisionList.Collisions2D.Values.CopyTo(collisions, 0);
        foreach (Collision2D collision2D in collisions)
        {
            for (int i = 0; i < collision2D.contacts.Length; i++)
            {
                float angle = Vector3.Angle(this.transform.up, collision2D.contacts[i].normal);
                float yContactOffset = Vector2.Dot(this.transform.up, (collision2D.contacts[i].point - (Vector2)this.transform.position));
                bool groundedAngle = angle <= _maxGroundAngle && yContactOffset < 0;

                // Check the type of collision.
                if (collision2D.gameObject.layer == Globals.MACHINE_LAYER)
                {
                    // If it's a machine, obtain the surface component.
                    Surface surface = collision2D.gameObject.GetComponent<Surface>();
                    if (surface != null)
                    {
                        // Kill the biped if this surface is a kill impactor, and stop since we're done processing.
                        if (surface.KillImpactor)
                        {
                            Die();
                            return;
                        }

                        // Apply bounce velocity if this surface bounces and we're grounded on this angle.
                        // TODO: Revisit this to allow for instantaneous velocity bounce.
                        if (groundedAngle && surface.HasBounce)
                            _bounceVelocity = surface.Bounciness;
                    }

                    // If this is an appropriate angle, the player should be grounded.
                    grounded |= groundedAngle;
                }
                else if (collision2D.gameObject.layer == Globals.BIPED_LAYER)
                {
                    // If it's a biped, obtain the Biped component.
                    Biped biped = collision2D.gameObject.GetComponent<Biped>();

                    if (groundedAngle && biped._bounceVelocity > _bounceVelocity)
                        _bounceVelocity = biped._bounceVelocity;
                    if (groundedAngle && biped._jumpVelocity > _jumpVelocity)
                        _jumpVelocity = biped._jumpVelocity;

                    // Check the biped is ontop.
                    if (groundedAngle)
                    {
                        // If it is, we update our lower ninjas state and determine our state based off of this.
                        biped.UpdateGrounding();

                        // If the lower ninja isn't grounded and we're on it, we adopt it's state.
                        adoptedGroundedValue = biped._grounded;
                        adoptedGrounded = true;
                    }
                }
                else
                {
                    // If this is an appropriate angle, the player should be grounded.
                    grounded |= groundedAngle;
                }
            }
        }

        // If we adopted grounded status from a biped, set it accordingly.
        if (adoptedGrounded)
            grounded = adoptedGroundedValue;

        // If we're grounded and have no jump or bounce velocity, set our velocity as 0.
        if (grounded && _jumpVelocity == 0 && _bounceVelocity == 0)
        {
            Vector2 relativeVelocity = RelativeVelocity;
            relativeVelocity.y = 0;
            RelativeVelocity = relativeVelocity;
        }

        // Update our grounded state
        _grounded = grounded;
    }
    /// <summary>
    /// Applies velocity to the biped which groups it towards other bipeds, to avoid wandering.
    /// </summary>
    private void UpdateGroupingVelocity()
    {
        // If we're grounded, we're not going to add grouping force.
        if (_grounded)
            return;

        // Calculate our delta vector for centroid.
        Vector2 delta = (_player.BipedsCentroid - (Vector2)transform.position);

        // Verify we are past our distance threshold.
        float distanceSqrd = delta.sqrMagnitude;
        if (distanceSqrd >= Globals.singleton.bipeds.groupingFarThresholdSqrd)
        {
            // Calculate the current speed in the direction, and the amount of grouping force to add, then set the velocity.
            float currentSpeedInDirection = Vector2.Dot(delta.normalized, _unappliedVelocity);
            float groupingForce = Mathf.Abs(currentSpeedInDirection) * Globals.singleton.bipeds.groupingBoostFraction;
            _rigidbody2D.velocity = _unappliedVelocity + (delta.normalized * groupingForce);
        }
    }

    // Respawn / Death / Effects
    /// <summary>
    /// Creates an effect game object at the given position
    /// </summary>
    /// <param name="gameObject">The game object which represents the effect to instantiate.</param>
    /// <param name="position">The position to instantiate the effect object at.</param>
    private void CreateEffect(GameObject gameObject, Vector3 position)
    {
        // If the prefab is null, stop.
        if (gameObject == null)
            return;

        // Instantiate and set the position/orientation.
        GameObject newEffectGameObject = Instantiate(gameObject, position, this.transform.rotation, Globals.singleton.containers.effects);
        newEffectGameObject.transform.localScale = this.transform.localScale;
    }
    /// <summary>
    /// Applies/activates all spawn effects
    /// </summary>
    internal void ApplySpawnEffects()
    {
        // Set our upward spawn velocity, and create the appropriate spawn effect.
        _jumpVelocity = _spawnUpwardVelocity;
        CreateEffect(_effect, this.transform.position);
    }
    /// <summary>
    /// Applies/activates all despawn effects
    /// </summary>
    internal void ApplyDespawnEffect()
    {
        // Create the appropriate despawn effect at this biped's position.
        CreateEffect(_effect, this.transform.position);
    }
    /// <summary>
    /// Respawns a dead biped at the given position and rotation.
    /// </summary>
    /// <param name="position">The position to respawn the biped at.</param>
    /// <param name="rotation">The rotation to respawn the biped with.</param>
    internal void Respawn(Vector3 position, Quaternion rotation)
    {
        // Set our dead state accordingly, enable input events.
        _dead = false;
        SetInputEvents(true);

        // Enable this game object, set it's position and rotation accordingly.
        this.gameObject.SetActive(true);
        this.transform.position = position;
        this.transform.rotation = rotation;
    }
    /// <summary>
    /// Sets the biped into a dead state, disabling input, marking it inactive, signalling it has died.
    /// </summary>
    internal void Die()
    {
        // If we're already dead, stop.
        if (Dead)
            return;

        // Set our dead state, disable input events
        _dead = true;
        SetInputEvents(false);

        // Disable this game object, and mark our biped as dead.
        this.gameObject.SetActive(false);
        _player.BipedDied(this);
    }

    // Powerup Functions
    internal void Powerup(PickupType powerupType)
    {
        // Determine which powerup to apply.
        switch (powerupType)
        {
            case PickupType.Nitro:
                PowerupNitro();
                break;
            case PickupType.Normous:
                PowerupNormous();
                break;
            case PickupType.Normal:
                PowerupNormal();
                break;
            default:
                break;
        }
    }
    /// <summary>
    /// Applies the normous powerup to the biped, setting biped size scale.
    /// </summary>
    private void PowerupNormous()
    {
        // If our scale isn't 1, we can stop already since we're already enlarged.
        if (Mathf.Abs(this.transform.localScale.x) != 1)
            return;
        
        // Otherwise apply our normous scale.
        this.transform.localScale *= _normousScale;
    }
    /// <summary>
    /// Applies the nitro powerup to the biped.
    /// </summary>
    private void PowerupNitro()
    {
        // Enable nitro speed.
        _useNitroSpeed = true;
    }
    /// <summary>
    /// Reverts powerup states of the biped back to normal.
    /// </summary>
    private void PowerupNormal()
    {
        // Determine our x component on scale based off direction (we want to restore to magnitude of 1).
        float xScale = this.transform.localScale.x > 0 ? 1 : -1;

        // Set the scale accordingly.
        this.transform.localScale = new Vector3(xScale, 1, 1);

        // Disable nitro speed.
        _useNitroSpeed = false;
    }

    #endregion
}
