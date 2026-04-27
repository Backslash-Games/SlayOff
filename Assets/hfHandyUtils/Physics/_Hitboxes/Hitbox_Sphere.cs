using UnityEngine;

namespace HFHandyUtils.Physics
{
    /// <summary>
    ///     Defines a sphere that checks for collision
    ///     <br></br>
    ///     <br>Luke Wittbrodt :: lwittbrodt87@gmail.com :: halfhand870</br>
    /// </summary>
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
            return UnityEngine.Physics.CheckSphere(GetWorldPosition(), radius, GetLayerMask());
        }
        /// <summary>
        ///     Checks collision and outputs collided objects
        /// </summary>
        /// <param name="collided">List of colliders that the hitbox collided with</param>
        /// <returns>CheckCollision()</returns>
        public override bool GetColliding(out Collider[] collided)
        {
            collided = UnityEngine.Physics.OverlapSphere(GetWorldPosition(), radius, GetLayerMask());
            return collided.Length != 0;
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

        #region Get/Set Methods
        /// <summary>
        ///     Gets the radius of the sphere
        /// </summary>
        /// <returns>Radius</returns>
        public void SetRadius(float input) { radius = input; }
        /// <summary>
        ///     Gets the radius of the sphere
        /// </summary>
        /// <returns>Radius</returns>
        public float GetRadius() { return radius; }
        #endregion
    }
}