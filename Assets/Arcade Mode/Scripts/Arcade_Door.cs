using UnityEngine;

public class Arcade_Door : MonoBehaviour
{
    [Header("Attributes")]
    [SerializeField] private OpenState state = OpenState.Air;
    [SerializeField] private OpenState unavaliableState = OpenState.Air;
    private enum OpenState { Open, Sealed, Air };
    [Space]

    [Tooltip("A door is marked as a connector when it is used as a spatial marking. Will only connect with doors marked as !isConnector")]
    [SerializeField] private bool isConnector;

    [Tooltip("Flag to control if the door is avaliable for further generation")]
    [SerializeField] private bool avaliable = true;

    [Tooltip("Doors will only be able to connect with other doors of the same size.")]
    [SerializeField] private Vector2 size;

    [Tooltip("Defines the direction the outside of the door is facing")]
    [SerializeField] private Vector3 outFace;

    [Header("Graphical")]
    [SerializeField] private GameObject[] stateGraphics = new GameObject[3];

    #region Unity Methods
    private void Awake()
    {
        SetGraphic_OnState();
    }
    private void OnDrawGizmos()
    {
        // Draw door bounds
        Gizmos.color = Color.blue;
        Bounds cBounds = GetDoorBounds();
        Gizmos.DrawWireMesh(Resources.GetBuiltinResource<Mesh>("Cube.fbx"), cBounds.center, transform.rotation, cBounds.size);

        // Draw door bounds, slightly smaller to help with visibility
        Gizmos.color = isAvaliable() ? Color.cyan : Color.red;
        Gizmos.DrawWireMesh(Resources.GetBuiltinResource<Mesh>("Cube.fbx"), cBounds.center, transform.rotation, cBounds.size * 0.9f);

        // Draw door out face
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + (GetOutFace_World() * 4f));
    }
    #endregion

    #region State Handling
    /// <summary>
    ///     Sets the door as unavaliable. 
    /// </summary>
    public void SetUnavaliable()
    {
        avaliable = false;
        state = unavaliableState;
        SetGraphic_OnState();
    }

    public void SetSealed()
    {
        state = OpenState.Sealed;
        SetGraphic_OnState();
    }
    #endregion
    #region Graphical
    /// <summary>
    ///     Sets the graphics based on state
    /// </summary>
    private void SetGraphic_OnState()
    {
        // Set all graphics as inactive
        HideAllGraphics();
        // Set state graphic to true
        UnhideGraphic((int)state);
    }
    /// <summary>
    ///     Hides all graphics
    /// </summary>
    private void HideAllGraphics()
    {
        // Set all graphics that are not null to false
        foreach (GameObject graphic in stateGraphics)
            if (graphic != null)
                graphic.SetActive(false);
    }
    private void UnhideGraphic(int value)
    {
        // Make sure current state is in graphical bounds
        if (stateGraphics.Length <= value)
            return;

        // Unhide the graphic if it is not null
        if (stateGraphics[value] != null)
            stateGraphics[value].SetActive(true);
    }
    #endregion

    #region Get Methods
    /// <summary>
    ///     Gets the bounds
    /// </summary>
    /// <returns>Bounds in world space</returns>
    public Bounds GetDoorBounds()
    {
        // Get the bounds center
        Vector3 bCenter = transform.position;
        // Get the bounds size
        Vector3 bSize = size;

        return new Bounds(bCenter, bSize);
    }

    /// <summary>
    ///     Gets avaliablility
    /// </summary>
    /// <returns>True if avaliable</returns>
    public bool isAvaliable() { return avaliable; }
    /// <summary>
    ///     Gets the out face in respect to world
    /// </summary>
    public Vector3 GetOutFace_World() { return transform.rotation * outFace.normalized; }
    /// <summary>
    ///     Gets the size
    /// </summary>
    /// <returns>Vector 3</returns>
    public Vector3 GetSize() { return size; }
    #endregion
}
