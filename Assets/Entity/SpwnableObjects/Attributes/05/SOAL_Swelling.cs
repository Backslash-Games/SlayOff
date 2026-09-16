using HFHandyUtils;
using UnityEngine;

public class SOAL_Swelling : SpawnableObject_AttributeLogic
{
    public override void OnFixedUpdate(SpawnableObject spawnable)
    {
        // Find size increase
        float sizeIncrease = Time.deltaTime * (1 + spawnable.statblock.GetEntropy());

        // Increase size
        spawnable.statblock.AddAdditive(Stat.Tag.size, sizeIncrease);
        spawnable.SetSize(spawnable.statblock.GetSize());
    }
}
