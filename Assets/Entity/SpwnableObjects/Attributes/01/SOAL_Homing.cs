using HFHandyUtils;
using System.Collections.Generic;
using UnityEngine;

public class SOAL_Homing : SpawnableObject_AttributeLogic
{
    private static float s_DistanceScaling = 225;
    private static float s_EntropyIncrease = 1;

    /// <summary>
    ///     Movement impact
    /// </summary>
    /// <param name="entity">Impacted entity</param>
    /// <param name="amount">Strength</param>
    public override void OnFixedUpdate(SpawnableObject spawnable)
    {
        // Pull the entity rigidbody
        Rigidbody rb = spawnable.GetRigidbody();
        float amount = spawnable.statblock.GetEntropy() + s_EntropyIncrease;

        // Find objects
        List<EntityData> entities = EntityManager.Instance.GetEntitiesOnOtherTeams(new EntityData.Team[] { spawnable.team, EntityData.Team.Prop });
        if (entities.Count <= 0) return;
        // !!!! Pull the closest monster -> Needs to be ambiguous based on owner
        EntityData cEntity = Mathh.GetClosestObject(entities, spawnable.transform.position, true);
        HFLogger.Log("Closest - " + cEntity.name);

        // Accelerate towards monster
        Vector3 direction = cEntity.transform.position - rb.position;
        Vector3 force = direction * amount * (s_DistanceScaling / direction.sqrMagnitude);
        rb.AddForce(force, ForceMode.Force);
        // -> Negate gravity
        rb.AddForce(-spawnable.GetGravity(), ForceMode.Acceleration);
        // Look at monster
        spawnable.transform.LookAt(cEntity.transform);
    }
}