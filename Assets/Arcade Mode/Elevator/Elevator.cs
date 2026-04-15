using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Elevator : MonoBehaviour
{
    public enum ElevatorTag { Start, Intermission, End };
    public enum TeleportationTag { Waypoint, ArcadeManaged };
    public enum AudioTag { Arrived };

    [Header("Attributes")]
    public ElevatorTag elevatorTag = ElevatorTag.Start;
    public TeleportationTag teleportationTag = TeleportationTag.Waypoint;
    [SerializeField] private Elevator _targetElevator = null;

    [Header("Animation")]
    [SerializeField] private Animator _animator;
    private static readonly string sr_arrivalAnimation = "Elevator_Start_Open";
    private static readonly string sr_departureAnimation = "Elevator_End_Close";

    [Header("Audio")]
    [SerializeField] private EffectLibrary<AudioTag, AudioClip, EffectComponent_Audio.AudioParameters> audioLibrary;

    [Header("Hitbox")]
    [SerializeField] private bool _drawTigger = false;
    [SerializeField] private Hitbox_Cube _triggerZone;

    [Header("Debug.Teleportation")]
    [SerializeField] private Transform _tpTestPoint;

    // Private objects
    private PlayerController _player = null;

    // Flags
    private bool _locked = false; // Checks if the elevator is locked

    #region Event Definitions
    /// <summary>
    ///     Delegate for transportation events
    /// </summary>
    public delegate void TransportationState();
    /// <summary>
    ///     Event for arrival
    /// </summary>
    public event TransportationState OnArrival;
    /// <summary>
    ///     Event for departure
    /// </summary>
    public event TransportationState OnDeparture;
    #endregion
    #region Event Binding
    /// <summary>
    ///     Binds local events
    /// </summary>
    private void BindEvents()
    {
        OnArrival += PlayArrivalAnimation;
    }
    /// <summary>
    ///     Unbinds all events
    /// </summary>
    private void UnbindAllEvents()
    {
        OnArrival = null;
        OnDeparture = null;
    }
    #endregion

    #region Unity Methods
    private void Awake()
    {
        BindEvents();
        General_Awake();
    }
    private void FixedUpdate()
    {
        OrientTriggerZone();
    }
    private void Update()
    {
        Tick();
    }
    private void OnDestroy()
    {
        UnbindAllEvents();
    }
    #endregion
    #region General
    /// <summary>
    ///     Collection of all awake methods
    /// </summary>
    private void General_Awake()
    {
        Start_Awake();
        Intermission_Awake();
        End_Awake();
    }
    /// <summary>
    ///     General Update Method
    /// </summary>
    private void Tick()
    {
        End_Tick();
    }
    #endregion

    #region Start
    /// <summary>
    ///     Intermission specific awake method
    /// </summary>
    private void Start_Awake()
    {
        // Check if we have the Intermission tag
        if (!elevatorTag.Equals(ElevatorTag.Start))
            return;
        // Bind Audio
        OnArrival += () => PlayAudio(AudioTag.Arrived);
    }
    #endregion
    #region Intermission
    /// <summary>
    ///     Intermission specific awake method
    /// </summary>
    private void Intermission_Awake()
    {
        // Check if we have the Intermission tag
        if (!elevatorTag.Equals(ElevatorTag.Intermission))
            return;

        // Bind event if we are a waypoint
        if (teleportationTag.Equals(TeleportationTag.Waypoint))
            OnArrival += Teleport;
    }
    #endregion
    #region End
    /// <summary>
    ///     End specific awake method
    /// </summary>
    private void End_Awake()
    {
        // Check if we have the end tag
        if (!elevatorTag.Equals(ElevatorTag.End))
            return;
        // Start by playing the arrival animation
        PlayArrivalAnimation();
    }
    /// <summary>
    ///     Update method for end elevators
    /// </summary>
    private void End_Tick()
    {
        // Check if we have the end tag
        if (!elevatorTag.Equals(ElevatorTag.End))
            return;

        // Check if we need to teleport to our target
        if (!_locked && _triggerZone.CheckCollision())
        {
            _locked = true;
            Teleport();
        }
    }
    #endregion

    #region Teleportation Handler
    /// <summary>
    ///     General statement that controls how teleportation is executed
    /// </summary>
    public void Teleport() 
    {
        // Branch based on animation
        if (GetAnimator() != null)
            AnimateTeleport();
        else
            TeleportPlayerToTarget();
    }
    
    /// <summary>
    ///     Teleports the player to the target elevator
    /// </summary>
    private void TeleportPlayerToTarget()
    {
        // Make sure target is set
        if (GetTargetElevator() == null)
            return;
        // Make sure player is set
        if (GetPlayer() == null)
            return;

        // Pull new position
        Vector3 position = MirrorVectorPosition(_player.transform.position, _targetElevator);

        // Pull camera orientation
        Vector3 viewPosition = Camera.main.transform.position + Camera.main.transform.forward;
        Vector3 cameraOrientation = MirrorVectorPosition(viewPosition, _targetElevator);

        // Pull Velocity
        Vector3 linearVelocity = _player.GetLinearVelocity() + GetOrigin();
        Vector3 rotatedVelocity = MirrorVectorPosition(linearVelocity, _targetElevator);
        rotatedVelocity = _targetElevator.GetVectorOffset(rotatedVelocity);

        // Teleport the player
        _player.Teleport(position, cameraOrientation, rotatedVelocity);

        // Invoke events
        OnDeparture?.Invoke(); // Departing from this elevator
        _targetElevator.OnArrival?.Invoke(); // Arriving at other elevator

        // Unlock
        _locked = false;
    }

    /// <summary>
    ///     Draws debug if the teleport object is set
    /// </summary>
    private void DrawTeleportDebug()
    {
        if (_tpTestPoint == null)
            return;
        if (_targetElevator == null)
            return;

        float radius = 0.5f;

        // Set initial information
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(_tpTestPoint.transform.position, radius);

        // Set other information
        Vector3 oPosition = MirrorVectorPosition(_tpTestPoint.transform.position, _targetElevator);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(oPosition, radius);
    }
    #endregion
    #region Hitbox Manager
    private bool _triggerDefaultSet = false;
    private Vector3 _triggerDefaultOffset = Vector3.zero;
    /// <summary>
    ///     Orients the trigger zone properly
    /// </summary>
    private void OrientTriggerZone()
    {
        // Mark default
        if (!_triggerDefaultSet)
        {
            _triggerDefaultOffset = _triggerZone.GetOffset();
            _triggerDefaultSet = true;
        }
        // Set position and rotation of trigger zone
        _triggerZone.SetOffset(transform.rotation * _triggerDefaultOffset);
        _triggerZone.SetLocalEuler(transform.eulerAngles);
    }
    #endregion

    #region Animation
    /// <summary>
    ///     Plays the arrival animation
    /// </summary>
    private void PlayArrivalAnimation()
    {
        if (GetAnimator() == null)
            return;
        // Play the awake animation
        _animator.Play(sr_arrivalAnimation);
    }

    Coroutine c_teleportAnimation = null;
    /// <summary>
    ///     Waits for animation to finish playing before teleporting
    /// </summary>
    private void AnimateTeleport()
    {
        // Check if our animator is null
        if (GetAnimator() == null)
            return;

        // Check for a cancel
        if (c_teleportAnimation != null)
            StopCoroutine(c_teleportAnimation);
        // Start up our coroutine
        c_teleportAnimation = StartCoroutine(Coroutine_AnimateTeleport());
    }
    private IEnumerator Coroutine_AnimateTeleport()
    {
        // Start animation
        _animator.Play(sr_departureAnimation);
        yield return new WaitForEndOfFrame();
        // Wait for animation length
        yield return new WaitForSeconds(_animator.GetCurrentAnimatorClipInfo(0)[0].clip.length);

        // Teleport
        TeleportPlayerToTarget();

        // Reset coroutine holder
        c_teleportAnimation = null;
    }


    /// <summary>
    ///     REMOVED
    /// </summary>
    public void TriggerAnimation(int elevator_index) { }
    #endregion
    #region Audio
    private void PlayAudio(AudioTag audio)
    {
        EffectManager.Instance.Play(audioLibrary, audio);
    }
    #endregion
    #region Math
    /// <summary>
    ///     Gets the origin of the elevator
    /// </summary>
    /// <returns>World position</returns>
    public Vector3 GetOrigin() { return transform.position; }
    /// <summary>
    ///     Gets the object offset in the current elevator
    /// </summary>
    /// <param name="cObject">Current object</param>
    /// <returns>Object offset</returns>
    public Vector3 GetVectorOffset(Vector3 point) { return point - GetOrigin(); }
    /// <summary>
    ///     Mirrors position in current elevator to another elevator
    /// </summary>
    /// <param name="point">Point in current elevator</param>
    /// <param name="other">Other elevator</param>
    /// <returns>World position in other elevator</returns>
    public Vector3 MirrorVectorPosition(Vector3 point, Elevator other)
    {
        // Get the offset from the current elevator
        Vector3 position = GetVectorOffset(point);
        // Interpret the position as if it weren't rotated
        position = Quaternion.Inverse(transform.rotation) * position;
        // Orient the position in the other elevator
        position = other.transform.rotation * position;
        // Move position into the other elevator
        position = other.GetOrigin() + position;

        return position;
    }
    #endregion

    #region Get Methods
    /// <summary>
    ///     Pulls the animator component
    /// </summary>
    /// <returns>Animator</returns>
    public Animator GetAnimator()
    {
        if (_animator == null)
            _animator = GetComponent<Animator>();
        return _animator;
    }
    /// <summary>
    ///     Pulls the Arcade Mode Manager
    /// </summary>
    /// <returns>Arcade Mode Manager</returns>
    public ArcadeModeManager GetArcadeModeManager()
    {
        if(ArcadeModeManager.Instance == null)
        {
            Debug.LogError("Please make sure the elevator is in an arcade mode scene");
            return null;
        }
        return ArcadeModeManager.Instance;
    }
    /// <summary>
    ///     Pulls the player
    /// </summary>
    /// <returns>PlayerController</returns>
    public PlayerController GetPlayer()
    {
        if (_player == null)
            _player = FindAnyObjectByType<PlayerController>();
        return _player;
    }
    /// <summary>
    ///     Gets the target elevator, finds correct elevator if null
    /// </summary>
    /// <returns>Target Elevator</returns>
    public Elevator GetTargetElevator() 
    { 
        // Check if target is null
        if(_targetElevator == null)
        {
            // Check if we are a starting elevator
            if (elevatorTag.Equals(ElevatorTag.Start)) return null;

            // Get all elevators in the scene
            Elevator[] elevators = FindObjectsByType<Elevator>(FindObjectsSortMode.None);
            // Hold search tag
            ElevatorTag searchTag = GetSearchTag();

            // Roll through all elevators in the scene and find our proper target
            foreach(Elevator elevator in elevators)
                if (elevator.elevatorTag.Equals(searchTag))
                {
                    _targetElevator = elevator;
                    break;
                }
        }
        return _targetElevator; 
    }
    /// <summary>
    ///     Gets the elevator search tag based on our current
    /// </summary>
    /// <returns>Elevator Tag</returns>
    public ElevatorTag GetSearchTag() 
    {
        switch (elevatorTag)
        {
            case ElevatorTag.Intermission:
                return ElevatorTag.Start;
            default:
                return ElevatorTag.Intermission;
        }
    }
    #endregion
    #region Set Methods
    /// <summary>
    ///     Sets the target elevator
    /// </summary>
    /// <param name="other">New Target</param>
    public void SetTargetElevator(Elevator other) { _targetElevator = other; }
    #endregion
    #region Debug
    private void OnDrawGizmos()
    {
        if (_drawTigger)
            _triggerZone.DrawGizmos();
        DrawTeleportDebug();
    }
    #endregion
}
