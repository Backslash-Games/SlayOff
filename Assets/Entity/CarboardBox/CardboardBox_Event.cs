using UnityEngine;
using System.Collections;
using UnityEngine.Events;

/// <summary>
///     Extension of Cardboard Box that allows for events to be called at different interaction stages
///     <br></br>
///     <br>Luke Wittbrodt :: lwittbrodt87@gmail.com :: halfhand870</br>
/// </summary>
public class CardboardBox_Event : CardboardBox
{
    [Header("Box.Event.Repsawn")]
    [SerializeField] private bool respawn = false;
    private Vector3 iOrigin;
    private Quaternion iRotation;
    [Space]
    [Header("Box.Event.Activatable")]
    [SerializeField] private IActivatable _activatable;
    [Space]
    [Header("Box.Event.UnityEvents")]
    [SerializeField] private UnityEvent UE_OnHurt;
    [SerializeField] private UnityEvent UE_OnDeath;
    [SerializeField] private UnityEvent UE_OnHeal;
    [SerializeField] private UnityEvent UE_OnSpawn;


    #region Unity Events
    private void Awake()
    {
        // Bind Unity Events to delegates
        OnHurt += (_, _, _) => UE_OnHurt.Invoke();
        OnHeal += (_, _, _) => UE_OnHeal.Invoke();

        // Bind Default
        UE_OnDeath.AddListener(ActivateStoredInterface);

        // Invoke initial spawn
        UE_OnSpawn.Invoke();

        // Setup information for repsawning the box
        if (respawn)
        {
            iOrigin = transform.position;
            iRotation = transform.rotation;
        }
    }
    private void OnDestroy()
    {
        UE_OnHurt.RemoveAllListeners();
        UE_OnDeath.RemoveAllListeners();
        UE_OnHeal.RemoveAllListeners();
        UE_OnSpawn.RemoveAllListeners();
    }
    #endregion
    #region Activatable Implementation
    /// <summary>
    ///     Sets the activatable object
    /// </summary>
    /// <param name="gameObject">Game Object</param>
    public void SetActivatable(GameObject gameObject)
    {
        // Check if the gameObject has a component that is activatable
        _activatable = gameObject.GetComponent<IActivatable>();
        if (_activatable == null)
            Debug.LogError($"Activatable never set, game object {gameObject.name} does not have IActivatable");
    }
    /// <summary>
    ///     Activates the stored IActivatable
    /// </summary>
    private void ActivateStoredInterface()
    {
        // Check for null
        if (_activatable == null)
            return;

        // Activate
        _activatable.Activate();
    }
    #endregion

    /// <summary>
    ///     CB_Event override for cleaning up the box
    /// </summary>
    public override void CleanupBox()
    {
        // Check if the respawn routine is null... It acts as a lock
        if (respawnRoutine != null)
            return;

        // Check if we need to run our respawn, or just our base cleanup
        if (respawn)
            respawnRoutine = StartCoroutine(RespawnBox());
        else
            base.CleanupBox();
    }
    /// <summary>
    ///     CB_Event override for death
    /// </summary>
    /// <param name="play_audio">Flag for if we should play break audio</param>
    public override void Death(bool play_audio = true)
    {
        // Check if our box is dead, if so invoke death event
        if (!isBoxDead())
            UE_OnDeath.Invoke();
        // Run basic death method
        base.Death(play_audio);
    }

    private Coroutine respawnRoutine = null; // Acts as a lock
    /// <summary>
    ///     Run logic for respawning the box
    /// </summary>
    /// <returns>yield</returns>
    private IEnumerator RespawnBox()
    {
        // Reset all basic components
        HealFull("Box.Event.Respawn");
        UnflattenBox();
        ResetVelocity();
        transform.position = iOrigin;
        transform.rotation = iRotation;
        SetConstraints(RigidbodyConstraints.FreezeAll);

        // Wait for a second before allowing the box to move again
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        // Reset flags
        SetDeadState(false);
        SetConstraints(RigidbodyConstraints.None);

        // Reset routine
        respawnRoutine = null;
    }
}
