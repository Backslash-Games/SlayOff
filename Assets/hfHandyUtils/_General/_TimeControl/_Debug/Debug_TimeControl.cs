using UnityEngine;

namespace HFHandyUtils
{
    /// <summary>
    ///     Debug script for TimeControl.cs... Should only be used in Unity Inspector
    ///     <br></br>
    ///     <br>Luke Wittbrodt :: lwittbrodt87@gmail.com :: halfhand870</br>
    /// </summary>
    public class Debug_TimeControl : MonoBehaviour
    {
        [SerializeField] private TimeControl timeControl = null;
        [SerializeField] private float timeScale = 1;

        // Start is called on the first frame
        private void Start()
        {
            timeControl = new TimeControl(this);
        }
        // Update is called once per frame
        void Update()
        {
            timeControl.SetScale(timeScale);
        }
    }
}