using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.VFX;

public class EffectManager : MonoBehaviour
{
    #region Play Type
    public enum PlayMode { Impulse, Continuous };
    #endregion

    #region Singleton
    // Singleton
    private static EffectManager _instance;
    public static EffectManager Instance { get { return _instance; } }

    private void CreateSingleton()
    {
        // -> Pulled from Out on the Red Sea
        // Checks if the instance of object is first of its type
        // If object is not unique, destroy current instance
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        // Declares this script as current
        else
        {
            _instance = this;
        }
    }
    #endregion
    #region Unity Methods
    private void Awake()
    {
        CreateSingleton();
    }
    #endregion

    #region Generic
    #region Play
    /// <summary>
    ///     Plays an effect from an Effect Library and state
    /// </summary>
    /// <typeparam name="TEnum">System Enum</typeparam>
    /// <typeparam name="TContent">Class</typeparam>
    /// <param name="library">Effect Library</param>
    /// <param name="state">Enum State</param>
    public void Play<TEnum, TContent, TParams>(EffectLibrary<TEnum, TContent, TParams> library, TEnum state, PlayMode mode = PlayMode.Impulse)
        where TEnum : System.Enum
        where TContent : class
        where TParams : EffectComponent.Parameters
    {
        try
        {
            // Pull our generic entry
            EffectLibraryEntry<TEnum, TContent> genericEntry = library.GetEntryFromState(state);
            TContent content = genericEntry.GetContent();
            string eventKey = genericEntry.GetEventKey();

            // Compare types
            // -> Audio
            if (content is AudioClip)
                // Cast information
                Play<EffectComponent_Audio, TParams>(content, library.GetParams(), eventKey, mode);
            // -> Visual
            else if (content is VisualEffectAsset)
                // Cast information
                Play<EffectComponent_Visual, TParams>(content, library.GetParams(), eventKey, mode);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"EffectManager.Play -> {e}");
        }
    }

    /// <summary>
    ///     Plays a target (playable) object on an effect component
    /// </summary>
    /// <typeparam name="TComponent">Effect component type</typeparam>
    /// <typeparam name="TParam">Effect component parameter type</typeparam>
    /// <param name="target">Target playable</param>
    /// <param name="parameters">Pass through parameters</param>
    /// <param name="eventKey">Event key</param>
    private void Play<TComponent, TParam>(object target, TParam parameters, string eventKey, PlayMode mode = PlayMode.Impulse)
       where TComponent : EffectComponent
       where TParam : EffectComponent.Parameters
    {
        int hash = target.GetHashCode();
        // Check if the content is playable
        if (!EffectComponent.isPlayable(hash.ToString()))
            return;

        // Summon a new audio effect clip
        TComponent effectComponent = BuildEffectComponent<TComponent>($"{hash}");
        // Check if we are null
        if (effectComponent == null)
        {
            Destroy(effectComponent.gameObject);
            return;
        }

        // Set up dictionary id
        effectComponent.SetIdentifier(hash.ToString());
        // -> Procress in effect component
        // Add to dupe dictionary on impulse
        if (mode.Equals(PlayMode.Impulse))
            EffectComponent.AddToDupeDictionary(hash.ToString());
        // Add to continuous list on continuous
        else if (mode.Equals(PlayMode.Continuous))
            EffectComponent.AddToContinuous(hash.ToString());

        // Play effect component
        effectComponent.Play(target, parameters, eventKey, mode);
    }
    #endregion
    #region Stop
    public void Stop<TEnum, TContent, TParams>(EffectLibrary<TEnum, TContent, TParams> library, TEnum state)
        where TEnum : System.Enum
        where TContent : class
        where TParams : EffectComponent.Parameters
    {
        try
        {
            EffectLibraryEntry<TEnum, TContent> genericEntry = library.GetEntryFromState(state);
            EffectComponent component = FindComponent(genericEntry.GetContent().GetHashCode());
            component.Stop();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"EffectManager.Stop -> {e}");
        }
    }
    #endregion
    #endregion
    #region Effect Component Handling
    private T BuildEffectComponent<T>(string name) where T : Component
    {
        // Build object
        GameObject buildTarget = new GameObject(name, new System.Type[] { typeof(T) });
        // Parent properly
        buildTarget.transform.parent = transform;

        return buildTarget.GetComponent<T>();
    }

    private EffectComponent FindComponent(int hash)
    {
        // Use a hash code to find the component in children
        try
        {
            return transform.Find(hash.ToString()).gameObject.GetComponent<EffectComponent>();
        }
        catch
        {
            return null;
        }
    }
    #endregion

    #region Audio
    [Header("Audio Components")]
    [SerializeField] private AudioMixer mixer;
    #region Set Methods
    /// <summary>
    ///     Sets all of the mixer volume based on input
    /// </summary>
    /// <param name="master">Master Volume</param>
    /// <param name="music">Music Volume</param>
    /// <param name="sound_effects">Sound Effect Volume</param>
    public void SetMixerVolumes(float master, float music, float sound_effects)
    {
        SetMixerVolume("Master", master);
        SetMixerVolume("Music", music);
        SetMixerVolume("Sound Effects", sound_effects);
    }

    /// <summary>
    ///     Set a specific aspect of the mixer
    /// </summary>
    /// <param name="tag">Mixer tag</param>
    /// <param name="value">Mixer volume</param>
    private void SetMixerVolume(string tag, float value)
    {
        // Error check
        if (mixer == null)
            return;

        // Set value - TEMPORARY
        if (value > 0.01f)
            mixer.SetFloat(tag, (1 - value) * -30);
        else
            mixer.SetFloat(tag, -80);
    }
    #endregion
    #endregion
}
