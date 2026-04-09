using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(VisualEffect))]
public class EffectComponent_Visual : EffectComponent
{
    private string _eventKey = "";

    #region Parameter Class
    [System.Serializable]
    public class VisualParameters : Parameters
    {
        public float length = 1;
    }
    #endregion

    [Header("Visual Components")]
    [SerializeField] private VisualEffect source;

    public override void Play(object target, Parameters parameters, string eventKey, EffectManager.PlayMode mode)
    {
        base.Play(target, parameters, eventKey, mode);

        // Try to parse the target
        if (target is not VisualEffectAsset)
        {
            Debug.LogError("Attempted to play non visual on visual effect component");
            return;
        }

        // Setup
        VisualEffectAsset asset = (VisualEffectAsset)target;
        GetVisualSource(); // Ensure visual source is set properly
        source.visualEffectAsset = asset;
        // Apply parameters
        if (parameters is VisualParameters) ApplyParameters((VisualParameters)parameters);
        // Set event key
        _eventKey = eventKey;

        // Branch based on play mode
        if (mode.Equals(EffectManager.PlayMode.Impulse)) InitImpulse();
        else if (mode.Equals(EffectManager.PlayMode.Continuous)) InitContinuous();

        // Play visual
        source.SendEvent(_eventKey);
    }

    #region Initialization
    /// <summary>
    ///     Container method to cleanup play mode handling
    /// </summary>
    private void InitImpulse()
    {
        // Just start the cooldown
        effectTimer.Start();
    }
    /// <summary>
    ///     Container method to cleanup play mode handling
    /// </summary>
    private void InitContinuous()
    {
        // Clear list
        effectTimer.RemoveAllListeners();
        // Ensure source is set up to loop
        effectTimer.OnCooldownSuccess += () => effectTimer.Start();
        effectTimer.OnCooldownSuccess += () => source.SendEvent(_eventKey);
    }
    #endregion

    #region Parameter Handling
    private void ApplyParameters(VisualParameters vp)
    {
        effectTimer.SetRates(vp.length, 1);
    }
    #endregion
    #region Get Methods
    /// <summary>
    ///     Pulls the audio source, ensures no null
    /// </summary>
    /// <returns>Audio Source</returns>
    private VisualEffect GetVisualSource()
    {
        if (source == null)
            source = GetComponent<VisualEffect>();
        if (source == null)
            source = gameObject.AddComponent<VisualEffect>();
        return source;
    }
    #endregion
}
