using UnityEngine;
using UnityEngine.AI;

public class Enemy : EntityData
{
    [Header("Pathfinding")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform eyes;

    [Header("Weapon")]
    [SerializeField] private bool melee_UseAttack;
    [SerializeField] private Hitbox_Sphere melee_AggressionRange;
    [SerializeField] private Weapon melee_Weapon;
    [SerializeField] private bool melee_DrawHitbox;

    private PlayerController player = null;

    #region Unity Methods
    private void Start()
    {
        melee_Weapon.Setup(this, GetPlayer());
        SetAgent(GetStatblock());
    }
    private void FixedUpdate()
    {
        // Set the agent destination
        SetAgentDestination(GetPlayer().GetGroundPosition());

        // Look at our target
        LookAtTarget(GetPlayer().gameObject);
        OnMeleeAttack();
    }

    private void OnDrawGizmos()
    {
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
        // Sets the agent destination
        agent.SetDestination(target); 
    }
    
    /// <summary>
    ///     Rotates eyes to look at the target
    /// </summary>
    /// <param name="target">Input Target</param>
    private void LookAtTarget(GameObject target)
    {
        // Check if eyes are set
        if (eyes == null)
            return;
        // Look at target
        eyes.transform.LookAt(target.transform);
    }
    #endregion
    #region Attacks
    /// <summary>
    ///     Checks and runs melee attack
    /// </summary>
    private void OnMeleeAttack()
    {
        // Checks if the melee attack is allowed
        if (!melee_UseAttack)
            return;

        // Ticks the melee weapon
        melee_AggressionRange.Tick();
        melee_Weapon.Tick(melee_AggressionRange.GetState());
    }
    #endregion

    #region String Handling
    public override string ToString()
    {
        string output = "";

        output += $"Melee Attack\n";
        output += $". > Use Attack: {melee_UseAttack}\n";
        output += $". > Draw: {melee_DrawHitbox}\n";
        output += $". > Weapon: {melee_Weapon}\n";

        return output;
    }
    #endregion
}
