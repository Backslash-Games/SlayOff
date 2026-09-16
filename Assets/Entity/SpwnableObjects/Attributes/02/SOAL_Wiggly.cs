using HFHandyUtils;
using UnityEngine;

public class SOAL_Wiggly : SpawnableObject_AttributeLogic
{
    private static readonly float s_WiggleAmplitude = 10;
    /// <summary>
    ///     Movement impact
    /// </summary>
    /// <param name="entity">Impacted entity</param>
    /// <param name="amount">Strength</param>
    public override void OnFixedUpdate(SpawnableObject spawnable)
    {
        // Pull the rigid body
        Rigidbody rb = spawnable.GetRigidbody();
        float amount = spawnable.statblock.GetEntropy() + 1;

        // Establish the direction
        Vector3 direction = spawnable.transform.right * Mathf.Cos(spawnable.aliveTime * s_WiggleAmplitude);
        direction *= amount;

        // Apply force
        rb.AddForce(direction, ForceMode.VelocityChange);
    }
}
