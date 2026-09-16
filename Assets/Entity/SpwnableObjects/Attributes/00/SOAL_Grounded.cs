using UnityEngine;

public class SOAL_Grounded : SpawnableObject_AttributeLogic
{
    public override void OnInitialization(SpawnableObject spawnable)
    {
        spawnable.statblock.AddPercentage(Stat.Tag.weight, 10);
    }
}
