using UnityEngine;

public class CardboardBox_Corpse : CardboardBox
{
    [SerializeField] private float fling_strength;

    public override void OnEnabled()
    {
        base.OnEnabled();
        ApplyForce(transform.position - Camera.main.transform.position, fling_strength, ForceMode.Impulse, "CardboardBox.Corpse.Enabled");
    }
}
