using UnityEngine;

[System.Serializable]
public class Hitbox_Sphere : Hitbox
{
    [Header("Sphere")]
    [SerializeField] private float radius = 1;

    public Hitbox_Sphere(Transform parent) : base(parent) { }

    /// <summary>
    ///     Check sphere collision
    /// </summary>
    /// <returns>True when colliding</returns>
    public override bool CheckCollision()
    {
        return Physics.CheckSphere(GetWorldPosition(), radius, GetLayerMask());
    }
    /// <summary>
    ///     Checks collision and outputs collided objects
    /// </summary>
    /// <param name="collided">List of colliders that the hitbox collided with</param>
    /// <returns>CheckCollision()</returns>
    public override bool GetColliding(out Collider[] collided)
    {
        return base.GetColliding(out collided);
    }

    /// <summary>
    ///     Draw sphere gizmos
    /// </summary>
    public override void DrawGizmos()
    {
        // Start by setting the color
        SetGizmoColor();
        // Draw the sphere
        Gizmos.DrawSphere(GetWorldPosition(), radius);
    }
}
