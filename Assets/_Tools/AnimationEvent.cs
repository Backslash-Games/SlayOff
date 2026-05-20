using UnityEngine;

namespace HFHandyUtils.Animation
{
    /// <summary>
    ///     Exposes a boolean to signal a timed animation event
    ///     <br></br>
    ///     <br>Luke Wittbrodt :: lwittbrodt87@gmail.com :: halfhand870</br>
    /// </summary>
    public class AnimationEvent : MonoBehaviour
    {
        [SerializeField] private string id = "New Animation Event";
        public bool flag = false;

        /// <summary>
        ///     Gets the state of flag
        /// </summary>
        /// <returns>Flag State</returns>
        public bool GetState() { return flag; }
        /// <summary>
        ///     Sets the state of flag
        /// </summary>
        /// <param name="value">New State</param>
        public void SetState(bool value) { flag = value; }
    }
}