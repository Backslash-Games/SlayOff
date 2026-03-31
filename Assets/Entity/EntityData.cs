using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;

[RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(Collider))]
public class EntityData : MonoBehaviour
{
    [Header("Entity Data")]
    [SerializeField] private float health = 0;
    [SerializeField] private Statblock statblock = new Statblock();
    [Space]
    [SerializeField] private Collider collision = null;
    [SerializeField] private Rigidbody physicsBody = null;
    private RigidbodyConstraints defaultConstraints = RigidbodyConstraints.None;

    [SerializeField] private bool awakeOnStart = true;

    [Header("Entity Data - Visuals")]
    [SerializeField] private Image op_HealthBar = null;

    public enum EffectState { Hurt, Heal, Death, Move };
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
    [Header("Entity Data - Audio")]
    [SerializeField] private bool audio_Muted = false;
    [SerializeField] private bool audio_spatial = true;
    [SerializeField] private bool audio_random_pitch = true;
    [SerializeField] private EntityAudio[] e_audio = new EntityAudio[0];

    [Header("Entity Data - Visual Effects")]
    [SerializeField] private EntityVFX[] e_vfx = new EntityVFX[0];

    // Events
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

    #region Object Interaction Definition
    [System.Serializable]
    private struct ObjectInteration
    {
        [SerializeField] private string source;
        [SerializeField] private string action;
        [SerializeField] private float time;

        public ObjectInteration(string source, string action, float time)
        {
            this.source = source;
            this.action = action;
            this.time = time;
        }

        public override string ToString()
        {
            string output = "";

            output += $"{time}: {source} did {action}";

            return output;
        }
    }
    private List<ObjectInteration> interactions = new List<ObjectInteration>();
    private void AddInteraction(string source, string action)
    {
        interactions.Add(new ObjectInteration(source, action, Time.time));
    }
    #endregion

    #region Unity Methods
    private void Awake()
    {
        // Set defaut parameters
        SetDefaultConstraints();
        // Set awake state
        SetRigidbodyAwake(awakeOnStart);
        
        // Run on awake
        OnAwake();
    }
    private void OnEnable()
    {
        // Setup events
        BindEvents();
        // Setup statblock
        statblock.Recalculate();

        // Heal full when enabled
        HealFull("Entity.OnEnable", false);
        // Run on enable
        OnEnabled();
    }
    private void OnDisable()
    {
        // Setup events
        UnbindEvents();

        // Run on disable
        OnDisabled();
    }
    #endregion
    #region Unity Passthroughs
    public virtual void OnAwake() { }
    public virtual void OnEnabled() { }
    public virtual void OnDisabled() { }
    #endregion
    #region Event Binding
    /// <summary>
    ///     Binds basic entity data events
    /// </summary>
    private void BindEvents()
    {
        OnHurt += (_, _, _) => Tick_HealthBar();
        OnHeal += (_, _, _) => Tick_HealthBar();
    }
    /// <summary>
    ///     Unbinds basic entity data events
    /// </summary>
    private void UnbindEvents()
    {
        OnHurt -= (_, _, _) => Tick_HealthBar();
        OnHeal -= (_, _, _) => Tick_HealthBar();
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
            OnDeath(play_audio);
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
    public virtual void OnDeath(bool play_audio = true)
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
    private void SetDefaultConstraints() { defaultConstraints = GetRigidbody().constraints; }
    public void ResetConstraints() { GetRigidbody().constraints = defaultConstraints; }
    public void SetConstraints(RigidbodyConstraints constraints) { GetRigidbody().constraints = constraints; }
    #endregion

    private static readonly bool WriteToConsole = false;
    /// <summary>
    ///     Runs whenever the entity has a force applied to them
    /// </summary>
    /// <param name="direction">Direction of force</param>
    /// <param name="strength">Strength of force</param>
    /// <param name="mode">Force mode - Unity</param>
    public virtual void ApplyForce(Vector3 direction, float strength, ForceMode mode, string source = "Unknown")
    {
        // Get the rigidbody
        Rigidbody lBody = GetRigidbody();

        // Apply the force
        lBody.AddForce(direction.normalized * strength, mode);

        // Debug
        if(WriteToConsole)
            Debug.Log($"Force: {source} > {direction.normalized * strength} > {mode}\nLinear Velocity: {GetLinearVelocity()}\nLog from {name}");
    }
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

    #endregion
    #region Audio
    public void MuteAudio() { audio_Muted = true; }
    public void UnmuteAudio() { audio_Muted = false; }

    public void PlayAudio(EffectState type) 
    {
        if (audio_Muted)
            return;
        if (AudioManager.Instance == null)
            return;

        // Get audio clip of type
        foreach (EntityAudio value in e_audio)
        {
            if (value.GetEffectState().Equals(type))
            {
                // Play the audio clip
                AudioManager.Instance.PlayAudio(value.GetAudioClip(), transform.position, audio_random_pitch, !audio_spatial);
                return;
            }
        }

    }
    #endregion
    #region VFX

    public void PlayVFX(EffectState type)
    {
        if (VisualEffectManager.Instance == null)
            return;

        // Get audio clip of type
        foreach (EntityVFX value in e_vfx)
        {
            if (value.GetEffectState().Equals(type))
            {
                // Play the vfx
                VisualEffectManager.Instance.PlayVisual(value.GetVFXAsset(), transform.position, value.GetPlayTime(), value.GetEventName());
                return;
            }
        }

    }
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
