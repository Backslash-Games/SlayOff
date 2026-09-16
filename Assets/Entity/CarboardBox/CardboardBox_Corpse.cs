using UnityEngine;

public class CardboardBox_Corpse : CardboardBox
{
    [SerializeField] private float fling_strength;

    protected override void OnEnable()
    {
        base.OnEnable();
        ApplyForce(Camera.main.transform.forward, fling_strength, ForceMode.Impulse, "CardboardBox.Corpse.Enabled");
    }
    protected override void OnDisable()
    {
        GetRigidbody().linearVelocity = Vector3.zero;
        base.OnDisable();
    }
}
