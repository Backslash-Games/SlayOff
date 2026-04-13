using UnityEngine;

public class Projectile : EntityData
{
    [Header("Projectile")]
    [SerializeField] private Hitbox_Sphere hitbox;
    [SerializeField] private LayerMask reflectedMask;
    [SerializeField] private bool hitbox_Draw = false;
    [SerializeField] private float aliveTime = 10;
    [SerializeField] private float homing_speed = 1;
    [SerializeField] private CrosshairController.CrosshairType crosshair_trigger = CrosshairController.CrosshairType.Hurt;
    private Cooldown aliveTimer;
    private bool reflected = false;

    private static PlayerController player = null;

    #region Unity Methods
    public override void OnEnabled()
    {
        aliveTimer = new Cooldown(this, aliveTime, 1);
        aliveTimer.OnCooldownSuccess += () => Kill("TimedDeath");
        //OnHurt += (_, _, _) => ReflectBullet();

        aliveTimer.Start();
    }
    public override void OnDisabled()
    {
        aliveTimer.OnCooldownSuccess -= () => Kill("TimedDeath");
        //OnHurt -= (_, _, _) => ReflectBullet();
    }

    void Update()
    {
        OnMovement();
        OnAttack();
    }
    private void LateUpdate()
    {
        Homing();
    }

    private void OnDrawGizmos()
    {
        if (hitbox_Draw)
            hitbox.DrawGizmos();
    }
    #endregion

    #region Movement
    /// <summary>
    ///     Controls bullet movement
    /// </summary>
    public virtual void OnMovement()
    {
        DefaultMovement();
    }

    /// <summary>
    ///     Defines default bullet movement
    /// </summary>
    private void DefaultMovement()
    {
        transform.position += transform.forward * GetStatblock().GetSpeed() * Time.deltaTime;
    }

    /// <summary>
    ///     Calculate the target direction for homing
    /// </summary>
    private void Homing()
    {
        Quaternion targetRotation = Quaternion.LookRotation(GetPlayer().transform.position - transform.position);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * homing_speed);
    }
    #endregion
    #region Collision
    /// <summary>
    ///     Controls bullet attacking
    /// </summary>
    private void OnAttack()
    {
        hitbox.Tick();

        // Check if the box should die
        if (hitbox.GetState())
            ExecuteAttack();
    }

    /// <summary>
    ///     Executes the attack
    /// </summary>
    private void ExecuteAttack()
    {
        // Damage the player
        hitbox.GetColliding(out Collider[] colliding);
        foreach (Collider collider in colliding)
            // Get entity data
            if (collider.TryGetComponent(out EntityData data))
            {
                data.Hurt("Projectile", transform.position, GetStatblock().GetAttack());
                CrosshairController.Instance.RequestCrosshair(crosshair_trigger);
            }

        // Kill the box
        Kill("Attack Executed");
    }
    #endregion
    #region Reflect
    /// <summary>
    ///     Reflects the bullet
    /// </summary>
    private void ReflectBullet()
    {
        // Make sure the bullet hasnt been reflected already
        if (reflected)
            return;
        // Lock out method
        reflected = true;

        // Setup reflected movement
        transform.forward = Camera.main.transform.forward;
        hitbox.SetLayerMask(reflectedMask);
    }
    #endregion

    #region Overrides
    public override void Death(bool play_audio = true)
    {
        Destroy(gameObject);
    }
    #endregion
    #region Get Methods
    private PlayerController GetPlayer()
    {
        if (player == null)
            player = FindAnyObjectByType<PlayerController>();
        return player;
    }
    #endregion
}
