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

    [Header("Stamper")]
    [SerializeField] private Weapon r_stamper;
    [SerializeField] private bool drawStamperHitbox = false;

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
        //melee.Setup(this, player);
        r_stamper.Setup(this, player);
        ranged.Setup(this, player);
    }
    private void OnDestroy()
    {
        //melee.Cleanup();
        r_stamper.Cleanup();
        ranged.Cleanup();
    }
    private void OnDrawGizmos()
    {
        /*if(drawMeleeHitbox)
            melee.GetHitbox().DrawGizmos();*/
        if (drawStamperHitbox)
            r_stamper.GetHitbox().DrawGizmos();
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
        //melee.Tick(in_melee.IsPressed());
        r_stamper.Tick(in_melee.IsPressed());
        ranged.Tick(in_ranged.IsPressed());
    }
    #endregion

    #region String Methods
    public override string ToString()
    {
        string output = "";

        output += $"Melee:{melee}\n";
        output += $"Stamper:{r_stamper}\n";
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
    [SerializeField] private Transform orientationTransform;
    [Space]

    [Header("Hit Types")]
    [SerializeField] private bool hitscan = false;
    [SerializeField] private Hitbox_Cube hitbox;
    private Vector3 hitboxDefault_Offset;
    private Vector3 hitboxDefault_Size;
    private Vector3 hitboxDefault_LocalEuler;
    [Space]

    [Header("Reload")]
    [SerializeField] private float cooldownTime = 1;
    [SerializeField] private float cooldownReductionRate = 1;
    private Cooldown reloadCooldown;
    [Space]
    [SerializeField] private string reloadAnimationID = string.Empty;
    [Space]


    [Header("Consecutive Attack")]
    [SerializeField] private int csc_AllowedAttacks = 1;
    [Space]
    [SerializeField] private float csc_SpacingTime = 0; // Time between each consecutive shot
    private Cooldown csc_Cooldown; // Will always have a reduction rate of 1

    private bool csc_Active = false;
    private int csc_Max = 0;
    private int csc_Current = 0;
    private int csc_Total = 0;
    [Space]

    [Header("Vecolity Bonus")]
    [SerializeField] private bool calculateLengthBonus = false;
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

    [Header("Visuals")]
    [SerializeField] private GameObject weapon_visual = null;
    [SerializeField] private Transform wv_nozzle = null;
    [SerializeField] private LayerMask wv_hitmask;

    [Header("Slow Down")]
    [SerializeField] private float hitTimeScale = 0.05f;
    [SerializeField] private float hitTimeScaleResetDelay = 0.0875f;
    [Space]

    [Header("Flags")]
    [SerializeField] private bool attacking = false;
    private bool attackingPrevious = false;
    [SerializeField] private bool cancelAllowed = false;

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
        // Create cooldowns
        reloadCooldown = new Cooldown(monoBehaviour, cooldownTime, cooldownReductionRate);
        csc_Cooldown = new Cooldown(monoBehaviour, csc_SpacingTime, 1);

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
        // Check if our orientation transform is set
        if (orientationTransform == null)
            return;

        // -> Rotate Box
        hitbox.SetLocalEuler(orientationTransform.eulerAngles + hitboxDefault_LocalEuler);
        // -> Scale box
        float hitboxSpeedScaleBonus = 0;
        if (calculateLengthBonus)
        {
            Vector3 linearVelocity = player.GetLinearVelocity();
            hitboxSpeedScaleBonus = Mathf.Lerp(0, maxLengthBonus, (linearVelocity.magnitude / maxLengthVelocity) * Mathm.GetVectorAccuracy(linearVelocity, orientationTransform.forward));
        }
        Vector3 cHitboxScale = hitboxDefault_Size + (Vector3.forward * hitboxSpeedScaleBonus);
        hitbox.SetSize(cHitboxScale);
        // -> Position Box
        Vector3 cHitboxPosition = orientationTransform.rotation * (hitboxDefault_Offset + (Vector3.forward * hitboxSpeedScaleBonus / 2f));
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
        // Ignore input if csc is active and we cant cancel the attack
        if (csc_Active && !cancelAllowed)
            return;

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
        // Check if we are on any cooldown
        if (reloadCooldown.Active() || csc_Cooldown.Active())
            return;
        // Check if we are attempting to attack
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
        PlayAnimation(GetCurrentAnimationID(), 0);
        // Start cooldown
        csc_Cooldown.Start();

        // -> Wait for a bit within the animation to execute attack
        while(a_Event != null && !a_Event.GetState())
            yield return new WaitForEndOfFrame();

        CalculateHitCollision(out EntityData[] hitEntities);
        // Run logic
        HitEntities(hitEntities);
        // Spawn visual
        SpawnVisual();

        // Check for impact stun
        if (hitbox.GetState())
            _timeControl.SetScale_AutoReset_Delay(hitTimeScale, hitTimeScaleResetDelay);

        // Set lock
        coroutineLock = false;
    }
    private void CalculateHitCollision(out EntityData[] hit)
    {
        // Check if we are using hitscan or hitbox
        if (hitscan)
        {
            // TEMPORARY SOLUTION TO MAKE DEADLINE

            // Raycast
            if (Physics.Raycast(orientationTransform.transform.position, orientationTransform.transform.forward, out RaycastHit rHit, 100, hitbox.GetLayerMask()))
            {

                // Try to get entity data
                EntityData cData = rHit.transform.GetComponent<EntityData>();
                if (cData == null)
                    rHit.transform.GetComponentInChildren<EntityData>();
                if (cData == null)
                    rHit.transform.GetComponentInParent<EntityData>();

                // Set hit information
                if (cData != null)
                    hit = new EntityData[] { cData };
                else
                    hit = new EntityData[0];
            }
            else
                hit = new EntityData[0];
            return;

            // TEMPORARY SOLUTION TO MAKE DEADLINE
        }
        else
        {
            // Start by setting up hitbox based on values
            RecalculateHitbox();
            // Tick the hitbox
            hitbox.Tick();
            // Get colliding objects
            hitbox.GetColliding(out Collider[] colliders);
            hit = PullEntityDataFromColliders(colliders);
            return;
        }
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
        csc_Cooldown.OnCooldownSuccess += IncreaseAtCooldownEnd;
    }
    /// <summary>
    ///     Cleans up structures for consecutive attacks
    /// </summary>
    private void CleanupConsecutiveAttacks()
    {
        OnStateChanged -= _ => ResetConsecutiveAttack();
        csc_Cooldown.OnCooldownSuccess -= IncreaseAtCooldownEnd;
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
        // Check if csc max is set
        if (csc_Max == 0)
            return;

        // Increase count
        csc_Current = (csc_Current + 1) % csc_Max;
        csc_Total++;

        // Check csc state
        csc_Active = isConsecutiveAllowed();
        if (!csc_Active) { SetAttackingState(false); }
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
        if(cancelAllowed)
            csc_Cooldown.Cancel();

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
    private void PlayAnimation(string name, int layer)
    {
        if (animationController == null)
            return;
        // Run animation
        animationController.Play(name, layer, 0);
    }
    /// <summary>
    ///     Locks the resting id on the animator
    /// </summary>
    private void LockRest()
    {
        csc_Active = true;
        // Set lock state
        if (animationController != null)
            animationController.SetBool(a_RestingLockID, true);
    }
    /// <summary>
    ///     Unlocks the resting id on the animator
    /// </summary>
    private void UnlockRest()
    {
        // Start reload
        if (reloadAnimationID != string.Empty && reloadAnimationID != "" && !reloadCooldown.Active())
            PlayAnimation(reloadAnimationID, 1);
        reloadCooldown.Start();
        // Set lock state
        if (animationController != null)
            animationController.SetBool(a_RestingLockID, false);
    }

    /// <summary>
    ///     Gets the current animation ID
    /// </summary>
    /// <returns>Animation ID</returns>
    private string GetCurrentAnimationID()
    {
        // Check if attack ids are set
        if (a_AttackIDs.Length <= 0)
            return a_InitialAttackID;
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
    #region Visual Handling
    private void SpawnVisual()
    {
        // Check if we have contents to spawn
        if (weapon_visual == null || wv_nozzle == null)
            return;

        // Spawn visual
        GameObject cObject = MonoBehaviour.Instantiate(weapon_visual, wv_nozzle.transform.position, Quaternion.identity, null);
        cObject.transform.forward = monoBehaviour.transform.forward;
        WeaponVisual cVisual = cObject.GetComponent<WeaponVisual>();

        // Shoot ray
        float rLength = hitbox.GetSize().z;
        Vector3 hitPosition = (monoBehaviour.transform.forward * rLength) + monoBehaviour.transform.position;
        if (Physics.Raycast(monoBehaviour.transform.position, monoBehaviour.transform.forward, out RaycastHit hit, rLength, wv_hitmask))
        {
            hitPosition = hit.point;
            cVisual.SetHitInformation(hit);
        }

        // Initialize visual
        cVisual.Initialize(wv_nozzle.transform, hitPosition);
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

    /// <summary>
    ///     Gets if the weapon is currently on cooldown
    /// </summary>
    /// <returns>True when on cooldown</returns>
    public bool isOnCooldown() { return reloadCooldown.Active(); }
    #endregion
    #region Set Methods
    /// <summary>
    ///     Sets the value of damage
    /// </summary>
    /// <param name="value">New Value</param>
    public void SetDamage(float value) { damage = value; }
    #endregion
    #region String Methods
    public override string ToString()
    {
        string output = "";

        output += $"CSC: {csc_Cooldown}\n";
        output += $"RLD: {reloadCooldown}\n";

        return output;
    }
    #endregion
}