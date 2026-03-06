using UnityEngine;

[System.Serializable]
public class Hitbox_Cube : Hitbox
{
    [Header("Cube")]
    [SerializeField] private Vector3 size = Vector3.one;
    //[SerializeField] private Vector3 up = Vector3.up;

    public Hitbox_Cube(Transform parent) : base(parent) { }

    /// <summary>
    ///     Check cube collision
    /// </summary>
    /// <returns>True when colliding</returns>
    public override bool CheckCollision()
    {
        return Physics.CheckBox(GetWorldPosition(), size / 2f, Quaternion.LookRotation(Vector3.up), GetLayerMask());
    }

    /// <summary>
    ///     Draw cube gizmos
    /// </summary>
    public override void DrawGizmos()
    {
        // Start by setting the color
        SetGizmoColor();
        // Draw the cube
        Gizmos.DrawCube(GetWorldPosition(), size);
    }
}
