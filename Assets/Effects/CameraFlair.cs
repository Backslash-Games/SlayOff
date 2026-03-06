using UnityEngine;
using Unity.Cinemachine;

public class CameraFlair : MonoBehaviour
{
    private Camera camera = null;
    [SerializeField] private CinemachineCamera cinemachineCamera = null;
    private PlayerController player = null;

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
    private void FixedUpdate()
    {
        TickCamera(); // Updates camera flair
    }
    #endregion

    #region Camera
    /// <summary>
    ///     Moves the camera methods forward by 1
    /// </summary>
    private void TickCamera()
    {
        CalculateCameraFOV();
    }
    
    #region Speed
    /// <summary>
    ///     Calculates the camera FOV based on speed
    /// </summary>
    private void CalculateCameraFOV()
    {
        // Get the current magnitude of speed
        Vector3 horizontalVelocity = GetPlayer().GetHorizontalVelocity();
        float cMagnitude = horizontalVelocity.magnitude;
        // Get the current time for slerping speed
        float cTime = Mathf.Clamp01((cMagnitude - cameraFOV_minimumVelocity) / (cameraFOV_maximumVelocity - cameraFOV_minimumVelocity));
        //-> Scale our time by accuracy to ensure a stronger result when looking towards movement
        cTime = cTime * Mathf.Clamp01(Mathm.GetVectorAccuracy(horizontalVelocity, GetCameraForward_Horizontal()) / cameraFOV_accuracyMaxThreshold);
        // Get the current and new FOV
        float currentFOV = GetCinemachineCamera().Lens.FieldOfView;
        float newFOV = Mathf.Lerp(cameraFOV_range.x, cameraFOV_range.y, cTime);

        // Move towards the current FOV
        GetCinemachineCamera().Lens.FieldOfView = Mathf.Lerp(currentFOV, newFOV, Time.deltaTime * cameraFOV_deltaScale);
    }
    #endregion

    #region Get Methods
    /// <summary>
    ///     Get the camera forward locked to horizontal
    /// </summary>
    /// <returns>Camera Forward - Horizontal</returns>
    private Vector2 GetCameraForward_Horizontal()
    {
        Vector3 cameraForward = GetCamera().transform.forward;
        return new Vector2(cameraForward.x, cameraForward.z);
    }
    #endregion
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
