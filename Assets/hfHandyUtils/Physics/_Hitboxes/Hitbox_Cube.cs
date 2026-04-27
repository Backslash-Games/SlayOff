using UnityEngine;

namespace HFHandyUtils.Physics
{
    /// <summary>
    ///     Defines a cube that checks for collision
    ///     <br></br>
    ///     <br>Luke Wittbrodt :: lwittbrodt87@gmail.com :: halfhand870</br>
    /// </summary>
    [System.Serializable]
    public class Hitbox_Cube : Hitbox
    {
        [Header("Cube")]
        [SerializeField] private Vector3 size = Vector3.one;
        [SerializeField] private Vector3 localEuler = Vector3.zero;

        public Hitbox_Cube(Transform parent) : base(parent) { }

        /// <summary>
        ///     Check cube collision
        /// </summary>
        /// <returns>True when colliding</returns>
        public override bool CheckCollision()
        {
            return UnityEngine.Physics.CheckBox(GetWorldPosition(), size / 2f, Quaternion.Euler(localEuler), GetLayerMask());
        }
        /// <summary>
        ///     Checks collision and outputs collided objects
        /// </summary>
        /// <param name="collided">List of colliders that the hitbox collided with</param>
        /// <returns>CheckCollision()</returns>
        public override bool GetColliding(out Collider[] collided)
        {
            collided = UnityEngine.Physics.OverlapBox(GetWorldPosition(), size / 2f, Quaternion.Euler(localEuler), GetLayerMask());
            return collided.Length != 0;
        }

        /// <summary>
        ///     Draw cube gizmos
        /// </summary>
        public override void DrawGizmos()
        {
            // Start by setting the color
            SetGizmoColor();
            Gizmos.DrawMesh(Resources.GetBuiltinResource<Mesh>("Cube.fbx"), GetWorldPosition(), Quaternion.Euler(localEuler), size);
        }


        #region Get/Set Methods
        /// <summary>
        ///     Sets the size of the box
        /// </summary>
        /// <param name="input">Scale Vector</param>
        public void SetSize(Vector3 input) { size = input; }
        /// <summary>
        ///     Gets the size of the hitbox
        /// </summary>
        /// <returns>Scale Vector</returns>
        public Vector3 GetSize() { return size; }
        /// <summary>
        ///     Sets the rotation of the box
        /// </summary>
        /// <param name="input">Euler Angles</param>
        public void SetLocalEuler(Vector3 input) { localEuler = input; }
        /// <summary>
        ///     Gets the rotation of the box
        /// </summary>
        /// <returns>Euler Angles</returns>
        public Vector3 GetLocalEuler() { return localEuler; }
        #endregion
    }
}
