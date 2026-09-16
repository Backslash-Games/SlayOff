using UnityEngine;

public class SOAL_Crumbly : SpawnableObject_AttributeLogic
{
    private static readonly float s_SplitLaunchSpeed = 15;
    private static readonly Vector3 s_SpawnOffset = Vector3.up;

    public override void OnImpact(SpawnableObject spawnable)
    {
        // Load resource
        SpawnableObject_Scriptable objectData = Resources.Load<SpawnableObject_Scriptable>(SpawnableObject.GetResourcePath(spawnable.childObject));

        // Set values
        Vector3 offset = s_SpawnOffset * spawnable.size;
        Vector3 initialVelocity = (Random.insideUnitSphere.normalized + Vector3.up).normalized * s_SplitLaunchSpeed;

        // Spawn new objects
        SpawnableObject spawn = SpawnableObject.Spawn(objectData, spawnable.transform, offset, spawnable.GetRigidbody(), initialVelocity, spawnable.spawnDepth);

        if (spawn != null) spawn.SetSize(spawnable.size / 2f);
    }
}
