using UnityEngine;

public class SOAL_Pogo : SpawnableObject_AttributeLogic
{
    private static readonly float s_EntropyIncrease = 1.5f;

    public override void OnImpact(SpawnableObject spawnable)
    {
        // Get the collision point
        Rigidbody rb = spawnable.GetRigidbody();
        Vector3 direction = rb.linearVelocity;
        float speed = direction.magnitude;
        if (speed <= 0) speed = 1;
        // Check if we have impacted
        if(spawnable.GetImpactPoint_Velocity(out RaycastHit hit))
        {
            // Apply a bounce
            rb.linearVelocity = Vector3.zero;
            spawnable.ApplyForce(Vector3.Reflect(direction, hit.normal).normalized + hit.normal, speed * (spawnable.statblock.GetEntropy() + s_EntropyIncrease), ForceMode.Impulse, "SOAL_Pogo");
        }
        else
        {
            // Apply a bounce
            rb.linearVelocity = Vector3.zero;
            spawnable.ApplyForce(Vector3.Reflect(Camera.main.transform.forward, Vector3.up).normalized, speed * (spawnable.statblock.GetEntropy() + s_EntropyIncrease), ForceMode.Impulse, "SOAL_Pogo");
        }
    }
}
