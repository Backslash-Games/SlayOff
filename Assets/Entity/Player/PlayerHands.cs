using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerHands : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private PlayerController player;

    [Header("Melee")]
    [SerializeField] private float maxMeleeWeaponLengthBonus = 0;
    [SerializeField] private float maxMeleeWeaponLengthVelocity = 0;
    [Space]
    [SerializeField] private float meleeWeaponKnockback = 5;
    [SerializeField] private float meleeWeaponDamage = 1;
    [Space]
    [SerializeField] private Hitbox_Cube weaponHitbox;
    private Vector3 weaponHitboxDefault_Offset;
    private Vector3 weaponHitboxDefault_Size;
    private Vector3 weaponHitboxDefault_LocalEuler;

    [Header("Input")]
    [SerializeField] private InputActionAsset PlayerActions;
    private InputAction in_melee;
    private InputAction in_ranged;

    #region Unity Methods
    private void OnDrawGizmos()
    {
        weaponHitbox.DrawGizmos();
    }

    private void OnEnable()
    {
        StoreHitboxDefault();
        RecalculateHitbox();
        BindEvents();
    }
    private void OnDisable()
    {
        UnbindEvents();
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
        in_melee.performed += _ => OnMeleeAttack();

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
    #region Hitbox Management
    /// <summary>
    ///     Stores the hitbox default variables
    /// </summary>
    private void StoreHitboxDefault()
    {
        weaponHitboxDefault_Offset = weaponHitbox.GetOffset();
        weaponHitboxDefault_Size = weaponHitbox.GetSize();
        weaponHitboxDefault_LocalEuler = weaponHitbox.GetLocalEuler();
    }
    /// <summary>
    ///     Recalculates the hitbox based on different player variables
    /// </summary>
    private void RecalculateHitbox()
    {
        // Get the camera transform
        Transform cameraTransform = Camera.main.transform;

        // Melee
        // -> Rotate Box
        weaponHitbox.SetLocalEuler(cameraTransform.eulerAngles + weaponHitboxDefault_LocalEuler);
        // -> Scale box
        Vector3 linearVelocity = player.GetLinearVelocity();
        float hitboxSpeedScaleBonus = Mathf.Lerp(0, maxMeleeWeaponLengthBonus, (linearVelocity.magnitude / maxMeleeWeaponLengthVelocity) * Mathm.GetVectorAccuracy(linearVelocity, cameraTransform.forward));
        Vector3 meleeBoxScale = weaponHitboxDefault_Size + (Vector3.forward * hitboxSpeedScaleBonus);
        weaponHitbox.SetSize(meleeBoxScale);
        // -> Position Box
        Vector3 meleeBoxPosition = cameraTransform.rotation * (weaponHitboxDefault_Offset + (Vector3.forward * hitboxSpeedScaleBonus / 2f));
        weaponHitbox.SetOffset(meleeBoxPosition);
    }
    #endregion

    #region Attacks
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
        for(int i = 0; i < colliders.Length; i++)
        {
            EntityData cData = colliders[i].GetComponent<EntityData>();
            // Check if it is null
            if (cData != null)
                foundEntities.Add(cData);
        }
        // Return the found list
        return foundEntities.ToArray();
    }

    #region Melee
    /// <summary>
    ///     Called when we try to melee attack
    /// </summary>
    private void OnMeleeAttack()
    {
        // Start by setting up hitbox based on values
        RecalculateHitbox();
        // Tick the hitbox
        weaponHitbox.Tick();
        // Get colliding objects
        weaponHitbox.GetColliding(out Collider[] colliders);
        EntityData[] hitEntities = PullEntityDataFromColliders(colliders);
        // Run logic
        HitEntities(hitEntities);
    }
    /// <summary>
    ///     Hits all entities in an array
    /// </summary>
    /// <param name="targets">List of Entity Datas</param>
    private void HitEntities(EntityData[] targets)
    {
        foreach(EntityData entity in targets)
        {
            ApplyMeleeKnockback(entity);
            ApplyMeleeDamage(entity);
        }
    }
    /// <summary>
    ///     Applies melee knockback to entity
    /// </summary>
    /// <param name="target">EntityData</param>
    private void ApplyMeleeKnockback(EntityData target)
    {
        // Gets the knockback direction
        Vector3 knockbackDirection = target.transform.position - Camera.main.transform.position;
        // Apply the knockback to the target
        target.ApplyForce(knockbackDirection, meleeWeaponKnockback, ForceMode.Impulse, "Weapon.Knockback");
    }
    /// <summary>
    ///     Applies melee damage to entity
    /// </summary>
    /// <param name="target">EntityData</param>
    private void ApplyMeleeDamage(EntityData target)
    {
        // Apply the knockback to the target
        target.Hurt("Player.Melee", meleeWeaponDamage);
    }
    #endregion
    #endregion
}
