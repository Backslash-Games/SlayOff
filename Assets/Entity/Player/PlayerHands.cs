using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerHands : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private PlayerController player;

    [Header("Melee")]
    [SerializeField] private Weapon melee;
    [SerializeField] private bool drawMeleeHitbox = false;

    [Header("Ranged")]
    [SerializeField] private Weapon ranged;
    [SerializeField] private bool drawRangedHitbox = false;

    [Header("Input")]
    [SerializeField] private InputActionAsset PlayerActions;
    private InputAction in_melee;
    private InputAction in_ranged;

    #region Unity Methods
    private void Awake()
    {
        melee.Setup(this, player);
        ranged.Setup(this, player);
    }
    private void OnDestroy()
    {
        melee.Cleanup();
        ranged.Cleanup();
    }
    private void OnDrawGizmos()
    {
        if(drawMeleeHitbox)
            melee.GetHitbox().DrawGizmos();
        if (drawRangedHitbox)
            ranged.GetHitbox().DrawGizmos();
    }

    private void OnEnable()
    {
        BindEvents();
    }
    private void OnDisable()
    {
        UnbindEvents();
    }
    private void Update()
    {
        TickAttacks();
    }
    #endregion
    #region Bind Events
    /// <summary>
    ///     Binds inputs to the player
    /// </summary>
    private void BindEvents()
    {
        BindInput();
    }
    /// <summary>
    ///     Unbinds inputs from the player
    /// </summary>
    private void UnbindEvents()
    {
        UnbindInput();
    }
    #endregion

    #region Input
    /// <summary>
    ///     Binds Input
    /// </summary>
    private void BindInput()
    {
        // Check if player actions are set
        if (PlayerActions == null)
            return;

        // Setup actions
        in_melee = PlayerActions.FindAction("Melee");

        in_ranged = PlayerActions.FindAction("Ranged");
    }
    /// <summary>
    ///     Unbinds Input
    /// </summary>
    private void UnbindInput()
    {
        // Check if player actions are set
        if (PlayerActions == null)
            return;
    }
    #endregion
    #region Attacks
    private void TickAttacks()
    {
        // Update melee to be pressed
        melee.Tick(in_melee.IsPressed());
        ranged.Tick(in_ranged.IsPressed());
    }
    #endregion

    #region String Methods
    public override string ToString()
    {
        string output = "";

        output += $"Melee:{melee}\n";
        output += $"Ranged:{ranged}\n";

        return output;
    }
    #endregion
}
[System.Serializable]
public class Weapon
{
    [Header("Main Attributes")]
    [SerializeField] private float knockback = 15;
    [SerializeField] private float damage = 1;
    [Space]

    [Header("Hitbox")]
    [SerializeField] private Hitbox_Cube hitbox;
    private Vector3 hitboxDefault_Offset;
    private Vector3 hitboxDefault_Size;
    private Vector3 hitboxDefault_LocalEuler;
    [Space]

    [Header("Cooldown")]
    [SerializeField] private Cooldown cooldown;
    [SerializeField] private float cooldownTime = 1;
    [SerializeField] private float cooldownReductionRate = 1;
    [Space]


    [Header("Consecutive Attack")]
    [SerializeField] private int csc_Current = 0;
    [Space]
    [SerializeField] private int csc_Total = 0;
    [SerializeField] private int csc_AllowedAttacks = 1;
    [Space]
    [SerializeField] private bool csc_Active = false;
    private int csc_Max = 0;
    [Space]

    [Header("Vecolity Bonus")]
    [SerializeField] private float maxLengthBonus = 5;
    [SerializeField] private float maxLengthVelocity = 24;
    [Space]

    [Header("Animation")]
    [SerializeField] private Animator animationController;
    [SerializeField] private string a_RestingLockID = "LockRest";
    [SerializeField] private string a_InitialAttackID = "MeleeStrike_i";
    private bool a_InitialAttackFlag = false;
    [SerializeField] private string[] a_AttackIDs = new string[0];
    [Space]
    [SerializeField] private AnimationEvent a_Event;
    [Space]

    [Header("Slow Down")]
    [SerializeField] private float hitTimeScale = 0.05f;
    [SerializeField] private float hitTimeScaleResetDelay = 0.0875f;
    [Space]

    [Header("Flags")]
    [SerializeField] private bool attacking = false;
    private bool attackingPrevious = false;

    /// <summary>
    ///     Delegate to help track attacking state
    /// </summary>
    /// <param name="state">Current state</param>
    private delegate void AttackingState(bool state);
    /// <summary
    ///     Event called whenever the state changes
    /// </summary>
    private AttackingState OnStateChanged;

    private MonoBehaviour monoBehaviour;
    private PlayerController player = null;
    private TimeControl _timeControl;

    #region Sequencing
    public void Setup(MonoBehaviour mono, PlayerController player)
    {
        // Set variables
        this.player = player;
        monoBehaviour = mono;
        // Create time control
        _timeControl = new TimeControl(monoBehaviour);
        // Create cooldown
        cooldown = new Cooldown(monoBehaviour, cooldownTime, cooldownReductionRate);

        // Setup information
        SetupConsecutiveAttacks();
        SetUpAnimation();

        // Store defaults
        StoreHitboxDefault();
    }
    public void Cleanup()
    {
        // Cleanup information
        CleanupConsecutiveAttacks();
        CleanUpAnimation();
    }

    public void Tick(bool state)
    {
        // Set the attacking state for now
        SetAttackingState(state);
        OnAttack();
    }
    #endregion
    #region Hitbox Management
    /// <summary>
    ///     Stores the hitbox default variables
    /// </summary>
    private void StoreHitboxDefault()
    {
        hitboxDefault_Offset = hitbox.GetOffset();
        hitboxDefault_Size = hitbox.GetSize();
        hitboxDefault_LocalEuler = hitbox.GetLocalEuler();

        RecalculateHitbox();
    }
    /// <summary>
    ///     Recalculates the hitbox based on different player variables
    /// </summary>
    public void RecalculateHitbox()
    {
        // Get the camera transform
        Transform cameraTransform = Camera.main.transform;

        // -> Rotate Box
        hitbox.SetLocalEuler(cameraTransform.eulerAngles + hitboxDefault_LocalEuler);
        // -> Scale box
        Vector3 linearVelocity = player.GetLinearVelocity();
        float hitboxSpeedScaleBonus = Mathf.Lerp(0, maxLengthBonus, (linearVelocity.magnitude / maxLengthVelocity) * Mathm.GetVectorAccuracy(linearVelocity, cameraTransform.forward));
        Vector3 cHitboxScale = hitboxDefault_Size + (Vector3.forward * hitboxSpeedScaleBonus);
        hitbox.SetSize(cHitboxScale);
        // -> Position Box
        Vector3 cHitboxPosition = cameraTransform.rotation * (hitboxDefault_Offset + (Vector3.forward * hitboxSpeedScaleBonus / 2f));
        hitbox.SetOffset(cHitboxPosition);
    }
    #endregion

    #region Attack State
    /// <summary>
    ///     Sets the attacking state, runs OnStateChanged when the state changes
    /// </summary>
    /// <param name="state">New state</param>
    private void SetAttackingState(bool state) 
    {
        // Set our attacking state
        attacking = state;
        // Check if our new state equals our last state
        if(attacking != attackingPrevious)
        {
            // Run event
            OnStateChanged?.Invoke(attacking);
        }

        // Set previous attacking
        attackingPrevious = attacking;
    }

    /// <summary>
    ///     Returns the attacking state
    /// </summary>
    /// <returns>Attacking (Bool)</returns>
    public bool GetAttacking() { return attacking; }
    #endregion
    #region Attack Actions
    private bool coroutineLock = false;
    /// <summary>
    ///     Run attack
    /// </summary>
    public void OnAttack()
    {
        // Check for an early rest unlock
        if (!isConsecutiveAllowed() && csc_Active && !coroutineLock)
        {
            UnlockRest();
            return;
        }
        // Check if we are on cooldown
        if (cooldown.Active())
            return;
        // Check if we are holding input
        if (!GetAttacking())
            return;
        // Play melee attack timed
        monoBehaviour.StartCoroutine(AttackTimed());
    }

    /// <summary>
    ///     Coroutine used for timing the melee attacck
    /// </summary>
    /// <returns>Wait</returns>
    private IEnumerator AttackTimed()
    {
        // Set lock
        coroutineLock = true;
        // Run animation
        LockRest();
        PlayAnimation(GetCurrentAnimationID());
        // Start cooldown
        cooldown.Start();

        // -> Wait for a bit within the animation to execute attack
        while(!a_Event.GetFlagState())
            yield return new WaitForEndOfFrame();

        // Start by setting up hitbox based on values
        RecalculateHitbox();
        // Tick the hitbox
        hitbox.Tick();
        // Get colliding objects
        hitbox.GetColliding(out Collider[] colliders);
        EntityData[] hitEntities = PullEntityDataFromColliders(colliders);
        // Run logic
        HitEntities(hitEntities);

        // Check for impact stun
        if (hitbox.GetState())
            _timeControl.SetScale_AutoReset_Delay(hitTimeScale, hitTimeScaleResetDelay);

        // Set lock
        coroutineLock = false;
    }
    /// <summary>
    ///     Hits all entities in an array
    /// </summary>
    /// <param name="targets">List of Entity Datas</param>
    private void HitEntities(EntityData[] targets)
    {
        foreach (EntityData entity in targets)
        {
            ApplyKnockback(entity);
            ApplyDamage(entity);
        }
    }
    /// <summary>
    ///     Applies melee knockback to entity
    /// </summary>
    /// <param name="target">EntityData</param>
    private void ApplyKnockback(EntityData target)
    {
        // Gets the knockback direction
        Vector3 knockbackDirection = target.transform.position - Camera.main.transform.position;
        // Apply the knockback to the target
        target.ApplyForce(knockbackDirection, knockback, ForceMode.Impulse, "Weapon.Knockback");
    }
    /// <summary>
    ///     Applies melee damage to entity
    /// </summary>
    /// <param name="target">EntityData</param>
    private void ApplyDamage(EntityData target)
    {
        // Apply the knockback to the target
        target.Hurt("Player.Melee", damage);
    }

    #endregion
    #region Consecutive Handling
    /// <summary>
    ///     Sets up structures for consecutive attacks
    /// </summary>
    private void SetupConsecutiveAttacks()
    {
        csc_Max = a_AttackIDs.Length;
        ResetConsecutiveAttack();

        OnStateChanged += _ => ResetConsecutiveAttack();
        cooldown.OnCooldownSuccess += IncreaseAtCooldownEnd;
    }
    /// <summary>
    ///     Cleans up structures for consecutive attacks
    /// </summary>
    private void CleanupConsecutiveAttacks()
    {
        OnStateChanged -= _ => ResetConsecutiveAttack();
        cooldown.OnCooldownSuccess -= IncreaseAtCooldownEnd;
    }

    /// <summary>
    ///     Increase consecutive if we are still holding attacking after cooldown ends
    /// </summary>
    private void IncreaseAtCooldownEnd()
    {
        if (GetAttacking())
            IncreaseConsecutiveAttack();
    }

    /// <summary>
    ///     Increases the current consecutive attack by 1
    /// </summary>
    private void IncreaseConsecutiveAttack() 
    {
        csc_Current = (csc_Current + 1) % csc_Max;
        csc_Total++;
        csc_Active = true;
    }

    /// <summary>
    ///     Checks if consecutive acctacks are allowed
    /// </summary>
    /// <returns>True if we can keep attacking</returns>
    private bool isConsecutiveAllowed() { return csc_Total < csc_AllowedAttacks || csc_AllowedAttacks <= -1; }

    /// <summary>
    ///     Resets the current 
    /// </summary>
    private void ResetConsecutiveAttack() 
    {
        cooldown.Cancel();

        csc_Current = 0;
        csc_Total = 0;
        csc_Active = false; 
    }
    #endregion
    #region Animation
    /// <summary>
    ///     Sets up structures for animations
    /// </summary>
    private void SetUpAnimation()
    {
        OnStateChanged += UnlockRestOnRelease;
        OnStateChanged += SetInitialAnimationFlag;
    }
    /// <summary>
    ///     Cleans up structures for animations
    /// </summary>
    private void CleanUpAnimation()
    {
        OnStateChanged -= UnlockRestOnRelease;
        OnStateChanged -= SetInitialAnimationFlag;
    }

    /// <summary>
    ///     Plays the attack animation
    /// </summary>
    private void PlayAnimation(string name)
    {
        if (animationController == null)
            return;
        // Run animation
        animationController.Play(name, -1, 0);
    }
    /// <summary>
    ///     Locks the resting id on the animator
    /// </summary>
    private void LockRest()
    {
        if (animationController == null)
            return;
        // Run animation
        animationController.SetBool(a_RestingLockID, true);
    }
    /// <summary>
    ///     Unlocks the resting id on the animator
    /// </summary>
    private void UnlockRest()
    {
        if (animationController == null)
            return;
        // Run animation
        animationController.SetBool(a_RestingLockID, false);
    }

    /// <summary>
    ///     Gets the current animation ID
    /// </summary>
    /// <returns>Animation ID</returns>
    private string GetCurrentAnimationID()
    {
        // Check if we are running initial
        if (a_InitialAttackFlag)
        {
            SetInitialAnimationFlag(false);
            return a_InitialAttackID;
        }
        // Return current consecutive ID
        return a_AttackIDs[csc_Current];
    }

    /// <summary>
    ///     Sets the initial animation flag
    /// </summary>
    /// <param name="value">Input value</param>
    private void SetInitialAnimationFlag(bool value) { a_InitialAttackFlag = value; }
    /// <summary>
    ///     Unlocks rest when state is false
    /// </summary>
    /// <param name="state">Input state</param>
    private void UnlockRestOnRelease(bool state)
    {
        if (state)
            return;
        UnlockRest();
    }
    #endregion

    #region Get Methods
    /// <summary>
    ///     Gets the hitbox
    /// </summary>
    /// <returns>Hitbox</returns>
    public Hitbox GetHitbox() { return hitbox; }

    /// <summary>
    ///     Get the entity datas from a list of colliders
    /// </summary>
    /// <param name="colliders">Input colliders</param>
    /// <returns>Array of Entity Data</returns>
    private EntityData[] PullEntityDataFromColliders(Collider[] colliders)
    {
        // Create temporary data structure
        List<EntityData> foundEntities = new List<EntityData>();
        // Find each entity data
        for (int i = 0; i < colliders.Length; i++)
        {
            EntityData cData = colliders[i].GetComponent<EntityData>();
            // Check if it is null
            if (cData != null)
                foundEntities.Add(cData);
        }
        // Return the found list
        return foundEntities.ToArray();
    }
    #endregion
    #region String Methods
    public override string ToString()
    {
        string output = "";

        output += $"{cooldown}\n";

        return output;
    }
    #endregion
}