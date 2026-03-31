using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class MenuContainer
{
    private bool active = false;

    [SerializeField] private MenuManager.Type menuType = MenuManager.Type.Pause;
    [SerializeField] private float time_scale = 1;
    private TimeControl time_control;
    [SerializeField] private Transform menuParent = null;
    [Space]
    [SerializeField] private Animator animator;
    [SerializeField] private string open_anim_id;
    [SerializeField] private string close_anim_id;

    public UnityEvent OnMenuOpened;
    public UnityEvent OnMenuClosed;

    private PlayerController player;
    private MonoBehaviour monoBehaviour;

    #region Initialization
    public void Initialize(MonoBehaviour mono)
    {
        monoBehaviour = mono;

        time_control = new TimeControl(mono);

        OnMenuOpened.AddListener(OnMenuOpened_DefaultListener);
        OnMenuClosed.AddListener(OnMenuClosed_DefaultListener);

        SetParentState(false);
        GetPlayer(); // Make sure player is set
    }
    public void Disable()
    {
        OnMenuOpened.RemoveListener(OnMenuOpened_DefaultListener);
        OnMenuClosed.RemoveListener(OnMenuClosed_DefaultListener);
    }
    #endregion

    #region Events
    private void OnMenuOpened_DefaultListener()
    {
        Debug.Log("Default");
        PlayAnimation(open_anim_id);
        time_control.SetScale(time_scale);
        SetParentState(true);
        TriggerControls(true);
    }
    private void OnMenuClosed_DefaultListener()
    {
        PlayAnimation(close_anim_id);
        monoBehaviour.StartCoroutine(WaitToClose());
    }
    private IEnumerator WaitToClose()
    {
        while (animator != null && animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == close_anim_id)
            yield return new WaitForEndOfFrame();

        // Run close
        time_control.ResetScale();
        SetParentState(false);
        TriggerControls(false);
    }
    #endregion

    #region Menu State
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
    public void OpenMenu() { TriggerMenu(true); }
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

    private void LockControls()
    {
        Cursor.lockState = CursorLockMode.None;
        if (player != null)
            player.SetControlMapActive(false);
    }
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
            return;
        animator.Play(id);
    }
    #endregion
    #region GameObject Management
    private void SetParentState(bool state) { GetMenuParent().gameObject.SetActive(state); }
    #endregion

    #region Get Methods
    public MenuManager.Type GetMenuType() { return menuType; }
    public float GetTimeScale() { return time_scale; }
    public Transform GetMenuParent() { return menuParent; }
    public bool GetMenuState() { return active; }
    
    private PlayerController GetPlayer()
    {
        if (player == null)
            player = MonoBehaviour.FindAnyObjectByType<PlayerController>();
        return player;
    }
    #endregion
}
