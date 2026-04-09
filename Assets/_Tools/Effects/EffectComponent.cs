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
        private bool _continuous = false;
        #region Get Set Methods
        public bool GetContinuousState() { return _continuous; }
        public void SetContinuousState(bool state) { _continuous = state; }
        #endregion
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
    private string _identifier = "";
    // Duplicate Handling
    private static readonly byte s_MaxDuplicates = 15;
    private static Dictionary<string, byte> s_ActiveDupes;
    // Continuous handling
    private static List<string> s_ActiveContinuous;

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
    public virtual void Play(object target, Parameters parameters, string eventKey, EffectManager.PlayMode mode) 
    { 
        Initialize(parameters); 
    }
    // public virtual void Pause() { }
    public virtual void Stop() { CleanupEffect(); }
    #endregion
    #region Object Handling
    /// <summary>
    ///     Cleans up the effect after it is done
    /// </summary>
    private void CleanupEffect()
    {
        // Make sure item is no longer present in any list
        RemoveFromDupeDictionary(_identifier);
        RemoveFromContinuous(_identifier);

        // Destroy
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
    public void SetIdentifier(string key) { _identifier = key; }
    #endregion

    #region STATIC - General
    /// <summary>
    ///     Checks if effect is playable
    /// </summary>
    /// <param name="effectName">Name of the effect</param>
    /// <returns>True if the number of effects played hasn't gone over maximum</returns>
    public static bool isPlayable(string effectName)
    {
        // Make sure dictionary is setup
        SetupDupeDictionary();
        SetupContinuousList();

        // --> Continuous checks <--
        if (s_ActiveContinuous.Contains(effectName))
            return false;

        // --> Duplicate checks <--
        // Check if the key exists in the dictionary
        if (s_ActiveDupes.ContainsKey(effectName) && s_ActiveDupes[effectName] >= s_MaxDuplicates)
            return false;

        // Otherwise return false
        return true;
    }
    #endregion
    #region STATIC - Duplicate Handling
    /// <summary>
    ///     Sets up the active effects dictionary
    /// </summary>
    private static void SetupDupeDictionary()
    {
        if (s_ActiveDupes == null)
            s_ActiveDupes = new Dictionary<string, byte>();
    }

    /// <summary>
    ///     Adds new effect to the dictionary
    /// </summary>
    /// <param name="effectName">Name of the effect</param>
    /// <param name="count">Amount of effect added</param>
    public static void AddToDupeDictionary(string effectName, int count = 1)
    {
        // Make sure we are setup
        SetupDupeDictionary();

        // Check if the key exists in the dictionary
        if (s_ActiveDupes.ContainsKey(effectName))
        {
            s_ActiveDupes[effectName] += (byte)count;
            return;
        }

        // Add new element
        s_ActiveDupes.Add(effectName, (byte)count);
    }
    /// <summary>
    ///     Removes effect from the dictionary
    /// </summary>
    /// <param name="effectName">Name of the effect</param>
    /// <param name="count">Amount of effect removed</param>
    public static void RemoveFromDupeDictionary(string effectName, int count = 1)
    {
        // Make sure we are setup
        SetupDupeDictionary();

        // Check if the key exists in the dictionary
        if (!s_ActiveDupes.ContainsKey(effectName))
            return;

        // Remove count from key
        s_ActiveDupes[effectName] = (byte)Mathf.Clamp(s_ActiveDupes[effectName] - count, 0, s_MaxDuplicates);
    }
    #endregion
    #region STATIC - Continuous Handling
    /// <summary>
    ///     Lazy initialization for Active List
    /// </summary>
    static public void SetupContinuousList()
    {
        if (s_ActiveContinuous == null)
            s_ActiveContinuous = new List<string>();
    }

    /// <summary>
    ///     Adds an effect name to the active continuous list
    /// </summary>
    /// <param name="effectName">Effect name</param>
    static public void AddToContinuous(string effectName)
    {
        // Make sure list is setup
        SetupContinuousList();

        // Make sure name isnt in continuous yet
        if (s_ActiveContinuous.Contains(effectName))
            return;

        // Add entry to list
        s_ActiveContinuous.Add(effectName);
    }

    /// <summary>
    ///     Removes entry from continuous list
    /// </summary>
    /// <param name="effectName">Effect name</param>
    static public void RemoveFromContinuous(string effectName)
    {
        // Make sure list is setup
        SetupContinuousList();

        // Remove entry
        s_ActiveContinuous.Remove(effectName);
    }
    #endregion
}