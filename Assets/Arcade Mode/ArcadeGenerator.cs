using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class ArcadeGenerator : MonoBehaviour
{
    private enum BuildMode { Normal, ConnectionTest };
    private enum BuildStage { Started, Placing, Success, Failed };

    private enum Debug_StallMode { Input, Delay, Yield };

    [Header("Generation")]
    [SerializeField] private Arcade_Tileset tileset;
    [SerializeField] private BuildMode buildMode = BuildMode.Normal;
    [SerializeField] private BuildStage stage = BuildStage.Failed;
    [Space]
    [SerializeField] private int seed = -1;
    [SerializeField] private int length = 10;
    [SerializeField] private Vector2Int lengthRange = new Vector2Int(3, 15);
    [SerializeField] private int maxDoorChecks = 12;


    [Header("Variables")]
    [SerializeField] private int currentFloor = 0;
    [SerializeField] private Transform cFloorParent = null;
    [Space]
    [SerializeField] private List<Arcade_Tile> tiles = new List<Arcade_Tile>();
    [SerializeField] private List<Arcade_Room> rooms = new List<Arcade_Room>();
    private PlayerController player = null;
    [Space(50)]

    [Header("Debug Stalling")]
    [SerializeField] private Debug_StallMode stallMode = Debug_StallMode.Delay;
    [SerializeField] private int ds_delay = 5;
    [SerializeField] private InputActionAsset PlayerActions;
    private InputAction in_jump;

    [Header("Connection Test")]
    [SerializeField] private int ct_otherDoorIndex = 0;


    public delegate void BuildPhase(int currentFloor, Transform cFloorParent, Arcade_Tile[] tiles, Arcade_Room[] rooms);
    public event BuildPhase OnGenerationStarted;
    public event BuildPhase OnGenerationPlacing;
    public event BuildPhase OnGenerationSuccess;
    public event BuildPhase OnGenerationFailed;


    #region Unity Methods
    private async void OnEnable()
    {
        BindInput();

        UpdateSeed();
        await Generate();
    }
    private async void OnDisable()
    {
        UnbindInput();

        await ClearGeneration();
    }
    #endregion
    #region Generate Overview
    public async void GenerateNew()
    {
        await ClearGeneration();
        await Generate();
    }
    private async Task Generate()
    {
        StageStarted();

        switch (buildMode)
        {
            case BuildMode.ConnectionTest:
                await GenDebug_ConnectionTest();
                break;

            default:
                await GenerateNewFloor();
                break;
        }

        StageSuccess();
    }

    private async Task ClearGeneration()
    {
        // Destroy transform
        Destroy(cFloorParent.gameObject);
        cFloorParent = null;

        // Clear lists
        tiles.Clear();
        rooms.Clear();

        await Task.Yield();
    }
    #endregion

    #region Generation
    /// <summary>
    ///     Parent for floor generation
    /// </summary>
    private async Task GenerateNewFloor()
    {
        StagePlacing();

        // Ensure floor parent always exists
        CreateNewFloorParent();
        // Set difficulty
        length = Mathf.Clamp(currentFloor, lengthRange.x, lengthRange.y);

        // Place spawn room
        await PlaceTile(tileset.GetSpawnRoom());
        // Spawn tiles until there is no more length
        for (int i = 0; i < length; i++)
        {
            await PlaceCombo(tileset.GetRandom_Hall(), tileset.GetRandom_Room());
            if (stage.Equals(BuildStage.Failed))
                break;
        }
        // Place goal room
        if (!stage.Equals(BuildStage.Failed))
            await PlaceCombo(tileset.GetRandom_Hall(), tileset.GetGoalRoom());

        // Check for a failed generation
        if (stage.Equals(BuildStage.Failed))
        {
            currentFloor--;
            await ClearGeneration();
            await Generate();
            return;
        }
    }

    #region Floor Handling
    /// <summary>
    ///     Creates a new floor parent
    /// </summary>
    private void CreateNewFloorParent()
    {
        currentFloor++;
        cFloorParent = (new GameObject($"Floor - {currentFloor}")).transform;
        cFloorParent.transform.parent = transform;
    }

    /// <summary>
    ///     Gets the floor parent, creates new if one doesnt exist
    /// </summary>
    /// <returns></returns>
    private Transform GetFloorParent()
    {
        if (cFloorParent == null)
            CreateNewFloorParent();
        return cFloorParent;
    }
    #endregion
    #region Tile Handling
    private async Task PlaceCombo(Arcade_Tile hallway, Arcade_Tile room)
    {
        // Keep track of generation flags
        bool success = false;
        int error_out = 0;

        while (!success)
        {
            await DebugStall($"PlaceCombo.1 -> Initial Loop...\nError: {error_out}");
            // Check to see if we have  overflowed
            if (error_out >= maxDoorChecks)
            {
                await DebugStall("PlaceCombo.2.f -> Failure Overflow");

                Debug.LogError("Generation completely failed - Trying again");
                StageFailed();
                break;
            }

            await PlaceTile(hallway);
            // Check if place tile completed properly
            if (!pt_status)
            {
                error_out++;
                rooms[rooms.Count - 1].Error_LastDoor();
                continue;
            }

            await PlaceTile(room);
            // Check if place tile completed properly
            if (!pt_status)
            {
                error_out++;
                RemoveListings(rooms.Count - 1);
                rooms[rooms.Count - 1].Error_LastDoor();
                continue;
            }

            success = true;
        }
    }

    bool pt_status = false;
    /// <summary>
    ///     Places a tile
    /// </summary>
    /// <param name="tile">Tile to Place</param>
    private async Task PlaceTile(Arcade_Tile tile, bool autoConnect = true)
    {
        await DebugStall($"PlaceTile.1 -> Placing tile {tile.GetIdentification()}");

        // Set status flag
        pt_status = false;

        // Spawn prefab and add to list
        GameObject spawnedRoomObject = Instantiate(tile.GetPrefab(), GetFloorParent());
        Arcade_Room cRoom = spawnedRoomObject.GetComponent<Arcade_Room>();
        // Connect rooms
        if (rooms.Count > 0 && autoConnect)
            ConnectRooms(cRoom, rooms[rooms.Count - 1]);

        // After room is setup make sure that bounds dont intersect
        if (CheckOverlap(cRoom.GetBounds_World()) && autoConnect)
        {
            Debug.LogError("FOUND OVERLAP");
            Destroy(spawnedRoomObject);
            return;
        }

        // Initialize assets
        await DebugStall($"PlaceTile.2 -> Initial pass for {tile.GetIdentification()} complete, initializing mesh");
        cRoom.InitializeMeshAssets();
        await DebugStall($"PlaceTile.3 -> {tile.GetIdentification()} initializing props");
        cRoom.InitializePropAssets();

        // Add information to lists
        rooms.Add(cRoom);
        tiles.Add(tile);

        // Set status flag
        pt_status = true;
    }
    #endregion
    #region Room Handling
    /// <summary>
    ///     Connects two tiles, other to stationary
    /// </summary>
    /// <param name="stationary">Arcade Tile - Stationary</param>
    /// <param name="other">Arcade Tile - Will be moved</param>
    private void ConnectRooms(Arcade_Room other, Arcade_Room stationary, int otherDoorIndex = -1, int stationaryDoorIndex = -1)
    {
        // Error check
        if (other == null)
        {
            Debug.LogError("Other room is not set");
            return;
        }
        if (stationary == null)
        {
            Debug.LogError("Stationary room is not set");
            return;
        }



        // Get the stationary door
        Arcade_Door sDoor = stationaryDoorIndex < 0 ? stationary.GetAvaliableDoor() : stationary.GetDoor(stationaryDoorIndex);
        // Get the opposing door
        Arcade_Door oDoor = otherDoorIndex < 0 ? other.GetAvaliableDoor() : other.GetDoor(otherDoorIndex);



        // Error check
        if (oDoor == null)
        {
            Debug.LogError("Other door could not be found");
            return;
        }
        if (sDoor == null)
        {
            Debug.LogError("Stationary door could not be found");
            return;
        }



        // Allign door rotations
        Vector3 od_Forward = oDoor.GetOutFace_World();
        Vector3 sd_Forward = sDoor.GetOutFace_World();
        other.transform.rotation = Quaternion.LookRotation(Quaternion.FromToRotation(od_Forward, -sd_Forward) * Vector3.forward, Vector3.up);

        // Connect the doors
        // -> Set the other position equal to the stationary door
        other.transform.position = sDoor.transform.position;
        // -> Offset others position by the difference between oDoor and center
        other.transform.position += other.transform.position - oDoor.transform.position;

        // Set door states
        oDoor.SetUnavaliable();
        sDoor.SetUnavaliable();
    }
    /// <summary>
    ///     Remove the room from alive
    /// </summary>
    /// <param name="room">Input Room</param>
    private void RemoveListings(int index)
    {
        // Error check
        if (rooms.Count <= index)
            return;
        if (tiles.Count <= index)
            return;


        // Remove room
        Arcade_Room cRoom = rooms[index];
        rooms.RemoveAt(index);
        Destroy(cRoom.gameObject);
        // Remove Tile
        tiles.RemoveAt(index);
    }
    #endregion
    #region Bound Handling
    /// <summary>
    ///     Checks if two room bounds are intersecting
    /// </summary>
    /// <param name="other">Input bounds</param>
    /// <returns>True if intersecting</returns>
    private bool CheckOverlap(Bounds other)
    {
        foreach(Arcade_Room room in rooms)
        {
            if (room.CheckOverlap(other))
                return true;
        }
        return false;
    }
    #endregion
    #endregion

    #region Randomization
    private void UpdateSeed()
    {
        if (seed == -1)
        {
            int setSeed = (int)System.DateTime.Now.Ticks;
            Random.InitState(setSeed);
            Debug.Log($"Setting seed to {setSeed}");
            return;
        }
        Random.InitState(seed);
    }
    #endregion
    #region Stage Sequencing
    /// <summary>
    ///     Sets the stage to a new value
    /// </summary>
    /// <param name="stage">New Stage</param>
    /// <param name="force">Force sets if true</param>
    private void SetStage(BuildStage stage, bool force = false) 
    {
        if (!force && this.stage.Equals(BuildStage.Failed))
            return;
        this.stage = stage; 
    }


    private void StageStarted() { SetStage(BuildStage.Started, true); OnGenerationStarted?.Invoke(currentFloor, cFloorParent, tiles.ToArray(), rooms.ToArray()); }
    private void StagePlacing() { SetStage(BuildStage.Placing); OnGenerationPlacing?.Invoke(currentFloor, cFloorParent, tiles.ToArray(), rooms.ToArray()); }
    private void StageSuccess() { SetStage(BuildStage.Success); OnGenerationSuccess?.Invoke(currentFloor, cFloorParent, tiles.ToArray(), rooms.ToArray()); }
    private void StageFailed() { SetStage(BuildStage.Failed); OnGenerationFailed?.Invoke(currentFloor, cFloorParent, tiles.ToArray(), rooms.ToArray()); }
    #endregion
    #region Generation Debugging
    private async Task GenDebug_ConnectionTest()
    {
        // Place 5 tiles
        await PlaceTile(tileset.GetRandom_Room(), false);
        await PlaceTile(tileset.GetRandom_Room(), false);
        await PlaceTile(tileset.GetRandom_Room(), false);
        await PlaceTile(tileset.GetRandom_Room(), false);
        await PlaceTile(tileset.GetRandom_Room(), false);

        // Run connections
        ConnectRooms(rooms[1], rooms[0], ct_otherDoorIndex, 0);
        ConnectRooms(rooms[2], rooms[0], ct_otherDoorIndex, 1);
        ConnectRooms(rooms[3], rooms[0], ct_otherDoorIndex, 2);
        ConnectRooms(rooms[4], rooms[0], ct_otherDoorIndex, 3);
    }

    private async Task DebugStall(string log)
    {
        switch (stallMode)
        {
            case Debug_StallMode.Input:
                await CheckInput(log);
                break;
            case Debug_StallMode.Yield:
                Debug.Log(log);
                await Task.Delay(ds_delay);
                break;

            default:
                Debug.Log(log);
                await Task.Delay(ds_delay);
                break;
        }
    }

    private async Task CheckInput(string log)
    {
        while (in_jump.ReadValue<float>() != 0)
        {
            //Debug.Log("Please let go of space");
            await Task.Yield();
        }
        Debug.Log(log);
        while (in_jump.ReadValue<float>() != 1)
            await Task.Yield();
    }
    private void BindInput()
    {
        PlayerActions.FindActionMap("Control").Enable();
        in_jump = PlayerActions.FindAction("Jump");
    }
    private void UnbindInput()
    {
        PlayerActions.FindActionMap("Control").Disable();
    }
    #endregion
    #region Player Handling
    private PlayerController GetPlayer()
    {
        if (player == null)
            player = FindAnyObjectByType<PlayerController>();
        return player;
    }
    #endregion

    #region Get Methods
    public int GetCurrentFloor() { return currentFloor; }
    #endregion
}
