using HFHandyUtils.Effects;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;

[RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(Collider))]
public class EntityData : MonoBehaviour
{
    public enum Team { None = 0, Allied, Opposing, Prop };

    [Header("Entity Data")]
    public Team team = 0;
    public delegate void OnTeamModified(Team team);
    public OnTeamModified OnTeamChanged;
    [Space]
    [SerializeField] private float health = 0;
    public Statblock statblock = new Statblock();
    [Space]
    [SerializeField] private Collider collision = null;
    [SerializeField] private Rigidbody physicsBody = null;
    
    private RigidbodyConstraints default_Constraints = RigidbodyConstraints.None;
    private PhysicsMaterial default_PhysicsMaterial = null;
    private float default_LinearDamping = 0;
    private float default_Mass = 0;

    public float controlScale = 1;

    [SerializeField] private bool awakeOnStart = true;
    
    
    public enum EffectState { Hurt, Heal, Death };
    [System.Serializable]
    private struct EntityAudio
    {
        [SerializeField] private EffectState effectState;
        [SerializeField] private AudioClip audioClip;

        public EntityAudio(EffectState effectState, AudioClip audioClip)
        {
            this.effectState = effectState;
            this.audioClip = audioClip;
        }

        public EffectState GetEffectState() { return effectState; }
        public AudioClip GetAudioClip() { return audioClip; }
    }
    [System.Serializable]
    private struct EntityVFX
    {
        [SerializeField] private EffectState effectState;
        [SerializeField] private VisualEffectAsset visualEffectAsset;
        [Space]
        [SerializeField] private float play_time;
        [SerializeField] private string event_name;

        public EntityVFX(EffectState effectState, VisualEffectAsset visualEffectAsset, float play_time, string event_name)
        {
            this.effectState = effectState;
            this.visualEffectAsset = visualEffectAsset;
            this.play_time = play_time;
            this.event_name = event_name;
        }

        public EffectState GetEffectState() { return effectState; }
        public VisualEffectAsset GetVFXAsset() { return visualEffectAsset; }
        public float GetPlayTime() { return play_time; }
        public string GetEventName() { return event_name; }
    }

    [Header("Entity Data - Visuals")]
    [SerializeField] private EffectLibrary<EffectState, VisualEffectAsset, EffectComponent_Visual.VisualParameters> particleLibrary;
    [Space]
    [SerializeField] private Image op_HealthBar = null;

    [Header("Entity Data - Audio")]
    [SerializeField] private EffectLibrary<EffectState, AudioClip, EffectComponent_Audio.AudioParameters> audioLibrary;
    [Space]
    [SerializeField] private bool audio_Muted = false;
    [SerializeField] private bool audio_spatial = true;
    [SerializeField] private bool audio_random_pitch = true;


    #region Events
    /// <summary>
    ///     Delegate that tracks when health is changed
    /// </summary>
    /// <param name="amount">Amount changed</param>
    public delegate void HealthChanged(string source, Vector3 origin, float amount);
    /// <summary>
    ///     Event that tracks when the entity is hurt
    /// </summary>
    public event HealthChanged OnHurt;
    /// <summary>
    ///     Event that tracks when the entity is healed
    /// </summary>
    public event HealthChanged OnHeal;

    /// <summary>
    ///     Delegate that tracks events bound to certain entity states
    /// </summary>
    public delegate void EntityState(bool playAudio);
    /// <summary>
    ///     Event that tracks when entity is dead
    /// </summary>
    public event EntityState OnDeath;
    #endregion


    #region Object Interaction Definition
    /// <summary>
    ///     Current list of interactions on the entity
    /// </summary>
    private List<ObjectInteration> interactions = new List<ObjectInteration>();
    private static readonly int s_MaximumInteractionLength = 30;

    /// <summary>
    ///     Structure used to track information about interactions on each entity
    /// </summary>
    [System.Serializable]
    private struct ObjectInteration
    {
        [SerializeField] private string _source;
        [SerializeField] private string _action;
        [SerializeField] private float _time;

        public ObjectInteration(string source, string action, float time)
        {
            _source = source;
            _action = action;
            _time = time;
        }

        public override string ToString()
        {
            string output = "";

            output += $"{_time}: {_source} did {_action}";

            return output;
        }
    }
    /// <summary>
    ///     Adds a new interaction to the entity
    /// </summary>
    /// <param name="source">Interaction Source</param>
    /// <param name="action">Interaction Action</param>
    private void AddInteraction(string source, string action)
    {
        interactions.Add(new ObjectInteration(source, action, Time.time));

        // Check if interactions have overflown maximum length
        while (interactions.Count > s_MaximumInteractionLength)
            interactions.RemoveAt(0);
    }
    #endregion

    #region Unity Methods
    protected virtual void Awake()
    {
        // Set defaut parameters
        SetDefaultConstraints();
        SetDefaultPhysicsMaterial();
        SetDefaultLinearDamping();
        SetDefaultMass();
        // Set awake state
        SetRigidbodyAwake(awakeOnStart);
    }
    protected virtual void OnEnable()
    {
        // Setup events
        BindEvents();
        // Setup statblock
        statblock.Recalculate();

        // Enables team logic
        EnableTeamLogic();

        // Heal full when enabled
        HealFull("Entity.OnEnable", false);
    }
    public void Force_OnEnable() { OnEnable(); }
    protected virtual void OnDisable()
    {
        // Disables team logic
        DisableTeamLogic();

        // Setup events
        UnbindEvents();
    }
    public void Force_OnDisable() { OnDisable(); }
    #endregion
    #region Event Binding
    /// <summary>
    ///     Binds basic entity data events
    /// </summary>
    private void BindEvents()
    {
        OnHurt += (_, _, _) => Tick_HealthBar();
        OnHeal += (_, _, _) => Tick_HealthBar();

        OnDeath += Death;
    }
    /// <summary>
    ///     Unbinds all events
    /// </summary>
    private void UnbindEvents()
    {
        OnHurt = null;
        OnHeal = null;
        OnDeath = null;
    }
    #endregion

    #region Stats
    public Statblock GetStatblock()
    {
        if (statblock == null)
            statblock = new Statblock();
        return statblock;
    }
    #endregion

    #region Health Management
    /// <summary>
    ///     Heals the entity
    /// </summary>
    /// <param name="amount">Heal amount</param>
    public void Heal(string source, float amount, bool play_audio = true)
    {
        // Add interaction
        AddInteraction(source, $"Entity.Heal({amount})");

        // Find the allowed increase amount
        float mHealth = GetStatblock().GetHealth() - health;

        // Clamp heal to allowed amount
        float cHeal = Mathf.Clamp(amount, 0, mHealth);
        // -> Check if healing is 0, if so return early
        if (cHeal <= 0)
            return;

        // Heal the entity
        health += cHeal;

        // Play heal audio
        if(play_audio)
            PlayAudio(EffectState.Heal);
        PlayVFX(EffectState.Heal);

        // Run Heal event
        OnHeal?.Invoke(source, Vector3.zero, amount);
    }
    /// <summary>
    ///     Heals the entity to full
    /// </summary>
    public void HealFull(string source, bool play_audio = true)
    {
        // Add interaction
        AddInteraction(source, "Entity.HealFull");

        Heal(source, GetStatblock().GetHealth(), play_audio);
    }
    /// <summary>
    ///     Hurts the entity
    /// </summary>
    /// <param name="amount">Hurt amount</param>
    public void Hurt(string source, Vector3 origin, float amount, bool play_audio = true)
    {
        // Add interaction
        AddInteraction(source, $"Entity.Hurt({amount})");

        // Hurt the entity by amount
        health -= amount;

        // Clamp health to allowed range
        health = Mathf.Clamp(health, 0, GetStatblock().GetHealth());

        // Play hurt audio
        if (play_audio)
            PlayAudio(EffectState.Hurt);
        PlayVFX(EffectState.Hurt);

        // Run hurt event
        OnHurt?.Invoke(source, origin, amount);

        // Check for a kill
        if (isDead())
            OnDeath?.Invoke(play_audio);
    }
    /// <summary>
    ///     Kills the entity
    /// </summary>
    public void Kill(string source, bool play_audio = true)
    {
        // Add interaction
        AddInteraction(source, "Entity.Kill");
        Hurt(source, transform.position, GetStatblock().GetHealth(), play_audio);
    }



    /// <summary>
    ///     Virtual Void - What code is run when the entity dies
    /// </summary>
    public virtual void Death(bool play_audio = true)
    {
        // Play death audio
        if (play_audio)
            PlayAudio(EffectState.Death);
        PlayVFX(EffectState.Death);

        // Set object to inactive
        gameObject.SetActive(false);
    }
    /// <summary>
    ///     Checks if the entity is dead
    /// </summary>
    /// <returns>True when dead</returns>
    public bool isDead() { return health <= 0; }
    #endregion
    #region Collision
    /// <summary>
    ///     Gets the collision tied to the current entity
    /// </summary>
    /// <returns>Collision</returns>
    public Collider GetCollision()
    {
        if (collision == null)
            collision = GetComponent<Collider>();
        return collision;
    }
    #endregion
    #region Physics
    /// <summary>
    ///     Pulls a reference to the entity rigidbody
    /// </summary>
    /// <returns>Rigidbody</returns>
    public Rigidbody GetRigidbody()
    {
        if (physicsBody == null)
            physicsBody = GetComponent<Rigidbody>();
        return physicsBody;
    }
    #region Velocity
    #region Get
    /// <summary>
    ///     Pulls the current linear velocity
    /// </summary>
    /// <returns>Linear Velocity</returns>
    public Vector3 GetLinearVelocity()
    {
        return GetRigidbody().linearVelocity;
    }
    /// <summary>
    ///     Pulls the current horizontal velocity
    /// </summary>
    /// <returns>Horizontal Velocity</returns>
    public Vector2 GetHorizontalVelocity()
    {
        Vector3 linearVelocity = GetLinearVelocity();
        return new Vector2(linearVelocity.x, linearVelocity.z);
    }
    /// <summary>
    ///     Pulls the current vertical velocity
    /// </summary>
    /// <returns>Vertical Velocity</returns>
    public float GetVerticalVelocity()
    {
        Vector3 linearVelocity = GetLinearVelocity();
        return linearVelocity.y;
    }

    /// <summary>
    ///     Pull current angular velocity
    /// </summary>
    /// <returns>Angular Velocity</returns>
    public Vector3 GetAngularVelocity()
    {
        return GetRigidbody().angularVelocity;
    }
    #endregion
    #region Set
    /// <summary>
    ///     Halts all vertical velocity while maintaining horizontal
    /// </summary>
    public void ResetVerticalVelocity()
    {
        // Pull horizontal velocity and force set it
        Vector2 horizontalVelocity = GetHorizontalVelocity();
        GetRigidbody().linearVelocity = new Vector3(horizontalVelocity.x, 0, horizontalVelocity.y);
    }
    /// <summary>
    ///     Halts all velocity
    /// </summary>
    public void ResetVelocity()
    {
        GetRigidbody().linearVelocity = Vector3.zero;
    }
    /// <summary>
    ///     Sets the awake state of the rigidbody
    /// </summary>
    /// <param name="state">New state</param>
    public void SetRigidbodyAwake(bool state)
    {
        if (state)
            GetRigidbody().WakeUp();
        else
            GetRigidbody().Sleep();
    }
    #endregion
    #endregion
    #region Constraints
    private void SetDefaultConstraints() { default_Constraints = GetRigidbody().constraints; }
    public void ResetConstraints() { GetRigidbody().constraints = default_Constraints; }
    public void SetConstraints(RigidbodyConstraints constraints) { GetRigidbody().constraints = constraints; }
    #endregion
    #region Material
    private void SetDefaultPhysicsMaterial() { default_PhysicsMaterial = GetCollision().sharedMaterial; }
    public void ResetPhysicsMaterial() { GetCollision().sharedMaterial = default_PhysicsMaterial; }
    public void SetPhysicsMaterial(PhysicsMaterial material) { GetCollision().sharedMaterial = material; }
    #endregion
    #region Linear Drag
    private float _lastLinearDampingSet = 0;
    private void SetDefaultLinearDamping() { default_LinearDamping = GetRigidbody().linearDamping; }
    public void ResetLinearDamping() { GetRigidbody().linearDamping = default_LinearDamping; }
    public void SetLinearDamping(float damping) { GetRigidbody().linearDamping = _lastLinearDampingSet = damping; }
    public void SetLinearDampingPercentage(float percentage) { GetRigidbody().linearDamping = Mathf.Lerp(_lastLinearDampingSet, default_LinearDamping, percentage); }
    #endregion
    #region Mass
    private float _lastMassSet = 0;
    private void SetDefaultMass() { default_Mass = GetRigidbody().mass; }
    public void ResetMass() { GetRigidbody().mass = default_Mass; }
    public void SetMass(float mass) { GetRigidbody().mass = _lastMassSet = mass; }
    public void SetMassPercentage(float percentage) { GetRigidbody().mass = Mathf.Lerp(_lastMassSet, default_Mass, percentage); }
    #endregion

    #region Forces
    private Cooldown knockbackCooldown = null;

    private static readonly float s_KnockbackTime = 1;
    private static readonly bool WriteToConsole = false;
    /// <summary>
    ///     Runs whenever the entity has a force applied to them
    /// </summary>
    /// <param name="direction">Direction of force</param>
    /// <param name="strength">Strength of force</param>
    /// <param name="mode">Force mode - Unity</param>
    /// <param name="source">Source of force</param>
    public virtual void ApplyForce(Vector3 direction, float strength, ForceMode mode, string source = "Unknown", bool ignoreControlScale = false)
    {
        // Get the rigidbody
        Rigidbody lBody = GetRigidbody();

        // Apply the force
        float cScale = ignoreControlScale ? 1 : controlScale;
        lBody.AddForce(direction.normalized * strength * cScale, mode);

        // Debug
        if(WriteToConsole)
            Debug.Log($"Force: {source} > {direction.normalized * strength} > {mode}\nLinear Velocity: {GetLinearVelocity()}\nLog from {name}");
    }
    /// <summary>
    ///     Applies knockback to the entity
    /// </summary>
    /// <param name="direction">Direction of force</param>
    /// <param name="strength">Strength of force</param>
    /// <param name="source">Source of force</param>
    public virtual void ApplyKnockback(Vector3 direction, float strength, string source = "Unknown")
    {
        StartCoroutine(ie_ApplyKnockback(direction, strength, source));
    }
    protected virtual IEnumerator ie_ApplyKnockback(Vector3 direction, float strength, string source = "Unknown")
    {
        // Disable physics control
        EnableRagdoll();
        // Start cooldown
        if (knockbackCooldown == null)
        {
            knockbackCooldown = new Cooldown(this, s_KnockbackTime, 1);
            knockbackCooldown.OnCooldownUpdate += () => { SetRagdollPercentage(knockbackCooldown.GetPercentComplete()); };
            knockbackCooldown.OnCooldownEnded += () => { DisableRagdoll(); };
        }
        if (knockbackCooldown.Active()) knockbackCooldown.ResetTimer();
        knockbackCooldown.Start();

        yield return new WaitForEndOfFrame();

        // Apply force
        ApplyForce(direction, strength, ForceMode.Impulse, source, true);
    }
    /// <summary>
    ///     Applies torque to the entity
    /// </summary>
    /// <param name="direction">Direction of force</param>
    /// <param name="strength">Strength of force</param>
    /// <param name="mode">Force mode - Unity</param>
    /// <param name="source">Source of force</param>
    public virtual void ApplyTorque(Vector3 direction, float strength, ForceMode mode, string source = "Unknown")
    {
        // Get the rigidbody
        Rigidbody lBody = GetRigidbody();

        // Apply the force
        lBody.AddTorque(direction.normalized * strength, mode);

        // Debug
        if (WriteToConsole)
            Debug.Log($"Torque: {source} > {direction.normalized * strength} > {mode}\nAngular Velocity: {GetAngularVelocity()}\nLog from {name}");
    }
    #endregion
    #region Ragdolling
    /// <summary>
    ///     Removes resistance from the rigid body
    /// </summary>
    protected virtual void EnableRagdoll()
    {
        controlScale = 0;
        SetLinearDamping(0);
        SetMass(1);
    }
    /// <summary>
    ///     Adds back resistance to the rigid body
    /// </summary>
    protected virtual void DisableRagdoll()
    {
        controlScale = 1;
        ResetLinearDamping();
        ResetMass();
    }
    /// <summary>
    ///     Drip feeds control back to the entity
    /// </summary>
    /// <param name="percent">Current percentage</param>
    protected virtual void SetRagdollPercentage(float percent)
    {
        controlScale = percent;
        SetLinearDampingPercentage(percent);
        SetMassPercentage(percent);
    }
    #endregion
    #endregion

    #region Positions
    private static readonly int groundCheckLength = 30;
    public Vector3 GetGroundPosition()
    {
        // Pull the ground point
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, groundCheckLength, LayerMask.NameToLayer("Terrain")))
            return hit.point;

        // Pull the default position
        return transform.position;
    }
    #endregion
    #region Visuals
    /// <summary>
    ///     Optional method, updates the entity healthbar.
    /// </summary>
    public virtual void Tick_HealthBar()
    {
        // Check if the healthbar is set... If not thats okay, just ignore
        if (op_HealthBar == null)
            return;

        // Defaults to setting the healthbar filling
        op_HealthBar.fillAmount = health / GetStatblock().GetHealth();
    }

    /// <summary>
    ///     Plays a visual effect
    /// </summary>
    /// <param name="type">Effect type</param>
    public void PlayVFX(EffectState type)
    {
        if (EffectManager.Instance == null)
            return;

        // Play visual
        EffectManager.Instance.Play(particleLibrary, type);
    }
    #endregion
    #region Audio
    public void MuteAudio() { audio_Muted = true; }
    public void UnmuteAudio() { audio_Muted = false; }

    public void PlayAudio(EffectState type) 
    {
        if (audio_Muted)
            return;
        if (EffectManager.Instance == null)
            return;

        // Play audio
        EffectManager.Instance.Play(audioLibrary, type);
    }
    #endregion

    #region Teams
    /// <summary>
    ///     Binds methods to team events
    /// </summary>
    private void EnableTeamLogic()
    {
        // Add this to the entity manager
        AddToEntityManager();

        // Bind events
        OnTeamChanged += _ => UpdateEntityManagerPlacement();
    }
    /// <summary>
    ///     Unbinds methods to team events
    /// </summary>
    private void DisableTeamLogic()
    {
        // Remove this from the entity manager
        RemoveFromEntityManager();

        // Unbind events
        OnTeamChanged -= _ => UpdateEntityManagerPlacement();
    }

    /// <summary>
    ///     Adds this to the entity manager
    /// </summary>
    private void AddToEntityManager()
    {
        if (EntityManager.Instance == null) FindAnyObjectByType<EntityManager>().AddToActive(this);
        else EntityManager.Instance.AddToActive(this);
    }
    /// <summary>
    ///     Removes this from the entity manager
    /// </summary>
    private void RemoveFromEntityManager()
    {
        EntityManager.Instance.RemoveFromActive(this);
    }
    /// <summary>
    ///     Updates this entities placement in the entity manager
    /// </summary>
    private void UpdateEntityManagerPlacement()
    {
        RemoveFromEntityManager();
        AddToEntityManager();
    }

    /// <summary>
    ///     Changes the entity team
    /// </summary>
    /// <param name="team">New team</param>
    public void ChangeTeam(Team team)
    {
        this.team = team;
        OnTeamChanged?.Invoke(team);
    }


    /// <summary>
    ///     Checks if this entity is on other entities team
    /// </summary>
    /// <param name="other">Other entity</param>
    /// <returns>True if on team</returns>
    public bool isOnTeam(EntityData other) { return isOnTeam(other.team); }
    /// <summary>
    ///     Checks if this entity is on a team
    /// </summary>
    /// <param name="tTest">Test team</param>
    /// <returns>True if on team</returns>
    public bool isOnTeam(Team tTest) { return team.Equals(tTest); }
    #endregion

    #region Debug
    public override string ToString()
    {
        string output = "";

        output += $"Entity Name: {name}\n";
        output += $"Interactions\n";
        foreach (ObjectInteration interaction in interactions)
            output += $". > {interaction}\n";
        output += $"\n\n";

        return output;
    }
    #endregion
}
