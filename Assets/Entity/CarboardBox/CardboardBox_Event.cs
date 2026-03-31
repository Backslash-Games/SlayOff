using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class CardboardBox_Event : CardboardBox
{
    [SerializeField] private UnityEvent UE_OnHurt;
    [SerializeField] private UnityEvent UE_OnHeal;
    [Space]
    [SerializeField] private bool respawn = false;
    private Vector3 iOrigin;
    private Quaternion iRotation;

    #region Unity Events
    private void Awake()
    {
        OnHurt += (_, _, _) => UE_OnHurt.Invoke();
        OnHeal += (_, _, _) => UE_OnHeal.Invoke();

        if (respawn)
        {
            iOrigin = transform.position;
            iRotation = transform.rotation;
        }
    }
    private void OnDestroy()
    {
        OnHurt -= (_, _, _) => UE_OnHurt.Invoke();
        OnHeal -= (_, _, _) => UE_OnHeal.Invoke();
    }
    #endregion

    public override void CleanupBox()
    {
        if (respawn)
        {
            StartCoroutine(RespawnBox());
        }
        else
            base.CleanupBox();
    }
    private IEnumerator RespawnBox()
    {
        HealFull("Box.Event.Respawn");
        UnflattenBox();
        ResetVelocity();
        transform.position = iOrigin;
        transform.rotation = iRotation;
        SetConstraints(RigidbodyConstraints.FreezeAll);

        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        SetDeadState(false);
        SetConstraints(RigidbodyConstraints.None);
    }
}
