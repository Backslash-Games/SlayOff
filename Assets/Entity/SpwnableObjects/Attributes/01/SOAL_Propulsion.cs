using UnityEngine;

public class SOAL_Propulsion : SpawnableObject_AttributeLogic
{
    public override void OnFixedUpdate(SpawnableObject spawnable)
    {
        spawnable.ApplyForce(spawnable.transform.forward, spawnable.statblock.GetSpeed() + (spawnable.aliveTime / 4), ForceMode.Acceleration, "SpawnableObject.AttributeLogic.Propulsion");
    }
}
