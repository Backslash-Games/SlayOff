using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class EffectComponent_Audio : EffectComponent
{

    #region Parameter Class
    [System.Serializable]
    public class AudioParameters : Parameters 
    {
        public AudioMixerGroup output;
        [Space]
        public bool randomizePitch = true;
        public bool spatial = true;
        [Space]
        [Range(0, 1)] public float volume = 1;
    }
    #endregion

    [Header("Audio Components")]
    [SerializeField] private AudioSource source;

    private static readonly Vector2 s_PitchRange = new Vector2(0.7f, 1.3f);

    public override void Play(object target, Parameters parameters, string eventKey, EffectManager.PlayMode mode)
    {
        base.Play(target, parameters, eventKey, mode);
        // Try to parse the target
        if (target is not AudioClip)
        {
            Debug.LogError("Attempted to play non audio on audio effect component");
            return;
        }
        
        // Set up initial
        AudioClip clip = (AudioClip)target;
        GetAudioSource(); // Ensure audio source is set properly
        // Apply parameters
        if(parameters is AudioParameters) ApplyParameters((AudioParameters)parameters);

        // Set cooldown time
        effectTimer.SetRates(clip.length, 1);

        // Branch based on play mode
        if (mode.Equals(EffectManager.PlayMode.Impulse)) InitImpulse();
        else if (mode.Equals(EffectManager.PlayMode.Continuous)) InitContinuous();

        // Play audio
        source.clip = clip;
        source.Play();
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
        // Ensure source is set up to loop
        source.loop = true;
    }
    #endregion
    
    #region Parameter Handling
    /// <summary>
    ///     Container method to cleanup parameter handling
    /// </summary>
    /// <param name="ap">Input parameters</param>
    private void ApplyParameters(AudioParameters ap)
    {
        // Set mixer group
        source.outputAudioMixerGroup = ap.output;

        // Handle pitch
        if (ap.randomizePitch) source.pitch = Random.Range(s_PitchRange.x, s_PitchRange.y);
        // Handle Spatial Audio
        source.spatialBlend = ap.spatial ? 0.7f : 0;

        // Handle voume
        source.volume = ap.volume;
    }
    #endregion
    #region Get Methods
    /// <summary>
    ///     Pulls the audio source, ensures no null
    /// </summary>
    /// <returns>Audio Source</returns>
    private AudioSource GetAudioSource()
    {
        if (source == null)
            source = GetComponent<AudioSource>();
        if (source == null)
            source = gameObject.AddComponent<AudioSource>();
        return source;
    }
    #endregion
}
