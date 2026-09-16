using UnityEngine;

public class SOAL_Split : SpawnableObject_AttributeLogic
{
    private static readonly float s_SplitLaunchSpeed = 15;
    private static readonly Vector3 s_SpawnOffset = Vector3.up * 0.5f;
    private static readonly float s_SpawnOffsetStrength = 1.25f;

    public override void OnBreak(SpawnableObject spawnable)
    {
        // Load resource
        SpawnableObject_Scriptable objectData = Resources.Load<SpawnableObject_Scriptable>(SpawnableObject.GetResourcePath(spawnable.childObject));

        // Spawn new objects
        Vector3 offset_1 = (s_SpawnOffset - spawnable.transform.right) * s_SpawnOffsetStrength;
        Vector3 offset_2 = (s_SpawnOffset + spawnable.transform.right) * s_SpawnOffsetStrength;

        Vector3 initialVelocity_1 = (-spawnable.transform.right + Vector3.up).normalized * s_SplitLaunchSpeed;
        Vector3 initialVelocity_2 = (spawnable.transform.right + Vector3.up).normalized * s_SplitLaunchSpeed;

        SpawnableObject spawn_1 = SpawnableObject.Spawn(objectData, spawnable.transform, offset_1, spawnable.GetRigidbody(), initialVelocity_1, spawnable.spawnDepth);
        SpawnableObject spawn_2 = SpawnableObject.Spawn(objectData, spawnable.transform, offset_2, spawnable.GetRigidbody(), initialVelocity_2, spawnable.spawnDepth);

        if (spawn_1 != null) spawn_1.SetSize(spawnable.size / 2f);
        if (spawn_2 != null) spawn_2.SetSize(spawnable.size / 2f);
    }
}
