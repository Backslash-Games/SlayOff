using UnityEngine;

public class SE_Fissure : SE_Rubber
{
    public override void HitEntity(EntityData entity, Statblock stats)
    {
        base.HitEntity(entity, stats);
        if (!entity.isOnTeam(team)) entity.Hurt("SpawnableEffect.Fissure", transform.position, GetStatblock().GetAttack());
    }
}
