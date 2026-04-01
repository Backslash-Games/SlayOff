using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Elevator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string anim_triggerID = "Activate";

    private PlayerController player = null;

    #region Animation
    public void TriggerAnimation(int elevator_index) { GetAnimator().SetTrigger(anim_triggerID); }
    #endregion

    #region Get Methods
    public Animator GetAnimator()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        return animator;
    }
    public ArcadeModeManager GetArcadeModeManager()
    {
        if(ArcadeModeManager.Instance == null)
        {
            Debug.LogError("Please make sure the elevator is in an arcade mode scene");
            return null;
        }
        return ArcadeModeManager.Instance;
    }

    public Vector3 GetOrigin() { return transform.position; }
    /// <summary>
    ///     Gets the object offset in the current elevator
    /// </summary>
    /// <param name="cObject">Current object</param>
    /// <returns>Object offset</returns>
    public Vector3 GetVectorOffset(Vector3 point) { return point - GetOrigin(); }
    /// <summary>
    ///     Using another elevator, we get the point in the current elevator that will be mirrored to the other elevator
    /// </summary>
    /// <param name="point">Point to mirror</param>
    /// <param name="other">Other elevator</param>
    /// <returns>Position</returns>
    public Vector3 MirrorVectorPosition(Vector3 point, Elevator other)
    {
        Vector3 offset = Quaternion.Inverse(other.transform.rotation) * other.GetVectorOffset(point);
        return GetOrigin() + offset;
    }

    public PlayerController GetPlayer()
    {
        if (player == null)
            player = FindAnyObjectByType<PlayerController>();
        return player;
    }
    #endregion
}
