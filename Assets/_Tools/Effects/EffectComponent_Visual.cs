using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(VisualEffect))]
public class EffectComponent_Visual : EffectComponent
{
    #region Parameter Class
    [System.Serializable]
    public class VisualParameters : Parameters
    {
        public float length = 1;
    }
    #endregion

    [Header("Visual Components")]
    [SerializeField] private VisualEffect source;

    public override void Play(object target, Parameters parameters, string eventKey)
    {
        base.Play(target, parameters, eventKey);

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
        if (parameters is VisualParameters)
            ApplyParameters((VisualParameters)parameters);

        // Start cooldown
        effectTimer.Start();

        // Play visual
        source.SendEvent(eventKey);
    }

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
