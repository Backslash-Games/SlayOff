using System.Collections.Generic;
using UnityEngine;

public class SOAL_Inhaling : SpawnableObject_AttributeLogic
{
    private static float s_InhaleSizeScale = 25;

    public override void OnFixedUpdate(SpawnableObject spawnable)
    {
        List<EntityData> entities = EntityManager.Instance.GetEntitiesOnOtherTeams(spawnable.team);
        // Check for a pull
        foreach (var entity in entities)
        {
            if(Vector3.Distance(spawnable.transform.position, entity.transform.position) <= spawnable.statblock.GetSize() * s_InhaleSizeScale)
            {
                // Pull
                entity.ApplyKnockback(spawnable.transform.position - entity.transform.position, 1 + (spawnable.GetStatblock().GetAttack() * 0.25f) + spawnable.statblock.GetEntropy());
            }
        }
    }
}
