using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : EntityData
{
    [Header("General")]
    [SerializeField] private StandingState standingState = StandingState.Standing;
    private enum StandingState { Standing, Crouching, Sliding }

    [Header("Hitboxes")]
    [SerializeField] private Hitbox_Sphere groundCheck;
    [SerializeField] private Hitbox_Sphere crouch_headCheck;

    [Header("Physics")]
    [SerializeField] private bool usePhysics = true;

    [Header("Physics.Gravity")]
    [SerializeField] private float gravityStrength;
    private float currentGravityScale = 0;

    [Header("Physics.Jumping")]
    [SerializeField] private int jumpsAllowed = 2;
    private int jumpsPreformed = 0;
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpLunge;
    private enum JumpLock { Ready, AwaitingRelease, SearchingGround};
    private JumpLock jumpLock = 0;

    [Header("Physics.Breaking")]
    [SerializeField] private float breakingSpeed = 1;
    [SerializeField] private float breakingVelocityThreshold = 1;

    [Header("Physics.Crouching")]
    [SerializeField] private float crouchSpeed = 1;
    [Space]
    [SerializeField] private float crouchHeight = 1;
    [SerializeField] private Vector3 crouchCenter = Vector3.zero;
    [SerializeField] private Vector3 crouchCameraCenter = Vector3.zero;
    private float crouch_collisionInitialHeight = 2;
    private Vector3 crouch_collisionInitialCenter = Vector3.zero;
    [Space]
    [SerializeField] private float crouchMovementScale = 1;
    [SerializeField] private float crouchGravityBonus = 1;

    [Header("Physics.Sliding")]
    [SerializeField] private float slidingVelocityThreshold = 8;
    [SerializeField] private float slidingSpeed = 10;
    private float slidingCurrentSpeed = 0;
    [SerializeField] private float slidingFrictionRate = 1;
    [Space]
    [SerializeField] private float slidingDownHillBonus = 1;
    private float slidingMomentumAccuracy = 0;
    private Vector3 slidingIdealMomentumNormal = Vector3.zero;
    private Vector3 slidingIdealMomentumPivot = Vector3.zero;
    private Vector3 slidingIdealMomentumDirection = Vector3.zero;
    [Space]
    [SerializeField] private float slidingJumpOutSpeed = 10;
    [SerializeField] private float slidingJumpOutUpwardForce = 10;
    private Vector3 slidingDirection = Vector3.zero;
    [Space]
    [SerializeField] private float slidingBonusGravity = 10;



    [Header("Camera")]
    [SerializeField] private CameraController cameraController;
    [SerializeField] private float whiplashStrength = 1;
    [SerializeField] private float whiplashReduction = 1;



    [Header("Input")]
    [SerializeField] private InputActionAsset PlayerActions;
    private InputAction in_move;
    private InputAction in_look;
    private InputAction in_jump;
    private InputAction in_crouch;
    private InputAction in_pause;

    /// <summary>
    ///     Objectives
    /// </summary>
    private static readonly string comboObjective_SlideKey = "Distance Slid";
    private float co_SlideDistance = 0;
    private static readonly string comboObjective_SlamKey = "Distance Slammed";
    private float co_SlamDistance = 0;
    private float co_SlamThreshold = 1f;
    private static readonly string comboObjective_JumpKey = "Bunny Hops";


    #region Unity Methods
    private void Start()
    {
        // Run collision start
        CollisionStart();
    }
    public override void OnEnabled()
    {
        // Runs the base on enabled method
        base.OnEnabled();
        // Binds player inputs
        BindEvents();
    }
    public override void OnDisabled()
    {
        // Unbinds player inputs
        UnbindEvents();
    }

    private void FixedUpdate()
    {
        // Updates hitboxes
        UpdateHitboxes();
        // Updates player inputs
        FixedUpdateInput();

        // Run physics updates
        PhysicsUpdate();
    }
    private void Update()
    {
        // Updates player inputs
        UpdateInput();
    }
    private void LateUpdate()
    {
        // Run Collision updates
        CollisionUpdate();
    }
    #endregion
    #region Hitboxes
    private void OnDrawGizmos()
    {
        // Draw grounded hitbox
        groundCheck.DrawGizmos();
        crouch_headCheck.DrawGizmos();

        // Velocity testing
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(Vector3.zero, GetLinearVelocity());

        Gizmos.color = Color.blueViolet;
        Gizmos.DrawLine(Vector3.zero, Quaternion.Inverse(cameraController.GetTransform_Yaw().rotation) * GetLinearVelocity());

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + slidingIdealMomentumNormal * 5);
        Gizmos.color = Color.darkGreen;
        Gizmos.DrawLine(transform.position, transform.position + slidingIdealMomentumPivot * 5);
        Gizmos.color = Color.dodgerBlue;
        Gizmos.DrawLine(transform.position, transform.position + slidingIdealMomentumDirection * 5);
    }

    /// <summary>
    ///     Ticks forward all hitboxes... Called in both fixed and normal update
    /// </summary>
    private void UpdateHitboxes()
    {
        groundCheck.Tick();
        // Update head check if we are crouching or sliding
        if (standingState.Equals(StandingState.Crouching) || standingState.Equals(StandingState.Sliding))
            crouch_headCheck.Tick();
    }
    #endregion

    #region Bind Events
    /// <summary>
    ///     Binds inputs to the player
    /// </summary>
    private void BindEvents()
    {
        // Check if player actions are set
        if (PlayerActions == null)
            return;

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;

        // Setup actions
        in_move = PlayerActions.FindAction("Move");
        in_look = PlayerActions.FindAction("Look");
        in_jump = PlayerActions.FindAction("Jump");
        in_crouch = PlayerActions.FindAction("Crouch");
        in_pause = PlayerActions.FindAction("Pause");

        // Set up events
        OnHurt += PlayerController_OnHurt;
        in_pause.performed += _ => OnPausePerformed();

        // External bind methods
        BindCrouch();

        // Enable input
        PlayerActions.FindActionMap("Control").Enable();
        PlayerActions.FindActionMap("UI").Enable();
    }

    /// <summary>
    ///     Unbinds inputs from the player
    /// </summary>
    private void UnbindEvents()
    {
        // Check if player actions are set
        if (PlayerActions == null)
            return;

        // Unlock cursor
        Cursor.lockState = CursorLockMode.None;

        // Set up events
        OnHurt -= PlayerController_OnHurt;
        in_pause.performed -= _ => OnPausePerformed();
        
        // External unbind methods
        UnbindCrouch();

        // Disable input
        PlayerActions.FindActionMap("Control").Disable();
        PlayerActions.FindActionMap("UI").Disable();
    }
    #endregion
    #region Events

    private void PlayerController_OnHurt(string source, Vector3 origin, float amount)
    {
        // Apply whiplash
        CameraController.Instance.AddModifierWhiplash(origin, whiplashStrength, whiplashReduction);

        // -> Get hurt rotation
        Vector3 direction = Camera.main.transform.rotation * (origin - transform.position).normalized;
        Vector2 flat_direction = new Vector2(direction.x, direction.z);
        float rotation = Vector2.SignedAngle(Vector2.up, flat_direction);
        // Apply crosshair
        CrosshairController.Instance.RequestCrosshair(CrosshairController.CrosshairType.Hurt, -rotation);
    }
    #endregion
    #region Inputs
    /// <summary>
    ///     Updates the players inputs through fixed update
    /// </summary>
    private void FixedUpdateInput()
    {
        Movement(); // Updates movement
        Jump(); // Updates Jump

        UpdateStandingState();
        UpdateSliding();
    }
    /// <summary>
    ///     Sends a desired influence to our camera controller. Controller handles interpolation
    /// </summary>
    private void UpdateInput()
    {
        Look(); // Updates look direction   
    }

    #region Look
    private void Look()
    {
        // Store the input
        Vector2 cInput = in_look.ReadValue<Vector2>();
        float inputScaling = 0.01f;
        cameraController.PushCameraAxis(cInput * inputScaling);
    }
    #endregion
    #region Move
    /// <summary>
    ///     Updates movement
    /// </summary>
    private void Movement()
    {
        // Skip movement when sliding
        if (standingState.Equals(StandingState.Sliding))
            return;

        // Get the input
        Vector3 worldForce = cameraController.GetTransform_Yaw().rotation * GetInputToWorldSpace();

        // Get the current speed
        float currentSpeed = GetStatblock().GetSpeed();
        // -> Check if we are crouched
        if (standingState.Equals(StandingState.Crouching))
            currentSpeed *= crouchMovementScale;

        // Apply movement
        ApplyForce(worldForce, currentSpeed, ForceMode.Acceleration, "Player.Movement");
    }

    /// <summary>
    ///     Gets player input and converts it world world space
    /// </summary>
    /// <returns>World Space - Uneffected by camera rotation</returns>
    private Vector3 GetInputToWorldSpace()
    {
        // Store input
        Vector2 cInput = in_move.ReadValue<Vector2>();
        // Calculate force that should be applied in world space
        return new Vector3(cInput.x, 0, cInput.y);
    }
    #endregion
    #region Jump
    /// <summary>
    ///     Updates Jumping
    /// </summary>
    private void Jump()
    {
        // Make sure we are standing
        if(standingState.Equals(StandingState.Crouching))
            return;
        // Check if we are on the ground
        // -> Unlock jump when in the air
        if (!groundCheck.GetState() && jumpsPreformed >= jumpsAllowed)
            return;
        else if (jumpLock.Equals(JumpLock.SearchingGround))
            jumpLock = JumpLock.AwaitingRelease;
        else if (groundCheck.GetState() && jumpsPreformed < jumpsAllowed)
            jumpsPreformed = 0;
        // Check if we are inputting to jump
        if (!in_jump.IsPressed())
        {
            if (jumpLock.Equals(JumpLock.AwaitingRelease))
                UnlockJump();
            return;
        }
        // Check if jump is locked
        if (!jumpLock.Equals(JumpLock.Ready))
            return;

        // Reset the vertical velocity
        ResetVerticalVelocity();

        // Use a raycast to find the jump angle
        bool groundFound = Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 1.5f, groundCheck.GetLayerMask());
        // Calculate force that should be applied in world space
        Vector3 worldForce = Vector3.up;
        if (groundFound)
            worldForce = hit.normal;

        // Apply movement based on standing state
        if (standingState.Equals(StandingState.Standing))
        {
            JumpLunge_Standing();
            ApplyForce(worldForce, jumpForce, ForceMode.Impulse, "Player.Jump.Standing");
        }
        else if (standingState.Equals(StandingState.Sliding))
        {
            JumpLunge_Sliding();
            ApplyForce(worldForce, slidingJumpOutUpwardForce, ForceMode.Impulse, "Player.Jump.Sliding");
            EndCrouch();
        }

        // Updates combo objective
        ComboObjective_TickJump();

        // Increase jumps preformed
        jumpsPreformed++;

        // Lock jump when max allowed jumps have been preformed
        if (jumpsPreformed >= jumpsAllowed)
            jumpLock = JumpLock.SearchingGround;
        else
            jumpLock = JumpLock.AwaitingRelease;
    }
    /// <summary>
    ///     Unlocks the jump
    /// </summary>
    private void UnlockJump()
    {
        jumpLock = JumpLock.Ready;

        // Reset when we have preformed the max allocated jumps
        if (jumpsPreformed >= jumpsAllowed)
            jumpsPreformed = 0;
    }
    /// <summary>
    ///     Does a minor lunge when the player jumps. Allows for bunny hopping
    /// </summary>
    private void JumpLunge_Standing()
    {
        // Get world force from input
        Vector3 worldForce = cameraController.GetTransform_Yaw().rotation * GetInputToWorldSpace();

        // Apply movement
        ApplyForce(worldForce, jumpLunge * GetHorizontalVelocity().magnitude, ForceMode.Impulse, "Player.JumpLunge");
    }
    /// <summary>
    ///     Does a minor lunge when the player jumps. Allows for bunny hopping
    /// </summary>
    private void JumpLunge_Sliding()
    {
        // Calculate force that should be applied in world space
        Vector2 local_horizontalVelocity = GetHorizontalVelocity();
        Vector3 worldForce = new Vector3(local_horizontalVelocity.x, 0, local_horizontalVelocity.y);

        // Apply movement
        ApplyForce(worldForce, slidingJumpOutSpeed * GetHorizontalVelocity().magnitude, ForceMode.Impulse, "Player.JumpLunge");
    }
    #endregion

    #region Standing
    private StandingState targetStandingState = StandingState.Standing;
    private void UpdateStandingState()
    {
        // Check if the target is equal to current, if so return
        if (targetStandingState.Equals(standingState))
            return;

        bool ssChangeAllowed = false;
        // Allow crouch and sliding to pass through
        if (targetStandingState.Equals(StandingState.Crouching) || targetStandingState.Equals(StandingState.Sliding))
            ssChangeAllowed = true;
        else if (targetStandingState.Equals(StandingState.Standing))
        {
            // Make sure the players head isnt blocked
            if (!crouch_headCheck.GetState())
                ssChangeAllowed = true;
        }

        // Allow the state to change
        if(ssChangeAllowed)
            standingState = targetStandingState;
    }
    /// <summary>
    ///     Sets up the target for a standing state change, establishes target
    /// </summary>
    /// <param name="state">New State</param>
    private void RequestStandingStateChange(StandingState state)
    {
        targetStandingState = state;
    }
    #endregion
    #region Crouch
    /// <summary>
    ///     Binds crouch functions to input
    /// </summary>
    private void BindCrouch()
    {
        in_crouch.started += _ => CheckCrouchState();
        in_crouch.canceled += _ => EndCrouch();
    }
    /// <summary>
    ///     Unbinds crouch to input
    /// </summary>
    private void UnbindCrouch()
    {
        in_crouch.started -= _ => CheckCrouchState();
        in_crouch.canceled -= _ => EndCrouch();
    }

    /// <summary>
    ///     Starts crouching to check for sliding
    /// </summary>
    private void StartCrouch()
    {
        // When we start crouching check if we need to shift to sliding instead
        RequestStandingStateChange(StandingState.Crouching);
    }
    /// <summary>
    ///     Stops crouching
    /// </summary>
    private void EndCrouch()
    {
        // Pass off functionality to start standing
        RequestStandingStateChange(StandingState.Standing);
    }

    /// <summary>
    ///     Checks if we continue with crouching, or if we pivot to sliding
    /// </summary>
    private void CheckCrouchState()
    {
        // Check if we have surpassed sliding threshold
        if (GetHorizontalVelocity().magnitude >= slidingVelocityThreshold)
            StartSliding();
        else
            StartCrouch();
    }
    #endregion
    #region Sliding
    /// <summary>
    ///     Starts sliding
    /// </summary>
    private void StartSliding()
    {
        // Set up initial sliding variables
        slidingDirection = cameraController.GetTransform_Yaw().rotation * GetInputToWorldSpace();
        slidingCurrentSpeed = slidingSpeed;

        // Reset Slide Distance
        co_SlideDistance = 0;

        // Request a swap to slide
        RequestStandingStateChange(StandingState.Sliding);
    }
    /// <summary>
    ///     Ends sliding
    /// </summary>
    private void EndSliding()
    {
        StartCrouch();
    }

    /// <summary>
    ///     Updates sliding
    /// </summary>
    private void UpdateSliding()
    {
        // Check if we are sliding
        if (!standingState.Equals(StandingState.Sliding))
            return;

        // Get the current horizontal velocity
        Vector2 local_horizontalVelocity = GetHorizontalVelocity();
        Vector3 world_horizontalVelocity = new Vector3(local_horizontalVelocity.x, 0, local_horizontalVelocity.y);
        // Continue to check the crouch state
        if (world_horizontalVelocity.magnitude < slidingVelocityThreshold)
            EndSliding();

        // Update the sliding distance combo objective
        ComboObjective_TickSliding();

        // Apply a constant force in the direction you are moving
        ApplyForce(slidingDirection, slidingCurrentSpeed, ForceMode.Acceleration, "Player.Sliding");

        // Change the sliding speed
        // -> Pull information about current surface
        Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 3, groundCheck.GetLayerMask());
        // Find the slope downward
        slidingIdealMomentumNormal = hit.normal;
        slidingIdealMomentumPivot = Vector3.Cross(slidingIdealMomentumNormal, Vector3.up).normalized;
        slidingIdealMomentumDirection = Vector3.Cross(slidingIdealMomentumNormal, slidingIdealMomentumPivot).normalized;
        // Calculate accuracy
        slidingMomentumAccuracy = Mathm.GetVectorAccuracy(Mathm.RemoveVerticalAxis(slidingDirection), Mathm.RemoveVerticalAxis(slidingIdealMomentumDirection));
        slidingCurrentSpeed = Mathf.Clamp((slidingCurrentSpeed + (slidingMomentumAccuracy - 0.5f) * slidingDownHillBonus) - slidingFrictionRate, 0, float.MaxValue);
    }
    #endregion

    #region Menu Handling
    private void OnPausePerformed()
    {
        if (MenuManager.Instance.isAnyMenuActive())
            MenuManager.Instance.CloseAllMenus();
        else
            MenuManager.Instance.OpenPauseMenu();
    }
    #endregion
    #region Control State
    public void SetControlMapActive(bool state)
    {
        if(state)
            PlayerActions.FindActionMap("Control").Enable();
        else
            PlayerActions.FindActionMap("Control").Disable();
    }
    #endregion
    #endregion

    #region Physics
    /// <summary>
    ///     General update for physics methods
    /// </summary>
    private void PhysicsUpdate()
    {
        if (!usePhysics)
            return;

        GravityCorrection();
        Braking();
    }

    /// <summary>
    ///     Whenever the player isnt on the ground, there should be an amount of gravity applied to them to ensure their descent feels snappy
    /// </summary>
    private void GravityCorrection()
    {
        // Check if the player is on the ground
        if (groundCheck.GetState() && currentGravityScale != 0)
            currentGravityScale = 1;
        // Apply an additional doward force to the player. This is our gravity correction
        else
            currentGravityScale += gravityStrength;

        // Speed up fall while crouching
        if (!standingState.Equals(StandingState.Standing) && !groundCheck.GetState())
            currentGravityScale += crouchGravityBonus;
        // Speed up even more while sliding
        if (standingState.Equals(StandingState.Sliding))
            currentGravityScale += slidingBonusGravity;

        // Update combo objective
        ComboObjective_TickSlam();

        // Apply downward force
        ApplyForce(Vector3.down, Physics.gravity.y * -currentGravityScale, ForceMode.Acceleration, "Player.GravityCorrection.Acceleration");
    }


    /// <summary>
    ///     Applies breaking whenever there is no moement input from the player
    /// </summary>
    private void Braking()
    {
        // Read the player input
        Vector2 cInput = in_move.ReadValue<Vector2>();
        // Run breaking methods
        // -> xAxis
        BrakingDirectional(cInput.x, Vector3.right);
        // -> zAxis
        BrakingDirectional(cInput.y, Vector3.forward);
    }
    /// <summary>
    ///     Handles breaking for the xAxis
    /// </summary>
    /// <param name="input">Player Input</param>
    /// <param name="direction">Defined direction</param>
    private void BrakingDirectional(float input, Vector3 direction)
    {
        // Check if our input is 0
        if (input != 0)
            return;

        // Get the players linear velocity
        Vector3 linearVelocity = Quaternion.Inverse(cameraController.GetTransform_Yaw().rotation) * GetLinearVelocity();
        linearVelocity = new Vector3(linearVelocity.x * direction.x, linearVelocity.y * direction.y, linearVelocity.z * direction.z);
        // Check if our directional velocity exceedes the breaking threshold
        if (linearVelocity.magnitude <= breakingVelocityThreshold)
            return;

        // Flip the direction velocity
        Vector3 directionVelocity = cameraController.GetTransform_Yaw().rotation * -linearVelocity;
        // Apply movement
        ApplyForce(directionVelocity, breakingSpeed, ForceMode.Force, $"Player.Breaking.{direction}");
    }

    #endregion
    #region Collision
    /// <summary>
    ///     Method for tracking information on start
    /// </summary>
    private void CollisionStart()
    {
        CollisionStart_Crouch();
    }
    /// <summary>
    ///     General collision update. Used for organization
    /// </summary>
    private void CollisionUpdate()
    {
        CollisionUpdate_Crouch();
    }

    /// <summary>
    ///     Tracks crouch information on start
    /// </summary>
    private void CollisionStart_Crouch()
    {
        CapsuleCollider capsuleCollider = (CapsuleCollider)GetCollision();
        crouch_collisionInitialHeight = capsuleCollider.height;
        crouch_collisionInitialCenter = capsuleCollider.center;
    }
    /// <summary>
    ///     Updates collision based on standing state
    /// </summary>
    private void CollisionUpdate_Crouch()
    {
        // Check our crouching state
        // -> Standing Collision
        if (standingState.Equals(StandingState.Standing))
        {
            Collision_MoveTowards(crouch_collisionInitialHeight, crouch_collisionInitialCenter, cameraController.GetInitialCenter());
        }
        // -> Crouching Collision
        else
        {
            Collision_MoveTowards(crouchHeight, crouchCenter, crouchCameraCenter);
        }
    }
    private void Collision_MoveTowards(float height, Vector3 center, Vector3 cameraCenter)
    {
        // Move Collision
        CapsuleCollider capsuleCollider = (CapsuleCollider)GetCollision();
        capsuleCollider.height = Mathf.Lerp(capsuleCollider.height, height, crouchSpeed * Time.deltaTime);
        capsuleCollider.center = Vector3.Lerp(capsuleCollider.center, center, crouchSpeed * Time.deltaTime);

        cameraController.GetParent().localPosition = Vector3.Lerp(cameraController.GetParent().localPosition, cameraCenter, crouchSpeed * Time.deltaTime);
    }
    #endregion

    #region Combo Objectives
    /// <summary>
    ///     Updates the sliding combo objective
    /// </summary>
    private void ComboObjective_TickSliding()
    {
        // Adds horizontal magintude to the combo objective sliding distance
        co_SlideDistance += GetHorizontalVelocity().magnitude;
        // When we have moved a unit, tick
        if (co_SlideDistance >= 1)
        {
            // Reduce Slide distance
            co_SlideDistance--;
            // Increase progress
            InventoryHandler.Instance.AddObjectiveProgress(comboObjective_SlideKey);
        }
    }
    /// <summary>
    ///     Updates the slam combo objective
    /// </summary>
    private void ComboObjective_TickSlam()
    {
        // Check if the player is not standing
        if (standingState.Equals(StandingState.Standing))
            return;
        // Check if the player is moving downward
        float verticalVelocity = GetVerticalVelocity();
        if (verticalVelocity >= -co_SlamThreshold)
            return;

        // Adds horizontal magintude to the combo objective sliding distance
        co_SlamDistance += Mathf.Abs(verticalVelocity);
        // When we have moved a unit, tick
        if (co_SlamDistance >= 1)
        {
            // Reduce Slide distance
            co_SlamDistance--;
            // Increase progress
            InventoryHandler.Instance.AddObjectiveProgress(comboObjective_SlamKey);
        }
    }
    /// <summary>
    ///     Updates the jump combo objective
    /// </summary>
    private void ComboObjective_TickJump()
    {
        // Increase progress
        InventoryHandler.Instance.AddObjectiveProgress(comboObjective_JumpKey);
    }
    #endregion
    #region Positioning
    public void Teleport(Vector3 position, Vector3 eyeTracking) { StartCoroutine(IEnum_Teleport(position, eyeTracking)); }
    private IEnumerator IEnum_Teleport(Vector3 position, Vector3 eyeTracking)
    {
        GetRigidbody().isKinematic = true;
        GetRigidbody().interpolation = RigidbodyInterpolation.None;
        usePhysics = false;

        ResetVelocity();
        transform.position = position;
        cameraController.ForceCameraLookAt(eyeTracking);
        yield return new WaitForSeconds(0.1f);

        GetRigidbody().isKinematic = false;
        GetRigidbody().interpolation = RigidbodyInterpolation.Interpolate;
        usePhysics = true;
    }
    #endregion

    #region String Processing
    public override string ToString()
    {
        string output = base.ToString();

        output += $"Stats\n";
        output += $"{GetStatblock()}\n";
        output += "\n";

        output += $"Rigidbody\n";

        Vector3 linearVelocity = GetRigidbody().linearVelocity;
        output += $"Current Velocity: {linearVelocity} ({linearVelocity.magnitude})\n";

        Vector3 horizontalVelocity = new Vector3(linearVelocity.x, 0, linearVelocity.z);
        output += $"Current Horizontal Velocity: {horizontalVelocity} ({horizontalVelocity.magnitude})\n";
        output += "\n";
        output += $"Current Sliding Direction: {slidingDirection}\n";
        output += "\n";
        output += $"Current Sliding Normal: {slidingIdealMomentumNormal}\n";
        output += $"Current Sliding Pivot: {slidingIdealMomentumPivot}\n";
        output += $"Current Sliding Direction: {slidingIdealMomentumDirection}\n";
        output += "\n";
        output += $"Current Sliding Speed: {slidingCurrentSpeed}\n";
        output += $"Sliding Movement Accuracy: {slidingMomentumAccuracy}\n";
        output += "\n";

        output += $"Player States\n";
        output += $"Standing State: {standingState}\n";


        return output;
    }
    #endregion
}
