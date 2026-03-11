using UnityEngine;

public class AnimationEvent : MonoBehaviour
{
    [SerializeField] private string id = "New Animation Event";
    public bool flag = false;

    /// <summary>
    ///     Gets the state of the flag
    /// </summary>
    /// <returns>Flag State</returns>
    public bool GetFlagState() { return flag; }
    /// <summary>
    ///     Sets the state of the flag
    /// </summary>
    /// <param name="value">New State</param>
    public void SetFlagState(bool value) { flag = value; }
}
