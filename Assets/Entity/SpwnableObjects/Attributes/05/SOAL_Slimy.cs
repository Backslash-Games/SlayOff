using HFHandyUtils.Data.Spawning;
using UnityEngine;

public class SOAL_Slimy : SpawnableObject_AttributeLogic
{
    private static readonly LayerMask s_RubberLayerMask = 21376;

    private static readonly float s_RubberKnockback = 17.5f;
    private static readonly float s_RubberSizeScale = 2.5f;

    public override void OnInitialization(SpawnableObject spawnable)
    {
        spawnable.AddAttribute(SpawnableObject_Attribute.Tag.Bounce);
    }

    public override void OnImpact(SpawnableObject spawnable)
    {
        // Check if we have impacted
        if (spawnable.GetImpactPoint_Velocity(out RaycastHit hit))
        {
            // Setup rubber
            SE_Rubber rubber = Spawn_SpawnableEffect("Rubber", spawnable, hit) as SE_Rubber;
            rubber.SetProperties(s_RubberKnockback);
            rubber.Initialize(spawnable, s_RubberLayerMask, spawnable.statblock.GetSize() * s_RubberSizeScale);
        }
    }
}
