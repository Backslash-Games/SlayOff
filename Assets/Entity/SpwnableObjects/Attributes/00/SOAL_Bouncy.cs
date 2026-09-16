using UnityEngine;

public class SOAL_Bouncy : SpawnableObject_AttributeLogic
{
    private static PhysicsMaterial s_BouncyMaterial = null;

    public override void OnInitialization(SpawnableObject spawnable)
    {
        // Increase object bounciness
        if (s_BouncyMaterial == null) s_BouncyMaterial = Resources.Load<PhysicsMaterial>("Physics Materials/SO_Bouncy");
        spawnable.GetCollision().material = s_BouncyMaterial;
    }
}
