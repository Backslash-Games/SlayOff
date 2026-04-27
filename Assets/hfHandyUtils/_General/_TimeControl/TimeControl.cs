using System.Collections;
using UnityEngine;

namespace HFHandyUtils
{
    /// <summary>
    ///     Organized method of controlling Unity Time
    ///     <br></br>
    ///     <br>Luke Wittbrodt :: lwittbrodt87@gmail.com :: halfhand870</br>
    /// </summary>
    public class TimeControl
    {
        // Holds a reference to the timescale of the project
        private static float defaultScale = 0;
        // Holds a reference to the associated monobehaviour script. Used for coroutines
        private MonoBehaviour mono = null;
        public TimeControl(MonoBehaviour mono)
        {
            // Set the private scale if it hasnt been set
            if (defaultScale == 0)
                defaultScale = Time.timeScale;
            // Set the mono behaviour
            this.mono = mono;
        }

        #region Time Scale Adjustments
        /// <summary>
        ///     Instantly resets time scale to default
        /// </summary>
        public void ResetScale() { SetScale(defaultScale); }
        /// <summary>
        ///     Sets Unity time scale
        /// </summary>
        /// <param name="amount">New time scale</param>
        public void SetScale(float amount)
        {
            Time.timeScale = amount;
        }
        /// <summary>
        ///     Sets Unity time scale with a reset fade over time frame
        /// </summary>
        /// <param name="amount">Initial Amount</param>
        /// <param name="speed">Reset Speed</param>
        public void SetScale_AutoReset_OverTime(float amount, float speed)
        {
            SetScale(amount); // Set the scale right out of the gates
            mono.StartCoroutine(AutoResetScale_OverTime(speed)); // Resets scale over time
        }
        /// <summary>
        ///     Sets Unity time scale with a force reset after delay
        /// </summary>
        /// <param name="amount">Initial Amount</param>
        /// <param name="speed">Reset Speed</param>
        public void SetScale_AutoReset_Delay(float amount, float delay)
        {
            SetScale(amount); // Set the scale right out of the gates
            mono.StartCoroutine(AutoResetScale_Delay(delay)); // Resets scale over time
        }

        /// <summary>
        ///     Fade reset over time frame
        /// </summary>
        /// <param name="speed">Reset speed</param>
        /// <returns>Wait</returns>
        private IEnumerator AutoResetScale_OverTime(float speed)
        {
            // Move time scale towards default
            while (Time.timeScale != defaultScale)
            {
                yield return new WaitForEndOfFrame(); // Stalls til end of frame
                Time.timeScale = Mathf.MoveTowards(Time.timeScale, defaultScale, Time.deltaTime * speed * (defaultScale / Time.timeScale));
            }
        }

        /// <summary>
        ///     Force reset after delay
        /// </summary>
        /// <param name="speed">Reset speed</param>
        /// <returns>Wait</returns>
        private IEnumerator AutoResetScale_Delay(float delay)
        {
            yield return new WaitForSecondsRealtime(delay); // Delays
            Time.timeScale = defaultScale;
        }
        #endregion

        #region String Methods
        public override string ToString()
        {
            string output = "";

            output += $"Time Scale: {Time.timeScale}\n";

            return output;
        }
        #endregion
    }
}