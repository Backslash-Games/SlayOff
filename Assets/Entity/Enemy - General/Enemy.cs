using UnityEngine;
using UnityEngine.AI;

public class Enemy : EntityData
{

    private enum MovementType { Pathfinding, Flying_Tracking, Flying_Stationary };
    [Header("Movement")]
    [SerializeField] private MovementType movementType = MovementType.Pathfinding;

    [Header("Movement.Pathfinding")]
    [SerializeField] private NavMeshAgent agent;
    [Space]
    [SerializeField] private Transform eyes;
    [SerializeField] private float eye_TrackingSpeed = 1;
    [SerializeField] private bool eye_predictMovemet = false;
    [Space]
    [SerializeField] private bool useTargeting = false;
    [SerializeField] private Hitbox_Sphere targetingRange;
    [SerializeField] private bool targeting_DrawHitbox;

    [Header("Movement.Flying_Stationary")]
    [SerializeField] private float hover_height = 0;
    [SerializeField] private float hover_speed = 0;
    private float hover_init_height = 0;

    private enum AttackType { Melee, Ranged };
    [Header("Attack")]
    [SerializeField] private AttackType attackType = AttackType.Melee;
    [SerializeField] private float movementPredictionStrength = 1;

    [Header("Attack.Melee")]
    [SerializeField] private Hitbox_Sphere melee_AggressionRange;
    [SerializeField] private Weapon melee_Weapon;
    [SerializeField] private AnimationEvent melee_Tracking;
    [SerializeField] private bool melee_DrawHitbox;
    [SerializeField] private bool lock_eyes_OnAttack = true;

    [Header("Attack.Ranged")]
    [SerializeField] private GameObject ranged_Projectile;
    [SerializeField] private Cooldown ranged_Cooldown;
    private static string ranged_ProjectileParentID = "Projectiles";
    private static Transform ranged_ProjectileParent = null;
    [SerializeField] private Vector2 ranged_CooldownVariation;
    [Space]
    [SerializeField] private Animator ranged_animator;
    [SerializeField] private string ranged_animation_id;
    [Space]
    [SerializeField] private EffectLibrary<AttackType, AudioClip, EffectComponent_Audio.AudioParameters> attackAudioLibrary;

    [Header("Animations")]
    [SerializeField] private Animator anim_walking = null;
    [SerializeField] private string anim_walking_MultiplierID = "walk_speed";
    [SerializeField] private float anim_walking_Speed = 1;
    [Space]
    [SerializeField] private GameObject corpsePrefab = null;


    private PlayerController player = null;

    #region Unity Methods
    private void Start()
    {
        melee_Weapon.Setup(this, GetPlayer());
        melee_Weapon.SetDamage(GetStatblock().GetAttack());
        SetupRangedCooldown();
        SetupMovement();

        SetAgent(GetStatblock());
    }
    private void FixedUpdate()
    {
        TickMovement_FU();

        // Try to use attacks
        OnMeleeAttack();
        OnRangedAttack();
    }
    private void Update()
    {
        Tick_Animations();
    }
    private void LateUpdate()
    {
        TickMovement_LU();
        // Look at our target
        LookAtTarget(GetPlayer().gameObject, (Vector3.up * GetCameraOffset()) + GetMovementPrediction());
    }

    private void OnDrawGizmos()
    {
        if (targeting_DrawHitbox)
            targetingRange.DrawGizmos();

        if (melee_DrawHitbox)
        {
            melee_Weapon.GetHitbox().DrawGizmos();
            melee_AggressionRange.DrawGizmos();
        }
    }
    #endregion
    #region Dependencies
    /// <summary>
    ///     Gets the player from the scene
    /// </summary>
    /// <returns>Player Controller</returns>
    private PlayerController GetPlayer()
    {
        if (player == null)
            player = FindAnyObjectByType<PlayerController>();
        return player;
    }
    #endregion

    #region Movement
    private void SetupMovement()
    {
        switch (movementType)
        {
            case MovementType.Flying_Tracking:
                agent.enabled = false;
                GetRigidbody().useGravity = false;
                SetConstraints(RigidbodyConstraints.FreezeRotation);
                break;
            case MovementType.Flying_Stationary:
                agent.enabled = false;
                hover_init_height = transform.position.y;
                break;
            default:
                agent.enabled = true;
                break;
        }
    }

    private void TickMovement_FU()
    {
        switch (movementType)
        {
            case MovementType.Flying_Tracking:
                MovementFlying_Tracking();
                break;
            case MovementType.Flying_Stationary:
                // NONE
                break;
            default:
                // Set the agent destination
                SetAgentDestination(GetPlayer().GetGroundPosition());
                break;
        }
    }
    private void TickMovement_LU()
    {
        switch (movementType)
        {
            case MovementType.Flying_Tracking:
                // NONE
                break;
            case MovementType.Flying_Stationary:
                MovementFlying_Stationary();
                break;
            default:
                // Set the agent destination
                SetAgentDestination(GetPlayer().GetGroundPosition());
                break;
        }
    }
    #endregion

    #region Flying
    private void MovementFlying_Stationary()
    {
        transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, hover_init_height, transform.position.z) + (Vector3.up * hover_height), Time.deltaTime * hover_speed);
    }

    private void MovementFlying_Tracking()
    {
        ApplyForce(GetPlayer().transform.position - transform.position, GetStatblock().GetSpeed(), ForceMode.Acceleration, "Enemy.Flying_Tracking");
    }
    #endregion
    #region AI
    /// <summary>
    ///     Sets the agent attributes based on statblock
    /// </summary>
    /// <param name="statblock">Input block</param>
    private void SetAgent(Statblock statblock)
    {
        // Check if agent is set
        if (agent == null)
            return;

        // Set agent speed
        agent.speed = statblock.GetSpeed();
    }

    /// <summary>
    ///     Sets the agent target
    /// </summary>
    /// <param name="target">Input target</param>
    private void SetAgentDestination(Vector3 target) 
    {
        // Check if agent is set
        if (agent == null)
            return;
        // Check if the target is in the hitbox
        if (useTargeting)
        {
            targetingRange.Tick();
            if (!targetingRange.GetState())
                return;
        }
        // Sets the agent destination
        agent.SetDestination(target); 
    }
    
    /// <summary>
    ///     Rotates eyes to look at the target
    /// </summary>
    /// <param name="target">Input Target</param>
    private void LookAtTarget(GameObject target, Vector3 offset)
    {
        // Check if eyes are set
        if (eyes == null)
            return;
        // Make sure we are not using our melee attack
        if (lock_eyes_OnAttack && !melee_Tracking.GetState())
            return;

        // Mark down our target position
        Vector3 tPosition = target.transform.position + offset;
        Quaternion lRotation = Quaternion.LookRotation(tPosition - transform.position);

        // Look at target
        eyes.transform.rotation = Quaternion.Lerp(eyes.transform.rotation, lRotation, Time.deltaTime * eye_TrackingSpeed);
    }

    /// <summary>
    ///     Gets the offset for movement prediction attacks
    /// </summary>
    /// <returns>World Position appliable as offset</returns>
    private Vector3 GetMovementPrediction()
    {
        // Check if we are predicting movemet
        if (!eye_predictMovemet)
            return Vector3.zero;

        // Hold a reference to the player
        PlayerController cPlayer = GetPlayer();
        // Start with the players linear velocity
        Vector3 movementPrediction = cPlayer.GetLinearVelocity();

        // Adjust scale based on distance
        float distanceModifier = Vector3.Distance(transform.position, cPlayer.transform.position) / targetingRange.GetRadius();
        movementPrediction *= distanceModifier * movementPredictionStrength;

        return movementPrediction;
    }

    /// <summary>
    ///     Gets the range between the player center and the camera
    /// </summary>
    /// <returns>Camera offset</returns>
    private float GetCameraOffset()
    {
        return Camera.main.transform.position.y - GetPlayer().transform.position.y;
    }
    #endregion
    #region Melee Attack
    /// <summary>
    ///     Checks and runs melee attack
    /// </summary>
    private void OnMeleeAttack()
    {
        // Checks if the melee attack is allowed
        if (!attackType.Equals(AttackType.Melee))
            return;

        // Ticks the melee weapon
        melee_AggressionRange.Tick();
        melee_Weapon.Tick(melee_AggressionRange.GetState());
    }
    #endregion
    #region Ranged Attack
    /// <summary>
    ///     Sets up information required by ranged_Cooldown
    /// </summary>
    private void SetupRangedCooldown()
    {
        ranged_Cooldown = new Cooldown(this, GetStatblock().GetFireRate(), 1);
    }

    /// <summary>
    ///     Runs information for a enemy ranged attack
    /// </summary>
    private void OnRangedAttack()
    {
        // Checks if the ranged attack is allowed
        if (!attackType.Equals(AttackType.Ranged))
            return;
        // Checks if the player is in targeting range
        /*if (!targetingRange.CheckCollision())
            return;*/
        // Check if the ranged cooldown is active
        if (ranged_Cooldown.Active())
            return;

        // Shoot a projectile
        ShootProjectile();
        // Restart cooldown
        ranged_Cooldown.Start(Random.Range(ranged_CooldownVariation.x, ranged_CooldownVariation.y));
        // Play audio
        EffectManager.Instance.Play(attackAudioLibrary, AttackType.Ranged);

        if(ranged_animator != null)
            ranged_animator.Play(ranged_animation_id);
    }
    /// <summary>
    ///     Shoot a projectile
    /// </summary>
    private void ShootProjectile()
    {
        GameObject spawned = Instantiate(ranged_Projectile);
        spawned.transform.position = eyes.transform.position;
        spawned.transform.forward = eyes.transform.forward;

        spawned.transform.parent = GetProjectileParent();
    }

    private Transform GetProjectileParent()
    {
        // Return parent if not null
        if (ranged_ProjectileParent != null)
            return ranged_ProjectileParent;
        // Create object if all else fails
        ranged_ProjectileParent = (new GameObject(ranged_ProjectileParentID)).transform;
        return ranged_ProjectileParent;
    }
    #endregion

    #region Animations
    private void Tick_Animations()
    {
        SetWalkingMultiplier();
    }

    private void SetWalkingMultiplier()
    {
        if (anim_walking == null)
            return;
        
        // Set anim walking variable
        anim_walking.SetFloat(anim_walking_MultiplierID, agent.velocity.magnitude * anim_walking_Speed);
    }
    #endregion
    #region Death Handling
    public override void Death(bool play_audio = true)
    {
        SpawnCorpse();
        base.Death(play_audio);
    }
    private void SpawnCorpse()
    {
        if (corpsePrefab == null)
            return;

        Corpse corpse = Instantiate(corpsePrefab, transform.position, transform.rotation).GetComponent<Corpse>();
        corpse.SetEyes(eyes);
    }
    #endregion

    #region String Handling
    public override string ToString()
    {
        string output = $"{base.ToString()}\n";

        output += $"Melee Attack\n";
        output += $". > Draw: {melee_DrawHitbox}\n";
        output += $". > Weapon: {melee_Weapon}\n";

        return output;
    }
    #endregion
}
