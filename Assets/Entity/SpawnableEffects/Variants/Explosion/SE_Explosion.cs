using HFHandyUtils.Physics;
using UnityEngine;

public class SE_Explosion : SpawnableEffect
{
    private float _knockback = 0;

    /// <summary>
    ///     Sets the properties of the explosion
    /// </summary>
    /// <param name="knockback"></param>
    public void SetProperties(float knockback)
    {
        _knockback = knockback;
    }

    public override void HitEntity(EntityData entity, Statblock stats)
    {
        // Apply Knockback
        entity.ApplyKnockback(entity.transform.position - transform.position, _knockback, "SpawnableEffect.Explosion");
        // Apply damage
        float percentage = Vector3.Distance(transform.position, entity.transform.position) / size;
        float attack = Mathf.Lerp(stats.GetValue(Stat.Tag.attackHigher), stats.GetValue(Stat.Tag.attackLower), percentage);
        entity.Hurt("SpawnableEffect.Explosion", transform.position, attack);
    }
}
