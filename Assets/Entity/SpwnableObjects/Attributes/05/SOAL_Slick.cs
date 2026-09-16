using UnityEngine;

public class SOAL_Slick : SpawnableObject_AttributeLogic
{
    private static PhysicsMaterial s_SlickMaterial = null;

    public override void OnInitialization(SpawnableObject spawnable)
    {
        // Increase object bounciness
        if (s_SlickMaterial == null) s_SlickMaterial = Resources.Load<PhysicsMaterial>("Physics Materials/SO_Slick");
        spawnable.GetCollision().material = s_SlickMaterial;
    }
}
