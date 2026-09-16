using HFHandyUtils.Data.Spawning;
using UnityEngine;

public class SOAL_Explosive : SpawnableObject_AttributeLogic
{
    private static readonly LayerMask s_ExplosiveLayerMask = 896;

    private static readonly float s_ExplosiveKnockback = 25;
    private static readonly float s_ExplosiveRadiusScale = 8.5f;

    public override void OnBreak(SpawnableObject spawnable)
    {
        // Spawn explosion
        GameObject explosivePrefab = ObjectPooler.Instance.Spawn("Explosion", spawnable.transform.position, Vector3.zero, true);

        SE_Explosion spawnedExplosion = explosivePrefab.GetComponent<SE_Explosion>();
        spawnedExplosion.SetProperties(s_ExplosiveKnockback);
        spawnedExplosion.OnTrigger(spawnable, s_ExplosiveLayerMask, spawnable.statblock.GetSize() * s_ExplosiveRadiusScale);
    }
}
