using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public class CameraController : MonoBehaviour
{
    // Singleton
    private static CameraController _instance;
    public static CameraController Instance { get { return _instance; } }


    private Camera camera = null;
    [SerializeField] private CinemachineCamera cinemachineCamera = null;
    private PlayerController player = null;

    [Header("General")]
    [SerializeField] private Transform parent;
    [Space]
    [SerializeField] private Transform yaw;
    [SerializeField] private Transform pitch;
    [SerializeField] private Transform roll;
    [Space]
    [SerializeField] private float horizontalSensitivity = 5.5f;
    [SerializeField] private float verticalSensitivity = 5.5f;

    private float vertical_bounds = 87.5f;
    private float smoothing_scale = 25;
    private Vector3 initial_center = Vector3.zero;

    // The keyword CURRENT denotes the actual view of the camera
    private float current_yaw = 0;
    private float current_pitch = 0;
    private float current_roll = 0;

    // The keyword MOD denotes the view of the camera after modifiers are applied
    private float mod_yaw = 0;
    private float mod_pitch = 0;
    private float mod_roll = 0;

    [Header("Modifiers")]
    [SerializeField] private List<CameraModifier> modifiers = new List<CameraModifier>();
    private float alive_threshold = 0.025f;


    // Field of view
    private static readonly Vector2 cameraFOV_range = new Vector2(60, 160);
    private static readonly float cameraFOV_minimumVelocity = 6; // Minimum velocity the player has to reach in order for FOV to change
    private static readonly float cameraFOV_maximumVelocity = 32; // Maximum velocity the player can reach with FOV change
    private static readonly float cameraFOV_deltaScale = 1.5f;
    private static readonly float cameraFOV_accuracyMaxThreshold = 0.525f;

    #region Dependencies
    /// <summary>
    ///     Gets the player
    /// </summary>
    /// <returns>Player Controller</returns>
    private PlayerController GetPlayer()
    {
        if (player == null)
            player = FindAnyObjectByType<PlayerController>();
        return player;
    }
    /// <summary>
    ///     Gets the Camera
    /// </summary>
    /// <returns>Camera</returns>
    private Camera GetCamera()
    {
        if (camera == null)
            camera = Camera.main;
        return camera;
    }
    /// <summary>
    ///     Gets the cinemachineCamera
    /// </summary>
    /// <returns>CinemachineCamera</returns>
    private CinemachineCamera GetCinemachineCamera()
    {
        if (cinemachineCamera == null)
        {
            Debug.LogError("Cinemachine Camera must be set in order to use Camera Flair");
            return null;
        }
        return cinemachineCamera;
    }
    #endregion
    #region Unity Methods
    private void Awake()
    {
        CreateSingleton();
    }
    private void Start()
    {
        CameraStart();
    }
    private void LateUpdate()
    {
        // Run fov
        CalculateCameraFOV();

        // Incorporate modifiers into camera location
        Tick_CameraModifiers();
        // Move camera to desired location
        LerpCamera();
    }
    #endregion
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

    #region Sequences
    /// <summary>
    ///     Runs information for camera start
    /// </summary>
    private void CameraStart()
    {
        // Make sure camera parent is set
        if (parent != null)
            initial_center = parent.localPosition;
    }
    #endregion

    #region General
    /// <summary>
    ///     Moves the camera to the desired yaw and pitch
    /// </summary>
    public void LerpCamera()
    {
        // Set the current yaw
        yaw.localRotation = Quaternion.Lerp(yaw.localRotation, Quaternion.AngleAxis(mod_yaw, Vector3.up), Time.deltaTime * smoothing_scale);
        // Set the current pitch
        pitch.localRotation = Quaternion.Lerp(pitch.localRotation, Quaternion.AngleAxis(mod_pitch, Vector3.right), Time.deltaTime * smoothing_scale);
        // Set the current roll
        roll.localRotation = Quaternion.Lerp(roll.localRotation, Quaternion.AngleAxis(mod_roll, Vector3.forward), Time.deltaTime * smoothing_scale);
    }

    /// <summary>
    ///     Pushes the yaw and pitch based on an input direction
    /// </summary>
    /// <param name="direction">Input Vector2 direction</param>
    public void PushCameraAxis(Vector2 direction)
    {
        // Rotate Yaw
        // -> Modify yaw
        current_yaw += direction.x * horizontalSensitivity;
        current_yaw = current_yaw % 360;

        // Rotate Pitch
        // -> Modify pitch
        current_pitch += -direction.y * verticalSensitivity;
        current_pitch = Mathf.Clamp(current_pitch, -vertical_bounds, vertical_bounds);
    }
    #endregion
    #region Speed
    /// <summary>
    ///     Calculates the camera FOV based on speed
    /// </summary>
    private void CalculateCameraFOV()
    {
        // Get the current magnitude of speed
        Vector3 simVelocity = GetPlayer().GetLinearVelocity();
        Vector3 cameraForward = GetCameraForward();
        // -> Pull magnitude
        float cMagnitude = simVelocity.magnitude;
        // Get the current time for slerping speed
        float cTime = Mathf.Clamp01((cMagnitude - cameraFOV_minimumVelocity) / (cameraFOV_maximumVelocity - cameraFOV_minimumVelocity));
        //-> Scale our time by accuracy to ensure a stronger result when looking towards movement
        cTime = cTime * Mathf.Clamp01(Mathm.GetVectorAccuracy(simVelocity, cameraForward) / cameraFOV_accuracyMaxThreshold);
        // Get the current and new FOV
        float currentFOV = GetCinemachineCamera().Lens.FieldOfView;
        float newFOV = Mathf.Lerp(cameraFOV_range.x, cameraFOV_range.y, cTime);

        // Move towards the current FOV
        GetCinemachineCamera().Lens.FieldOfView = Mathf.Lerp(currentFOV, newFOV, Time.deltaTime * cameraFOV_deltaScale);
    }
    #endregion
    #region Modifiers
    private void Tick_CameraModifiers()
    {
        Modifier_Setup();

        Modifier_ApplyAxis(CameraModifier.Axis.yaw, ref mod_yaw);
        Modifier_ApplyAxis(CameraModifier.Axis.pitch, ref mod_pitch);
        Modifier_ApplyAxis(CameraModifier.Axis.roll, ref mod_roll);
    }

    private void Modifier_Setup()
    {
        mod_yaw = current_yaw;
        mod_pitch = current_pitch;
        mod_roll = current_roll;
    }

    private void Modifier_ApplyAxis(CameraModifier.Axis axis, ref float value)
    {
        // Roll through all modifiers
        for (int i = 0; i < modifiers.Count; i++)
        {
            // Check if the current value is our target axis
            if (modifiers[i].GetAxis().Equals(axis))
            {
                //  Apply influence
                value += modifiers[i].GetInfluence();

                // Reduce influence
                modifiers[i].ReduceInfluence();
                // Check if value needs to be removed
                if (Mathf.Abs(modifiers[i].GetInfluence()) <= alive_threshold)
                {
                    modifiers.RemoveAt(i);
                    i--;
                }
            }
        }
    }

    public void AddModifier(CameraModifier modifier) { modifiers.Add(modifier); }

    #region Tools
    /// <summary>
    ///     Adds modifiers to simulate a whiplash effect
    /// </summary>
    public void AddModifierWhiplash(Vector3 origin, float strength, float reduction_rate)
    {
        // Calculate the vector accuracy between direction of input and camera forward
        float front_accuracy = Mathm.GetVectorAccuracy(camera.transform.forward, camera.transform.position - origin);
        float side_accuracy = Mathm.GetVectorAccuracy(camera.transform.right, camera.transform.position - origin);

        // Whiplash impacts pitch and roll
        // -> Calculate pitch impact
        float pitch_impact = (front_accuracy - 0.5f) * 2;
        AddModifier(new CameraModifier(CameraModifier.Axis.pitch, strength * pitch_impact, reduction_rate));
        // -> Calculate roll impact
        float roll_impact = (side_accuracy - 0.5f) * 2;
        AddModifier(new CameraModifier(CameraModifier.Axis.roll, strength * -roll_impact, reduction_rate));
    }
    #endregion
    #endregion

    #region Get Methods
    /// <summary>
    ///     Get the camera forward
    /// </summary>
    /// <returns>Camera Forward </returns>
    private Vector3 GetCameraForward()
    {
        return GetCamera().transform.forward;
    }
    /// <summary>
    ///     Get the camera forward locked to horizontal
    /// </summary>
    /// <returns>Camera Forward - Horizontal</returns>
    private Vector2 GetCameraForward_Horizontal()
    {
        Vector3 cameraForward = GetCamera().transform.forward;
        return new Vector2(cameraForward.x, cameraForward.z);
    }

    public Transform GetParent() { return parent; }

    public Transform GetTransform_Pitch() { return pitch; }
    public Transform GetTransform_Yaw() { return yaw; }

    public float GetCurrentPitch() { return current_pitch; }
    public float GetCurrentYaw() { return current_yaw; }

    public Vector3 GetInitialCenter() { return initial_center; }
    #endregion
    #region Set Methods
    public void ForceCameraLookAt(Vector3 position)
    {
        yaw.LookAt(position);
        Vector3 cAngles = yaw.eulerAngles;

        yaw.eulerAngles = new Vector3(0, cAngles.y, 0);
        current_yaw = cAngles.y % 360;
        pitch.eulerAngles = new Vector3(cAngles.x, cAngles.y, 0);
        current_pitch = cAngles.x % 360;
    }

    public void SetSensitivity(float horizontal, float vertical)
    {
        horizontalSensitivity = horizontal;
        verticalSensitivity = vertical;
    }
    #endregion

    #region Debug
    public override string ToString()
    {
        string output = "";

        output += "FOV\n";
        output += $"Current FOV {GetCinemachineCamera().Lens.FieldOfView}\n";
        float vva = Mathm.GetVectorAccuracy(GetPlayer().GetHorizontalVelocity(), GetCameraForward_Horizontal());
        output += $"Velocty-View Accuracy {vva} || Scaled with threshold({cameraFOV_accuracyMaxThreshold}) {Mathf.Clamp01(vva / cameraFOV_accuracyMaxThreshold)}\n";
        output += $"Range {cameraFOV_range} || Min-Velocity {cameraFOV_minimumVelocity} || Max-Velocity {cameraFOV_maximumVelocity} || Delta Scale {cameraFOV_deltaScale}\n";

        return output;
    }
    #endregion
}