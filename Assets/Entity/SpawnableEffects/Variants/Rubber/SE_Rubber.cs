using System.Drawing;
using UnityEngine;

public class SE_Rubber : SE_GroundEffect
{
    public PhysicsMaterial bounceMaterial;

    private float _knockback = 0;

    public void SetProperties(float knockback)
    {
        _knockback = knockback;
    }
    public override void HitGround(EntityData entity, Statblock stats)
    {
        Vector3 entityVelocity = entity.GetLinearVelocity();

        // Remove velocity along transform.up
        Quaternion rotation = Quaternion.FromToRotation(transform.up, Vector3.up);
        entityVelocity = rotation * entityVelocity;
        entityVelocity = Mathh.RemoveVerticalAxis(entityVelocity);
        entityVelocity = Quaternion.Inverse(rotation) * entityVelocity;

        // Apply force
        entity.GetRigidbody().linearVelocity = entityVelocity;
        entity.ApplyKnockback(transform.up, _knockback, "SpawnableEffect.Rubber");
    }
}
