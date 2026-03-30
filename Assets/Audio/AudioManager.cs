using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // Singleton
    private static AudioManager _instance;
    public static AudioManager Instance { get { return _instance; } }

    [Header("Sound Effects")]
    [SerializeField] private GameObject audioEffectPrefab;

    #region Singleton
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

    #region Tools
    public void PlayAudio(AudioClip clip, Vector3 position, bool randomize_pitch = true, bool disable_spatial = false)
    {
        // Check if the clip is playable
        if (!AudioEffect.isPlayable(clip.name))
            return;

        // Summon a new audio effect clip
        GameObject spawned = Instantiate(audioEffectPrefab, position, Quaternion.identity, transform);
        spawned.name = $"Audio Effect - {clip.name}";
        AudioEffect effect = spawned.GetComponent<AudioEffect>();

        // Check if we are null
        if(effect == null)
        {
            Destroy(spawned);
            return;
        }

        // Apply the clip to the effect
        effect.Play(clip, randomize_pitch, disable_spatial);
    }
    #endregion
}
