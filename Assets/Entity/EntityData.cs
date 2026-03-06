using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(Collider))]
public class EntityData : MonoBehaviour
{
    [Header("Entity Data")]
    [SerializeField] private float health = 0;
    [SerializeField] private Statblock statblock = new Statblock();
    [Space]
    [SerializeField] private Collider collision = null;
    [SerializeField] private Rigidbody physicsBody = null;

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
    private void OnEnable()
    {
        // Setup statblock
        statblock.Recalculate();

        // Heal full when enabled
        HealFull("Entity.OnEnable");
        // Run on enable
        OnEnabled();
    }
    #endregion
    #region Unity Passthroughs
    public virtual void OnEnabled() { }
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
    public void Heal(string source, float amount)
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
    }
    /// <summary>
    ///     Heals the entity to full
    /// </summary>
    public void HealFull(string source)
    {
        // Add interaction
        AddInteraction(source, "Entity.HealFull");

        Heal(source, GetStatblock().GetHealth());
    }
    /// <summary>
    ///     Hurts the entity
    /// </summary>
    /// <param name="amount">Hurt amount</param>
    public void Hurt(string source, float amount)
    {
        // Add interaction
        AddInteraction(source, $"Entity.Hurt({amount})");

        // Hurt the entity by amount
        health -= amount;

        // Clamp health to allowed range
        health = Mathf.Clamp(health, 0, GetStatblock().GetHealth());

        // Check for a kill
        if (health <= 0)
            OnDeath();
    }
    /// <summary>
    ///     Kills the entity
    /// </summary>
    public void Kill(string source)
    {
        // Add interaction
        AddInteraction(source, "Entity.Kill");
        Hurt(source, GetStatblock().GetHealth());
    }



    /// <summary>
    ///     Virtual Void - What code is run when the entity dies
    /// </summary>
    public virtual void OnDeath() 
    {
        // Set object to inactive
        gameObject.SetActive(false);
    }
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
