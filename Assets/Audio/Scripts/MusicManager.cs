using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    private enum SceneState { title, factory, office, corporate };
    [System.Serializable]
    private struct SceneCorrelation
    {
        public string sceneName;
        public SceneState sceneState;

        public SceneCorrelation(string name, SceneState state)
        {
            sceneName = name;
            sceneState = state;
        }
    }

    [SerializeField] private SceneCorrelation[] sceneCorrelations;
    [SerializeField] private SceneState currentSceneState;
    [Space]
    [SerializeField] private EffectLibrary<SceneState, AudioClip, EffectComponent_Audio.AudioParameters> musicLibrary;
    [SerializeField] private EffectLibrary<SceneState, AudioClip, EffectComponent_Audio.AudioParameters> ambienceLibrary;

    private static readonly EffectManager.PlayMode s_mode = EffectManager.PlayMode.Continuous;

    #region Unity Methods
    private void Awake()
    {
        BindEvents();
    }
    private void OnDestroy()
    {
        UnbindEvents();
    }
    #endregion
    #region Scene Binding
    private void BindEvents()
    {
        SceneManager.sceneLoaded += PlayAudio;
        SceneManager.sceneUnloaded += StopAudio;
    }
    private void UnbindEvents()
    {
        SceneManager.sceneLoaded -= PlayAudio;
        SceneManager.sceneUnloaded -= StopAudio;
    }
    #endregion

    #region Music Handling
    /// <summary>
    ///     Plays music and ambience track based on current scene
    /// </summary>
    private void PlayAudio(Scene scene, LoadSceneMode loadMode)
    {
        // Update the current scene state
        UpdateCurrentSceneState(scene.name);

        // Play music and ambience
        EffectManager.Instance.Play(musicLibrary, currentSceneState, s_mode);
        EffectManager.Instance.Play(ambienceLibrary, currentSceneState, s_mode);
    }
    /// <summary>
    ///     Stops music and ambience based on current playing
    /// </summary>
    private void StopAudio(Scene scene)
    {
        // Update the current scene state
        UpdateCurrentSceneState(scene.name);

        // Play music and ambience
        EffectManager.Instance.Stop(musicLibrary, currentSceneState);
        EffectManager.Instance.Stop(ambienceLibrary, currentSceneState);
    }

    private void UpdateCurrentSceneState(string currentSceneName)
    {
        // Roll through all scene correlations and find the matching scene
        foreach (SceneCorrelation correlation in sceneCorrelations)
            if (correlation.sceneName.Equals(currentSceneName))
            {
                currentSceneState = correlation.sceneState;
                return;
            }
    }
    #endregion
}
