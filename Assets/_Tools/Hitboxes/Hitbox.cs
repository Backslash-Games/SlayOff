using UnityEngine;

[System.Serializable]
public class Hitbox
{
    [SerializeField] private Transform parent;
    [Space]
    [SerializeField] private bool state = false;
    [SerializeField] private LayerMask mask = 0;
    [Space]
    [SerializeField] private Vector3 offset;

    /// <summary>
    ///     Constructor for hitbox
    /// </summary>
    /// <param name="parent">Associated parent</param>
    public Hitbox(Transform parent)
    {
        this.parent = parent;
    }

    /// <summary>
    ///     General method that updates the hitbox
    /// </summary>
    public void Tick() 
    {
        TickState(); // Update the state
    }

    #region State Handling
    /// <summary>
    ///     Updates the collision state
    /// </summary>
    private void TickState() { state = CheckCollision(); }
    /// <summary>
    ///     Gets the current value of state
    /// </summary>
    /// <returns>Collsision state</returns>
    public bool GetState() { return state; }
    #endregion
    #region Collision Handling
    /// <summary>
    ///     Checks if the hitbox is colliding
    /// </summary>
    /// <returns>True when a collision is found</returns>
    public virtual bool CheckCollision() { return false; }
    /// <summary>
    ///     Checks collision and outputs collided objects
    /// </summary>
    /// <param name="collided">List of colliders that the hitbox collided with</param>
    /// <returns>CheckCollision()</returns>
    public virtual bool GetColliding(out Collider[] collided) { collided = new Collider[0]; return CheckCollision(); }


    /// <summary>
    ///     Gets the current layer mask
    /// </summary>
    /// <returns>Layer mask</returns>
    public LayerMask GetLayerMask() { return mask; }
    /// <summary>
    ///     Sets the current layer mask
    /// </summary>
    /// <param name="layerMask">New Mask</param>
    public void SetLayerMask(LayerMask layerMask) { mask = layerMask; }
    #endregion

    #region Transform
    /// <summary>
    ///     Gets the current parent associated with the hitbox
    /// </summary>
    /// <returns>Current parent</returns>
    public Transform GetParent() { return parent; }
    #endregion
    #region Position
    /// <summary>
    ///     Gets the world position of the hitbox
    /// </summary>
    /// <returns>World position</returns>
    public Vector3 GetWorldPosition() { return GetOffset() + GetParent().position; }
    /// <summary>
    ///     Gets the offset of the hitbox
    /// </summary>
    /// <returns>Offset</returns>
    public Vector3 GetOffset() { return offset; }
    /// <summary>
    ///     Sets the offset of the hitbox
    /// </summary>
    /// <param name="input">Input Offset</param>
    public void SetOffset(Vector3 input) { offset = input; }
    #endregion

    #region Debug
    private static float gizmo_alpha = 0.5f;
    private static Color[] gizmo_colors = new Color[2] { new Color(0, 1, 0, gizmo_alpha), new Color(1, 0, 0, gizmo_alpha) };
    /// <summary>
    ///     Called in OnDrawGizmos, will output a debug view
    /// </summary>
    public virtual void DrawGizmos() { }



    /// <summary>
    ///     Sets the gizmo color based on state
    /// </summary>
    public void SetGizmoColor() { SetGizmoColor(GetState()); }
    /// <summary>
    ///     Sets the gizmo color based on input value
    /// </summary>
    /// <param name="value">Input</param>
    public void SetGizmoColor(bool value) { Gizmos.color = value ? gizmo_colors[0] : gizmo_colors[1]; }
    #endregion
}