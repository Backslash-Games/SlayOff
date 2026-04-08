using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.VFX;

public class EffectManager : MonoBehaviour
{
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
    /// <summary>
    ///     Plays an effect from an Effect Library and state
    /// </summary>
    /// <typeparam name="TEnum">System Enum</typeparam>
    /// <typeparam name="TContent">Class</typeparam>
    /// <param name="library">Effect Library</param>
    /// <param name="state">Enum State</param>
    public void Play<TEnum, TContent, TParams>(EffectLibrary<TEnum, TContent, TParams> library, TEnum state)
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
                Play<EffectComponent_Audio, TParams>(content, library.GetParams(), eventKey);
            // -> Visual
            else if (content is VisualEffectAsset)
                // Cast information
                Play<EffectComponent_Visual, TParams>(content, library.GetParams(), eventKey);
        }
        catch
        {
            Debug.LogWarning("Could not find library entry with state name " + state.ToString());
        }
    }
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
    #endregion


    private void Play<TComponent, TParam>(object target, TParam parameters, string eventKey)
        where TComponent : EffectComponent
        where TParam : EffectComponent.Parameters
    {
        int hash = target.GetHashCode();
        // Check if the content is playable
        if (!EffectComponent.isPlayable(hash.ToString()))
            return;

        // Summon a new audio effect clip
        TComponent effectComponent = BuildEffectComponent<TComponent>($"{typeof(TComponent)} - {hash}");
        // Check if we are null
        if(effectComponent == null)
        {
            Destroy(effectComponent.gameObject);
            return;
        }

        // Set up dictionary id
        effectComponent.SetDictionaryKey(hash.ToString());
        EffectComponent.AddToDictionary(hash.ToString());

        // Play effect component
        effectComponent.Play(target, parameters, eventKey);
    }

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
            mixer.SetFloat("Music", (1 - value) * -30);
        else
            mixer.SetFloat("Music", -80);
    }
    #endregion
    #endregion
}
