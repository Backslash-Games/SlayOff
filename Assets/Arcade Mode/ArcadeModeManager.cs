using UnityEngine;

public class ArcadeModeManager : MonoBehaviour
{
    // Singleton
    private static ArcadeModeManager _instance;
    public static ArcadeModeManager Instance { get { return _instance; } }

    [Header("General")]
    [SerializeField] private ArcadeGenerator generator = null;
    [SerializeField] private ArcadeResultsHandler results = null;
    private PlayerController player = null;

    private int cf_index = 0;
    private Transform cf_parent = null;
    private Arcade_Tile[] cf_tiles = new Arcade_Tile[0];
    private Arcade_Room[] cf_rooms = new Arcade_Room[0];

    [Header("Waypoint Tracking")]
    [SerializeField] private Elevator[] elevators = new Elevator[3]; // Should always be 3 entries

    [Header("Player Tracking")]
    [SerializeField] private bool ptrackingEnabled = false;
    [Space]
    [SerializeField] private string pt_roomId = "";
    [SerializeField] private int pt_lastKnownIndex = 0;

    public delegate void RoomTracking(Arcade_Room room);
    public event RoomTracking OnRoomChanged;
    public event RoomTracking OnRoomEntered;
    public event RoomTracking OnRoomLeft;

    public delegate void ModeSequence();
    public event ModeSequence OnPlayerStaged;

    #region Unity Methods
    private void Awake()
    {
        CreateSingleton();

        PT_Awake();
        RH_Awake();
        WP_Awake();
    }
    private void OnDestroy()
    {
        PT_Destroy();
        RH_Destroy();
        WP_Destroy();
    }

    private void LateUpdate()
    {
        PT_Tick();
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
    #region Player Tracking
    /// <summary>
    ///     Setup for player tracking
    /// </summary>
    private void PT_Awake()
    {
        GetArcadeGenerator().OnGenerationSuccess += SetGenerationVariables;

        GetArcadeGenerator().OnGenerationStarted += (_, _, _, _) => PT_SetActive(false);
        GetArcadeGenerator().OnGenerationSuccess += (_, _, _, _) => PT_SetActive(true);
    }
    /// <summary>
    ///     Setup for player tracking
    /// </summary>
    private void PT_Destroy()
    {
        GetArcadeGenerator().OnGenerationSuccess -= SetGenerationVariables;

        GetArcadeGenerator().OnGenerationStarted -= (_, _, _, _) => PT_SetActive(false);
        GetArcadeGenerator().OnGenerationSuccess -= (_, _, _, _) => PT_SetActive(true);
    }
    /// <summary>
    ///     Handled through late update
    /// </summary>
    private void PT_Tick()
    {
        // Check if player tracking is enabled
        if (!ptrackingEnabled)
            return;

        // Run logic
        PT_TrackPlayer();
    }

    /// <summary>
    ///     Tracks the players current location
    /// </summary>
    private void PT_TrackPlayer()
    {
        // Hold a reference for current depth
        int cDepth = 0;
        int cOffset = pt_lastKnownIndex;
        // Always start from the last known index, work our way out
        for (int i = 0; i < cf_rooms.Length; i++) 
        {
            // Evaluate our current (target) index
            int cIndex = cOffset + (cDepth * (i % 2 == 1 ? 1 : -1));
            // Check the targeted room
            if (PT_CheckRoom(cIndex))
            {
                PT_SetCurrent(cIndex);
                break;
            }

            // Check if we need to increase our depth
            if ((i + 1) % 2 == 1)
                cDepth++;
        }
    }

    /// <summary>
    ///     Checks if the player is in a room using room index
    /// </summary>
    /// <param name="roomIndex">Input room index</param>
    /// <returns>True if player is in room</returns>
    private bool PT_CheckRoom(int roomIndex)
    {
        // Interpret the room to array bounds
        int cIndex = roomIndex % cf_rooms.Length;
        // -> Check for a negative
        if (cIndex < 0) cIndex = cf_rooms.Length + (roomIndex % cf_rooms.Length);

        Arcade_Room cRoom = cf_rooms[cIndex];
        // Check bounds
        return cRoom.GetPlayerBounds_World().Contains(GetPlayer().transform.position);
    }
    /// <summary>
    ///     Sets the players current room
    /// </summary>
    /// <param name="roomIndex"></param>
    private void PT_SetCurrent(int roomIndex)
    {
        // Error check 
        if(cf_rooms.Length != cf_tiles.Length)
        {
            Debug.LogError("ArcadeModeManager -> Tile and Room arrays have different lengths, please check generation for errors");
            return;
        }

        // Interpret the room to array bounds
        int cIndex = roomIndex % cf_rooms.Length;
        // -> Check for a negative
        if (cIndex < 0) cIndex = cf_rooms.Length + (roomIndex % cf_rooms.Length);

        Arcade_Tile cTile = cf_tiles[cIndex];
        Arcade_Room cRoom = cf_rooms[cIndex];
        

        // Check for a change in room
        if(cIndex != pt_lastKnownIndex)
        {
            // Trigger events
            OnRoomChanged?.Invoke(cRoom);
            OnRoomEntered?.Invoke(cRoom);
            OnRoomLeft?.Invoke(cf_rooms[pt_lastKnownIndex]);
        }

        // Set information
        pt_roomId = $"{cTile.GetIdentification()} :: {cRoom.name}";
        pt_lastKnownIndex = cIndex;
    }

    /// <summary>
    ///     Sets ptracking enabled to state
    /// </summary>
    /// <param name="state">New state</param>
    private void PT_SetActive(bool state) 
    { 
        ptrackingEnabled = state; 
        if(!ptrackingEnabled)
            PT_Reset(); 
    }
    /// <summary>
    ///     Resets player tracking
    /// </summary>
    private void PT_Reset() 
    {
        pt_roomId = "";
        pt_lastKnownIndex = 0;
    }
    #endregion
    #region Room Handling
    private void RH_Awake()
    {
        OnRoomEntered += RH_TriggerCombat;
    }
    private void RH_Destroy()
    {
        OnRoomEntered -= RH_TriggerCombat;
    }


    /// <summary>
    ///     Triggers combat in room
    /// </summary>
    private void RH_TriggerCombat(Arcade_Room room)
    {
        if (room == null)
            return;

        // Request room combat
        room.RequestCombat();
    }
    #endregion

    #region Waypoints
    private void WP_Awake()
    {
        GetArcadeGenerator().OnGenerationSuccess += (_, _, _, _) => WP_GatherEndpoints();
        GetArcadeGenerator().OnGenerationSuccess += (_, _, _, _) => WP_MoveToPoint(0, 1);
    }
    private void WP_Destroy()
    {
        GetArcadeGenerator().OnGenerationSuccess -= (_, _, _, _) => WP_GatherEndpoints();
        GetArcadeGenerator().OnGenerationSuccess -= (_, _, _, _) => WP_MoveToPoint(0, 1);
    }

    /// <summary>
    ///     Gets elevators at the start and finish of the stage
    /// </summary>
    private void WP_GatherEndpoints()
    {
        // Get elevators at each end of the stage
        elevators[1] = GameObject.FindGameObjectWithTag("Elevator_Start").GetComponent<Elevator>();
        elevators[2] = GameObject.FindGameObjectWithTag("Elevator_End").GetComponent<Elevator>();
    }

    /// <summary>
    ///     Move from current to new
    /// </summary>
    /// <param name="currentIndex">Our current location</param>
    /// <param name="otherIndex">Location we want to end up at</param>
    public void WP_MoveToPoint(int currentIndex, int otherIndex)
    {
        if(otherIndex >= elevators.Length)
        {
            Debug.Log($"Point index is out of bounds :: index:{otherIndex} length:{elevators.Length}");
            return;
        }
        // Get the elevator at index
        Elevator nElevator = elevators[otherIndex];
        // Make sure our new position isnt null
        if (nElevator == null)
        {
            Debug.Log($"No point set :: index:{otherIndex}");
            return;
        }
        // Move the player to the new point
        Vector3 position = nElevator.MirrorVectorPosition(GetPlayer().transform.position, elevators[currentIndex]);
        Vector3 cameraOrientation = nElevator.MirrorVectorPosition(Camera.main.transform.position + (Camera.main.transform.forward), elevators[currentIndex]);
        GetPlayer().Teleport(position, cameraOrientation);

        // Run on staging player
        OnPlayerStaged?.Invoke();
    }
    #endregion
    #region Results
    #endregion

    #region Get Methods
    public ArcadeGenerator GetArcadeGenerator()
    {
        if (generator == null)
            generator = FindAnyObjectByType<ArcadeGenerator>();
        return generator;
    }
    private PlayerController GetPlayer()
    {
        if (player == null)
            player = FindAnyObjectByType<PlayerController>();
        return player;
    }
    public ArcadeResultsHandler GetArcadeResults()
    {
        if (results == null)
            results = FindAnyObjectByType<ArcadeResultsHandler>();
        return results;
    }
    #endregion
    #region Set Methods
    private void SetGenerationVariables(int currentFloor, Transform cFloorParent, Arcade_Tile[] tiles, Arcade_Room[] rooms)
    {
        cf_index = currentFloor;
        cf_parent = cFloorParent;
        cf_tiles = tiles;
        cf_rooms = rooms;
    }
    #endregion
}
