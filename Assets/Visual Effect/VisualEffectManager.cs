using UnityEngine;
using UnityEngine.VFX;

// In the future collapse this with audio manager, can be a generic effect manager that spawns generic effects

public class VisualEffectManager : MonoBehaviour
{
    // Singleton
    private static VisualEffectManager _instance;
    public static VisualEffectManager Instance { get { return _instance; } }

    [Header("Visual Effects")]
    [SerializeField] private GameObject visualEffectPrefab;

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
        Debug.LogWarning("VisualManager is depreciated, please refer to EffectManager");
        //CreateSingleton();
    }
    #endregion

    #region Tools
    public void PlayVisual(VisualEffectAsset asset, Vector3 position, float time, string play_id)
    {
        Debug.LogWarning("VisualManager is depreciated, please refer to EffectManager");
        /*// Check if the clip is playable
        if (!VisualEffectContainer.isPlayable(asset.name))
            return;

        // Summon a new audio effect clip
        GameObject spawned = Instantiate(visualEffectPrefab, position, Quaternion.identity, transform);
        spawned.name = $"Visual Effect - {asset.name}";
        VisualEffectContainer effect = spawned.GetComponent<VisualEffectContainer>();

        // Check if we are null
        if (effect == null)
        {
            Destroy(spawned);
            return;
        }

        // Apply the clip to the effect
        effect.Play(asset, time, play_id);*/
    }
    #endregion
}
