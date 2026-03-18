using UnityEngine;
using UnityEngine.AI;

public class Enemy : EntityData
{

    [Header("Pathfinding")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform eyes;
    [SerializeField] private Hitbox_Sphere targetingRange;
    [SerializeField] private bool targeting_DrawHitbox;

    private enum AttackType { Melee, Ranged };
    [Header("Attacking General")]
    [SerializeField] private AttackType attackType = AttackType.Melee;
    [SerializeField] private float movementPredictionStrength = 1;

    [Header("Melee Weapon")]
    [SerializeField] private Hitbox_Sphere melee_AggressionRange;
    [SerializeField] private Weapon melee_Weapon;
    [SerializeField] private AnimationEvent melee_Tracking;
    [SerializeField] private bool melee_DrawHitbox;

    [Header("Ranged Weapon")]
    [SerializeField] private GameObject ranged_Projectile;
    [SerializeField] private Cooldown ranged_Cooldown;
    private static string ranged_ProjectileParentID = "Projectiles";
    private static Transform ranged_ProjectileParent = null;
    [SerializeField] private Vector2 ranged_CooldownVariation;

    private PlayerController player = null;

    #region Unity Methods
    private void Start()
    {
        melee_Weapon.Setup(this, GetPlayer());
        melee_Weapon.SetDamage(GetStatblock().GetAttack());
        SetupRangedCooldown();

        SetAgent(GetStatblock());
    }
    private void FixedUpdate()
    {
        // Set the agent destination
        SetAgentDestination(GetPlayer().GetGroundPosition());

        // Look at our target
        LookAtTarget(GetPlayer().gameObject, (Vector3.up * GetCameraOffset()) + GetMovementPrediction());
        
        // Try to use attacks
        OnMeleeAttack();
        OnRangedAttack();
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
        targetingRange.Tick();
        if (!targetingRange.GetState())
            return;
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
        if (!melee_Tracking.GetState())
            return;

        // Look at target
        eyes.transform.LookAt(target.transform.position + offset);
    }

    /// <summary>
    ///     Gets the offset for movement prediction attacks
    /// </summary>
    /// <returns>World Position appliable as offset</returns>
    private Vector3 GetMovementPrediction()
    {
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
        if (!targetingRange.GetState())
            return;
        // Check if the ranged cooldown is active
        if (ranged_Cooldown.Active())
            return;

        // Shoot a projectile
        ShootProjectile();
        // Restart cooldown
        ranged_Cooldown.Start(Random.Range(ranged_CooldownVariation.x, ranged_CooldownVariation.y));
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
