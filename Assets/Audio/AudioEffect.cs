using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioEffect : MonoBehaviour
{
    [SerializeField] private AudioSource source;
    [Space]
    [SerializeField] private AudioClip clip;

    private bool played = false;

    private Cooldown clip_timer;

    private static readonly Vector2 pitch_range = new Vector2(0.6f, 1.4f);
    
    private static readonly byte max_duplicates = 15;
    private static Dictionary<string, byte> active_clips;

    #region Playing
    /// <summary>
    ///     Plays an audio clip
    /// </summary>
    public void Play(AudioClip clip, bool randomize_pitch = true, bool disable_spatial = false)
    {
        // Check if the clip is null
        if(clip == null)
        {
            Debug.LogError("Attempted to play null audio clip");
            return;
        }

        // Set up the clip
        this.clip = clip;
        GetAudioSource().clip = clip;
        // Setup cooldown
        clip_timer = new Cooldown(this, clip.length, 1);
        clip_timer.OnCooldownSuccess += DestroySelf;
        // Add clip to dictionary
        AddToDictionary(clip.name);

        // Randomize the pitch
        if (randomize_pitch)
            GetAudioSource().pitch = Random.Range(pitch_range.x, pitch_range.y);
        // Disable spatial
        if (disable_spatial)
            GetAudioSource().spatialBlend = 0;

        // Mark as played
        played = true;
        // Start Audio and timer
        clip_timer.Start();
        GetAudioSource().Play();
    }
    #endregion

    #region Object Management
    /// <summary>
    ///     Destroys itself
    /// </summary>
    private void DestroySelf()
    {
        // Unbind method from clip timer
        if(clip_timer != null)
            clip_timer.OnCooldownSuccess -= DestroySelf;
        // Remove clip name from dictionary if we played
        if (played)
            RemoveFromDictionary(clip.name);

        // Destroy the gameobject
        Destroy(gameObject);
    }
    #endregion
    #region Dictionary Management
    private void AddToDictionary(string clipName, int count = 1)
    {
        // Make sure we are setup
        CheckActiveClipsDictionary();

        // Check if the key exists in the dictionary
        if (active_clips.ContainsKey(clipName))
        {
            active_clips[clipName] += (byte)count;
            return;
        }

        // Add new element
        active_clips.Add(clipName, (byte)count);
    }
    private void RemoveFromDictionary(string clipName, int count = 1)
    {
        // Make sure we are setup
        CheckActiveClipsDictionary();

        // Check if the key exists in the dictionary
        if (!active_clips.ContainsKey(clipName))
            return;

        // Remove count from key
        active_clips[clipName] = (byte)Mathf.Clamp(active_clips[clipName] - count, 0, max_duplicates);
    }

    public static bool isPlayable(string clipName)
    {
        // Make sure we are setup
        CheckActiveClipsDictionary();

        // Check if the key exists in the dictionary
        if (!active_clips.ContainsKey(clipName))
            return true;

        // Check if the active clips are less than the max allowed
        if (active_clips[clipName] < max_duplicates)
            return true;

        // Otherwise return false
        return false;
    }
    #endregion

    #region Dependencies
    private static void CheckActiveClipsDictionary()
    {
        if (active_clips == null)
            active_clips = new Dictionary<string, byte>();
    }
    #endregion
    #region Get Methods
    private AudioSource GetAudioSource()
    {
        if (source == null)
            source = GetComponent<AudioSource>();
        return source;
    }
    #endregion
}
