using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

[System.Serializable]
public class EffectComponent : MonoBehaviour
{
    #region Parameter Class
    [System.Serializable]
    public class Parameters 
    {
        public Transform origin;
    }
    #endregion
    public enum EffectState { None, Initialized, Started, Paused, Stopped, Completed };
    
    [Header("Effect Variables")]
    public EffectState effectState = EffectState.None;
    #region Effect State Events
    public delegate void EffectStateChanged();
    public event EffectStateChanged OnInitialized;
    public event EffectStateChanged OnStarted;
    public event EffectStateChanged OnPaused;
    public event EffectStateChanged OnStopped;
    public event EffectStateChanged OnCompleted;
    #endregion
    public Cooldown effectTimer;

    // Private variables
    private string _dictionaryKey = "";
    private static readonly byte s_MaxDuplicates = 15;
    private static Dictionary<string, byte> s_ActiveEffects;

    #region Unity Methods
    private void Awake()
    {
        effectTimer = new Cooldown(this, 0, 1);
        effectTimer.OnCooldownSuccess += CleanupEffect;
    }
    #endregion

    #region Sequencing
    public virtual void Initialize(Parameters p)
    {
        // Apply basic parameters
        transform.position = p.origin.position;
    }
    public virtual void Play(object target, Parameters parameters, string eventKey) { }
    // public virtual void Pause() { }
    // public virtual void Stop() { }
    #endregion
    #region Object Handling
    /// <summary>
    ///     Cleans up the effect after it is done
    /// </summary>
    private void CleanupEffect()
    {
        RemoveFromDictionary(_dictionaryKey);
        Destroy(gameObject);
    }
    #endregion

    #region Effect State Handling
    /// <summary>
    ///     Compares another effect state with our current
    /// </summary>
    /// <param name="other">Other effect state</param>
    /// <returns>True if states are equal</returns>
    public bool CompareEffectState(EffectState other) { return other.Equals(effectState); }

    /// <summary>
    ///     Sets our effect state to a new state, runs events if changed
    /// </summary>
    /// <param name="other">State to move to</param>
    public void SetEffectState(EffectState other)
    {
        // Check if other does not equal our current
        if (CompareEffectState(other))
            return;

        // Set our new state
        effectState = other;

        // Trigger event
        InvokeEffectStateChanged();
    }

    /// <summary>
    ///     Triggers the effect state event based on our current effect state
    /// </summary>
    private void InvokeEffectStateChanged()
    {
        switch (effectState)
        {
            case EffectState.Initialized:
                OnInitialized?.Invoke();
                break;
            case EffectState.Started:
                OnStarted?.Invoke();
                break;
            case EffectState.Paused:
                OnPaused?.Invoke();
                break;
            case EffectState.Stopped:
                OnStopped?.Invoke();
                break;
            case EffectState.Completed:
                OnCompleted?.Invoke();
                break;
        }
    }
    #endregion
    #region Set Methods
    public void SetDictionaryKey(string key) { _dictionaryKey = key; }
    #endregion

    #region STATIC - Dictionary Handling
    /// <summary>
    ///     Sets up the active effects dictionary
    /// </summary>
    private static void SetupDictionary()
    {
        if (s_ActiveEffects == null)
            s_ActiveEffects = new Dictionary<string, byte>();
    }

    /// <summary>
    ///     Adds new effect to the dictionary
    /// </summary>
    /// <param name="effectName">Name of the effect</param>
    /// <param name="count">Amount of effect added</param>
    public static void AddToDictionary(string effectName, int count = 1)
    {
        // Make sure we are setup
        SetupDictionary();

        // Check if the key exists in the dictionary
        if (s_ActiveEffects.ContainsKey(effectName))
        {
            s_ActiveEffects[effectName] += (byte)count;
            return;
        }

        // Add new element
        s_ActiveEffects.Add(effectName, (byte)count);
    }
    /// <summary>
    ///     Removes effect from the dictionary
    /// </summary>
    /// <param name="effectName">Name of the effect</param>
    /// <param name="count">Amount of effect removed</param>
    public static void RemoveFromDictionary(string effectName, int count = 1)
    {
        // Make sure we are setup
        SetupDictionary();

        // Check if the key exists in the dictionary
        if (!s_ActiveEffects.ContainsKey(effectName))
            return;

        // Remove count from key
        s_ActiveEffects[effectName] = (byte)Mathf.Clamp(s_ActiveEffects[effectName] - count, 0, s_MaxDuplicates);
    }

    /// <summary>
    ///     Checks if effect is playable
    /// </summary>
    /// <param name="effectName">Name of the effect</param>
    /// <returns>True if the number of effects played hasn't gone over maximum</returns>
    public static bool isPlayable(string effectName)
    {
        // Make sure we are setup
        SetupDictionary();

        // Check if the key exists in the dictionary
        if (!s_ActiveEffects.ContainsKey(effectName))
            return true;

        // Check if the active effects are less than the max allowed
        if (s_ActiveEffects[effectName] < s_MaxDuplicates)
            return true;

        // Otherwise return false
        return false;
    }
    #endregion
}