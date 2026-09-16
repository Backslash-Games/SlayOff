using HFHandyUtils.Time;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class MenuContainer
{
    #region Static
    /// <summary>
    ///     Default fade speed
    /// </summary>
    private static readonly float s_DefaultFadeSpeed = 0.2f;
    /// <summary>
    ///     Default fade threshold
    /// </summary>
    private static readonly float s_FadeThreshold = 0.01f;
    #endregion
    #region Serialized
    [Header("Data")]
    [SerializeField] private string id = "NO ID";
    [Space]
    [SerializeField] private MenuManager.Type menuType = MenuManager.Type.Pause;
    [SerializeField] private CanvasGroup _parentGroup = null;

    [Header("Logic")]
    [SerializeField] private bool _setInactiveWhenClosed = true;

    [Header("Time")]
    [SerializeField] private float time_scale = 1;
    private TimeControl time_control;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string open_anim_id;
    [SerializeField] private string close_anim_id;
    [SerializeField] private float closing_time = 0;

    [Header("Events")]
    public UnityEvent OnMenuOpened;
    public UnityEvent OnMenuClosed;
    #endregion
    #region Unserialized
    private bool active = false;

    private PlayerController player;
    private MonoBehaviour monoBehaviour;
    #endregion


    #region Initialization
    /// <summary>
    ///     Initializes the current menu. Binding the mono behaviour
    /// </summary>
    /// <param name="mono">Parent Mono Behaviour</param>
    public void Initialize(MonoBehaviour mono)
    {
        monoBehaviour = mono;

        time_control = new TimeControl(mono);

        // Bind opening methods
        OnMenuOpened.AddListener(OnMenuOpened_DefaultListener);
        // Bind closing methods
        OnMenuClosed.AddListener(OnMenuClosed_DefaultListener);

        SetParentState(false);
        GetPlayer(); // Make sure player is set
    }
    /// <summary>
    ///     Disables components associated with the menu component
    /// </summary>
    public void Disable()
    {
        OnMenuOpened.RemoveListener(OnMenuOpened_DefaultListener);
        OnMenuClosed.RemoveListener(OnMenuClosed_DefaultListener);
    }
    #endregion

    #region Events
    /// <summary>
    ///     Default listener for OnMenuOpened
    /// </summary>
    private void OnMenuOpened_DefaultListener()
    {
        SetParentState(true);        
        PlayAnimation(open_anim_id);
        time_control.SetScale(time_scale);
        TriggerControls(true);
    }
    /// <summary>
    ///     Default listener for OnMenuClosed
    /// </summary>
    private void OnMenuClosed_DefaultListener()
    {
        if (closing_time > 0)
            monoBehaviour.StartCoroutine(WaitToClose());
        else
            ForceClose();
    }
    /// <summary>
    ///     Delayed close
    /// </summary>
    private IEnumerator WaitToClose()
    {
        PlayAnimation(close_anim_id);
        yield return new WaitForSecondsRealtime(closing_time);

        // Run close
        time_control.ResetScale();
        SetParentState(false);
        TriggerControls(false);
    }
    /// <summary>
    ///     Forceful close
    /// </summary>
    private void ForceClose()
    {
        PlayAnimation(close_anim_id);
        time_control.ResetScale();
        SetParentState(false);
        TriggerControls(false);
    }
    #endregion

    #region Menu State
    /// <summary>
    ///     Triggers a menu with a state
    /// </summary>
    /// <param name="state">Menu state</param>
    public void TriggerMenu(bool state)
    {
        // Ensure the new state is not the same as our active state
        if (active == state)
            return;

        // Set up active
        active = state;

        // Run Events
        if (active)
            OnMenuOpened.Invoke();
        else
            OnMenuClosed.Invoke();
    }
    /// <summary>
    ///     Quick call to open the menu
    /// </summary>
    public void OpenMenu() { TriggerMenu(true); }
    /// <summary>
    ///     Quick call to close the menu
    /// </summary>
    public void CloseMenu() { TriggerMenu(false); }
    #endregion
    #region Controls
    /// <summary>
    ///     Note that the nomenclature of this method differs from menu opening. Due to more complex actions
    /// </summary>
    /// <param name="state">Input state</param>
    private void TriggerControls(bool state) 
    {
        if (state)
            LockControls();
        else
            UnlockControls(); 
    }
    /// <summary>
    ///     Locks player controls
    /// </summary>
    private void LockControls()
    {
        Cursor.lockState = CursorLockMode.None;
        if (player != null)
            player.SetControlMapActive(false);
    }
    /// <summary>
    ///     Unlocks player controls
    /// </summary>
    private void UnlockControls()
    {
        Cursor.lockState = CursorLockMode.Locked;
        if(player != null)
            player.SetControlMapActive(true);
    }
    #endregion

    #region Animation
    private void PlayAnimation(string id)
    {
        if (animator == null)
        {
            Debug.Log("Animator is null");
            return;
        }
        Debug.Log("Playing " + id);
        animator.Play(id);
    }
    #endregion
    #region GameObject Management
    /// <summary>
    ///     Sets the parent to a new state
    /// </summary>
    /// <param name="state">New State</param>
    private void SetParentState(bool state) { SetParentActive(state); }
    #endregion

    #region Get Methods
    /// <summary>
    ///     Pulls the menu type
    /// </summary>
    /// <returns>Menu type</returns>
    public MenuManager.Type GetMenuType() { return menuType; }
    /// <summary>
    ///     Pulls the time scale
    /// </summary>
    /// <returns>Time scale</returns>
    public float GetTimeScale() { return time_scale; }
    /// <summary>
    ///     Pulls the menu parent
    /// </summary>
    /// <returns>Canvas group transform</returns>
    public Transform GetMenuParent() { return _parentGroup.transform; }
    /// <summary>
    ///     Gets the current menu state
    /// </summary>
    /// <returns>Active state</returns>
    public bool GetMenuState() { return active; }
    
    /// <summary>
    ///     Pulls the player
    /// </summary>
    /// <returns>Player</returns>
    private PlayerController GetPlayer()
    {
        if (player == null)
            player = MonoBehaviour.FindAnyObjectByType<PlayerController>();
        return player;
    }
    #endregion

    #region Fade
    private Coroutine _parentFadeRoutine = null;
    /// <summary>
    ///     Fades the parent in and out
    /// </summary>
    /// <param name="state">New state</param>
    private void SetParentActive(bool state)
    {
        // Check if the parent can be set active
        if (_parentGroup == null) return;
        // Run the fade routine
        if (_parentFadeRoutine != null) monoBehaviour.StopCoroutine(_parentFadeRoutine);
        _parentFadeRoutine = monoBehaviour.StartCoroutine(IENUM_SetParentActive(state));
    }
    /// <summary>
    ///     Asynchronous procress for fading in and out the parent canvas group
    /// </summary>
    /// <param name="state">New state</param>
    /// <returns>WaitForEndOfFrame</returns>
    private IEnumerator IENUM_SetParentActive(bool state)
    {
        float targetAlpha = state ? 1 : 0;
        if (_setInactiveWhenClosed && state) _parentGroup.gameObject.SetActive(true);

        while (Mathf.Abs(targetAlpha - _parentGroup.alpha) >= s_FadeThreshold)
        {
            _parentGroup.alpha = Mathf.Lerp(_parentGroup.alpha, targetAlpha, s_DefaultFadeSpeed);
            yield return new WaitForEndOfFrame();
        }
        _parentGroup.alpha = targetAlpha;

        if (_setInactiveWhenClosed && !state) _parentGroup.gameObject.SetActive(false);
    }
    #endregion
}
