using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField] private bool active = true;
    [SerializeField] private Transform target = null;

    // Update is called once per frame
    void Update() { UpdateBillboard(); }



    /// <summary>
    ///     Updates billboarding if active
    /// </summary>
    private void UpdateBillboard()
    {
        // Check if we are billboarding
        if (!active)
            return;
        // Check if target is null
        if (target == null)
            return;

        // Make canvas object face the main camera
        target.LookAt(Camera.main.transform, Vector3.down);
    }



    /// <summary>
    ///     Initialzes all components of the billboard
    /// </summary>
    /// <param name="state">Initial State</param>
    /// <param name="target">Initial Target</param>
    public void Initialize(bool state, Transform target) { SetState(state); SetTarget(target); }
    /// <summary>
    ///     Sets the active state
    /// </summary>
    /// <param name="state">New state</param>
    public void SetState(bool state) { active = state; }
    /// <summary>
    ///     Sets the active target
    /// </summary>
    /// <param name="state">New target</param>
    public void SetTarget(Transform target) { this.target = target; }
}
