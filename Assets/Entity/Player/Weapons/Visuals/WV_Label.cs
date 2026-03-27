using UnityEngine;

public class WV_Label : WeaponVisual
{
    [SerializeField] private Vector3 target_position = Vector3.zero;
    [SerializeField] private float move_speed = 35;
    [SerializeField] private float scale_speed = 5;
    [SerializeField] private float destructionThreshold = 0.01f;
    [Space]
    [SerializeField] private LayerMask ignore_layer;

    private void Awake()
    {
        OnInitialization += SetTarget;
        OnInitialization += SetInitialPosition;
    }
    private void OnDestroy()
    {
        OnInitialization -= SetTarget;
        OnInitialization -= SetInitialPosition;
    }

    public override void OnMove()
    {
        transform.position = Vector3.MoveTowards(transform.position, target_position, Time.deltaTime * move_speed);
        transform.localScale = Vector3.MoveTowards(transform.localScale, transform.localScale + Vector3.one, Time.deltaTime * scale_speed);

        // Check if we need to die
        if (Vector3.Distance(transform.position, target_position) <= destructionThreshold)
            Destroy(transform.gameObject);
    }

    private void SetTarget()
    {
        target_position = (Camera.main.transform.forward * 15) + GetHitPosition();
    }
    private void SetInitialPosition()
    {
        if (HitIgnored())
            return;

        transform.position = GetHitPosition() - Camera.main.transform.forward;
    }

    private bool HitIgnored() { return GetHit().transform == null || ((1 << GetHit().transform.gameObject.layer) & ignore_layer) != 0; }
}
