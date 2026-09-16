using UnityEngine;

public class SOAL_Knockback : SpawnableObject_AttributeLogic
{
    public override void OnImpactEntity(SpawnableObject spawnable, EntityData entity)
    {
        float strength = spawnable.statblock.GetAttack() + spawnable.statblock.GetWeight();
        entity.ApplyKnockback(spawnable.GetLinearVelocity(), strength, "SpawnableObject.AttributeLogic.Knockback");
    }
}
