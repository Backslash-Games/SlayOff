using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;
using UnityEngine.SceneManagement;

public class TitleScreen_ClickHandler : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private Animator titleAnimator;
    [SerializeField] private string introAnimationName;

    [Header("Input")]
    [SerializeField] private InputActionAsset PlayerActions;
    [SerializeField] private bool action_locked = false;
    [Space]
    private InputAction in_click;
    [SerializeField] private VisualEffectAsset dust_burst;
    [SerializeField] private float shootStrength = 5;

    [Header("Scene Handling")]
    [SerializeField] private bool load_starting_scene;
    [SerializeField] private string scene_name;

    #region Unity Methods
    private void Awake()
    {
        if (PlayerActions == null)
            return;

        in_click = PlayerActions.FindAction("Melee");
        in_click.started += _ => Shoot();

        PlayerActions.Enable();

        MenuManager.Instance.OpenMenu(MenuManager.Type.LayOff);
    }
    private void OnDestroy()
    {
        in_click.started -= _ => Shoot();

        PlayerActions.Disable();
    }

    private void LateUpdate()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
            Cursor.lockState = CursorLockMode.None;

        CheckSceneLoad();
    }
    #endregion

    private void Shoot()
    {
        if (action_locked)
            return;

        // Get mouse position in world
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        // Fire the ray
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            VisualEffectManager.Instance.PlayVisual(dust_burst, hit.point, 0.5f, "OnBurst");
            EntityData entity = hit.transform.gameObject.GetComponent<EntityData>();
            // Check if we have hit an entity
            if (entity != null)
            {
                entity.ApplyForce(hit.point - Camera.main.transform.position, shootStrength, ForceMode.Impulse, "TitleScreen.Shoot");
            }
        }
    }

    #region Sequencing
    public void StartGame()
    {
        Debug.Log("Starting Game");
        SetActionLock(true);
        titleAnimator.Play(introAnimationName);
    }
    public void OpenOptions()
    {
        Debug.Log("Opening Options");
        SetActionLock(true);
    }
    public void QuitGame()
    {
        Debug.Log("Quitting Game");
        SetActionLock(true);
    }

    public void SetActionLock(bool state) { action_locked = state; }
    #endregion
    #region Scene Loading
    private void CheckSceneLoad()
    {
        if (load_starting_scene)
            SceneManager.LoadScene(scene_name);
    }
    #endregion
}
