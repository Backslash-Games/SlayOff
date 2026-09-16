using HFHandyUtils;
using UnityEngine;

public class SOAL_Drippy : SpawnableObject_AttributeLogic
{
    private static readonly string s_ValueIdentifier = "SOAL_Drippy_Distance";

    private static readonly float s_SpawnDistance = 500;
    private static readonly float s_SpawnOffsetStrength = 2.5f;

    public override void OnInitialization(SpawnableObject spawnable)
    {
        spawnable.AddAttributeVariable(s_ValueIdentifier, (float)0);
    }

    public override void OnFixedUpdate(SpawnableObject spawnable)
    {
        // Update drippy spawn distance
        float distance = (float)spawnable.ReadAttributeVariable(s_ValueIdentifier);
        // -> Increase distance
        distance += spawnable.GetRigidbody().linearVelocity.magnitude;
        spawnable.WriteAttributeVariable(s_ValueIdentifier, distance);
        // -> Check if a spawn is ready. Freeze if we are not ready
        if (distance < s_SpawnDistance) return;
        // -> Reset distance
        distance = 0;
        spawnable.WriteAttributeVariable(s_ValueIdentifier, distance);

        // Load resource
        SpawnableObject_Scriptable objectData = Resources.Load<SpawnableObject_Scriptable>(SpawnableObject.GetResourcePath(spawnable.childObject));

        // Set values
        Vector3 offset = s_SpawnOffsetStrength * spawnable.size * -spawnable.GetRigidbody().linearVelocity.normalized;
        /*HFLogger.Log($"strength {s_SpawnOffsetStrength} || size {spawnable.size} || velocity {spawnable.GetRigidbody().linearVelocity} || velocity normalized {spawnable.GetRigidbody().linearVelocity.normalized} || final {offset}");*/
        Vector3 initialVelocity = -spawnable.GetRigidbody().linearVelocity.normalized * 10;

        // Spawn new objects
        SpawnableObject spawn = SpawnableObject.Spawn(objectData, spawnable.transform, offset, null, initialVelocity, spawnable.spawnDepth);

        if (spawn != null) spawn.SetSize(spawnable.size / 2f);
    }
}
