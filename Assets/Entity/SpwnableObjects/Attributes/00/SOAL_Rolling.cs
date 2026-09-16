using UnityEngine;

public class SOAL_Rolling : SpawnableObject_AttributeLogic
{
    public override void OnInitialization(SpawnableObject spawnable)
    {
        spawnable.AddAttribute(SpawnableObject_Attribute.Tag.Grounded);
        spawnable.AddAttribute(SpawnableObject_Attribute.Tag.Propulsion);
        spawnable.AddAttribute(SpawnableObject_Attribute.Tag.Slick);
    }
}
