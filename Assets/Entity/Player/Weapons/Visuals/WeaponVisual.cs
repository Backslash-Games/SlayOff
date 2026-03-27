using UnityEngine;

public class WeaponVisual : MonoBehaviour
{
    [SerializeField] private Transform nozzle_transform = null;
    [SerializeField] private Vector3 hit_position = Vector3.zero;

    private RaycastHit hit_information;

    private bool active = false;

    public delegate void VisualState();
    public event VisualState OnInitialization;
    public event VisualState OnHitSet;

    #region Initialization
    /// <summary>
    ///     Initializes the weapon visual with a start and end point. Must be called before weapon visuals can work
    /// </summary>
    /// <param name="nozzle_position">Starting world position</param>
    /// <param name="hit_position">Ending world position</param>
    public void Initialize(Transform nozzle_transform, Vector3 hit_position) 
    {
        this.nozzle_transform = nozzle_transform;
        this.hit_position = hit_position;

        OnInitialization?.Invoke();

        active = true;
    }
    #endregion
    #region Unity Methods
    private void LateUpdate()
    {
        if (!isActive())
            return;

        OnMove();
    }
    #endregion

    #region Events
    /// <summary>
    ///     Defines logic for movement
    /// </summary>
    public virtual void OnMove() { }
    #endregion

    #region Get Methods
    public Transform GetNozzle() { return nozzle_transform; }
    public Vector3 GetHitPosition() { return hit_position; }
    public RaycastHit GetHit() { return hit_information; }
    public bool isActive() { return active; }
    #endregion
    #region Set Methods
    public void SetHitInformation(RaycastHit input) { hit_information = input; OnHitSet?.Invoke(); }
    #endregion
}
