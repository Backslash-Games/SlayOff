using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class EffectComponent_Audio : EffectComponent
{
    #region Parameter Class
    [System.Serializable]
    public class AudioParameters : Parameters 
    {
        public bool randomizePitch = true;
        public bool spatial = true;
    }
    #endregion

    [Header("Audio Components")]
    [SerializeField] private AudioSource source;

    private static readonly Vector2 s_PitchRange = new Vector2(0.7f, 1.3f);

    public override void Play(object target, Parameters parameters, string eventKey)
    {
        base.Play(target, parameters, eventKey);

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
        if(parameters is AudioParameters)
            ApplyParameters((AudioParameters)parameters);

        // Set up and start cooldown
        effectTimer.SetRates(clip.length, 1);
        effectTimer.Start();

        // Play audio
        source.clip = clip;
        source.Play();
    }

    #region Parameter Handling
    private void ApplyParameters(AudioParameters ap)
    {
        // Handle pitch
        if (ap.randomizePitch)
            source.pitch = Random.Range(s_PitchRange.x, s_PitchRange.y);
        // Handle Spatial Audio
        source.spatialize = ap.spatial;
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
