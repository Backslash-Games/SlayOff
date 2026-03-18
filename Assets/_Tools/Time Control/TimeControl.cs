using System.Collections;
using UnityEngine;

public class TimeControl
{
    private static float defaultScale = 0;

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
    ///     Resets time scale to default
    /// </summary>
    public void ResetScale() { SetScale(defaultScale); }
    /// <summary>
    ///     Sets the time scale
    /// </summary>
    /// <param name="amount">New time scale</param>
    public void SetScale(float amount) 
    {
        Time.timeScale = amount;
    }
    /// <summary>
    ///     Sets the scale with an auto reset
    /// </summary>
    /// <param name="amount">Initial Amount</param>
    /// <param name="speed">Reset Speed</param>
    public void SetScale_AutoReset_OverTime(float amount, float speed) 
    {
        SetScale(amount); // Set the scale right out of the gates
        mono.StartCoroutine(AutoResetScale_OverTime(speed)); // Resets scale over time
    }
    /// <summary>
    ///     Sets the scale with an auto reset
    /// </summary>
    /// <param name="amount">Initial Amount</param>
    /// <param name="speed">Reset Speed</param>
    public void SetScale_AutoReset_Delay(float amount, float delay)
    {
        SetScale(amount); // Set the scale right out of the gates
        mono.StartCoroutine(AutoResetScale_Delay(delay)); // Resets scale over time
    }

    /// <summary>
    ///     Resets time scale over time
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
    ///     Resets time scale over time
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
