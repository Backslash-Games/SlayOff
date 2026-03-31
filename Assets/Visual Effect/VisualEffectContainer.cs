using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(VisualEffect))]
public class VisualEffectContainer : MonoBehaviour
{
    [SerializeField] private VisualEffect vfx;
    [SerializeField] private VisualEffectAsset asset;

    private bool played = false;

    private Cooldown effect_timer;

    private static readonly byte max_duplicates = 20;
    private static Dictionary<string, byte> active_effects;

    #region Playing
    /// <summary>
    ///     Plays a visual effect
    /// </summary>
    public void Play(VisualEffectAsset asset, float time, string play_id)
    {
        // Check if the clip is null
        if (vfx == null)
        {
            Debug.LogError("Attempted to play null visual effect");
            return;
        }

        // Set up the clip
        this.asset = asset;
        vfx.visualEffectAsset = asset;
        vfx.SendEvent(play_id);

        // Setup cooldown
        effect_timer = new Cooldown(this, time, 1);
        effect_timer.OnCooldownSuccess += DestroySelf;
        // Add clip to dictionary
        AddToDictionary(asset.name);

        // Mark as played
        played = true;
        // Start Audio and timer
        effect_timer.Start();
    }
    #endregion

    #region Object Management
    /// <summary>
    ///     Destroys itself
    /// </summary>
    private void DestroySelf()
    {
        // Unbind method from clip timer
        if (effect_timer != null)
            effect_timer.OnCooldownSuccess -= DestroySelf;
        // Remove clip name from dictionary if we played
        if (played)
            RemoveFromDictionary(asset.name);

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
        if (active_effects.ContainsKey(clipName))
        {
            active_effects[clipName] += (byte)count;
            return;
        }

        // Add new element
        active_effects.Add(clipName, (byte)count);
    }
    private void RemoveFromDictionary(string clipName, int count = 1)
    {
        // Make sure we are setup
        CheckActiveClipsDictionary();

        // Check if the key exists in the dictionary
        if (!active_effects.ContainsKey(clipName))
            return;

        // Remove count from key
        active_effects[clipName] = (byte)Mathf.Clamp(active_effects[clipName] - count, 0, max_duplicates);
    }

    public static bool isPlayable(string effectName)
    {
        // Make sure we are setup
        CheckActiveClipsDictionary();

        // Check if the key exists in the dictionary
        if (!active_effects.ContainsKey(effectName))
            return true;

        // Check if the active clips are less than the max allowed
        if (active_effects[effectName] < max_duplicates)
            return true;

        // Otherwise return false
        return false;
    }
    #endregion

    #region Dependencies
    private static void CheckActiveClipsDictionary()
    {
        if (active_effects == null)
            active_effects = new Dictionary<string, byte>();
    }
    #endregion
}
