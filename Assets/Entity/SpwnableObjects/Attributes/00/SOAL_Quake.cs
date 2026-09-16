using HFHandyUtils.Data.Spawning;
using UnityEngine;

public class SOAL_Quake : SpawnableObject_AttributeLogic
{
    private static readonly LayerMask s_FissureLayerMask = 21376;

    private static readonly float s_FissureKnockback = 25f;
    private static readonly float s_FissureScale = 11.5f;

    public override void OnImpact(SpawnableObject spawnable)
    {
        // Check if we have impacted
        if (spawnable.GetImpactPoint_Velocity(out RaycastHit hit))
        {
            // Setup fissure
            SE_Fissure fissure = Spawn_SpawnableEffect("Fissure", spawnable, hit) as SE_Fissure;
            fissure.SetProperties(s_FissureKnockback);
            fissure.OnTrigger(spawnable, s_FissureLayerMask, spawnable.statblock.GetSize() * s_FissureScale);
        }
    }
}
