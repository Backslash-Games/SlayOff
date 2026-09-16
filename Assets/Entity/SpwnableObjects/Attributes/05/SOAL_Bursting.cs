using UnityEngine;

public class SOAL_Bursting : SpawnableObject_AttributeLogic
{
    private static readonly float s_MaxSize = 2.5f;

    public override void OnInitialization(SpawnableObject spawnable)
    {
        spawnable.AddAttribute(SpawnableObject_Attribute.Tag.Swelling);
    }
    public override void OnFixedUpdate(SpawnableObject spawnable)
    {
        // Check for a burst
        if (spawnable.statblock.GetSize() >= s_MaxSize * (1 + spawnable.statblock.GetEntropy()))
        {
            spawnable.AddAttribute(SpawnableObject_Attribute.Tag.Explosive);
            spawnable.Break();
        }
    }
}
