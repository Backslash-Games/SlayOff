using System.Collections.Generic;
using UnityEngine;

public class ArcadeGenerator : MonoBehaviour
{
    private enum BuildMode { Normal, ConnectionTest };

    [Header("Generation")]
    [SerializeField] private Arcade_Tileset tileset;
    [SerializeField] private BuildMode buildMode = BuildMode.Normal;
    [Space]
    [SerializeField] private int length = 10;


    [Header("Variables")]
    [SerializeField] private int currentFloor = 0;
    [SerializeField] private Transform cFloorParent = null;
    [Space]
    [SerializeField] private List<Arcade_Tile> tiles = new List<Arcade_Tile>();
    [SerializeField] private List<Arcade_Room> rooms = new List<Arcade_Room>();
    [Space(50)]

    [Header("Connection Test")]
    [SerializeField] private int ct_otherDoorIndex = 0;

    #region Unity Methods
    private void OnEnable()
    {
        Generate();
    }
    private void OnDisable()
    {
        ClearGeneration();
    }
    #endregion
    #region Generate Overview
    private void Generate()
    {
        switch (buildMode)
        {
            case BuildMode.ConnectionTest:
                GenDebug_ConnectionTest();
                break;

            default:
                GenerateNewFloor();
                break;
        }
    }

    private void ClearGeneration()
    {
        // Destroy transform
        Destroy(cFloorParent.gameObject);
        cFloorParent = null;

        // Clear lists
        tiles.Clear();
        rooms.Clear();
    }
    #endregion

    #region Generation
    /// <summary>
    ///     Parent for floor generation
    /// </summary>
    private void GenerateNewFloor()
    {
        // Ensure floor parent always exists
        CreateNewFloorParent();
        
        PlaceTile(tileset.GetSpawnRoom());

        // Spawn tiles until there is no more length
        for (int i = 1; i < length; i++)
        {
            if (i % 2 == 1)
                PlaceTile(tileset.GetRandom_Hall());
            else
                PlaceTile(tileset.GetRandom_Room());
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
    /// <summary>
    ///     Places a tile
    /// </summary>
    /// <param name="tile">Tile to Place</param>
    private void PlaceTile(Arcade_Tile tile, bool autoConnect = true)
    {
        // Spawn prefab and add to list
        GameObject spawnedRoomObject = Instantiate(tile.GetPrefab(), GetFloorParent());
        Arcade_Room cRoom = spawnedRoomObject.GetComponent<Arcade_Room>();
        // Connect rooms
        if (rooms.Count > 0 && autoConnect)
            ConnectRooms(cRoom, rooms[rooms.Count - 1]);

        // Add information to lists
        rooms.Add(cRoom);
        tiles.Add(tile);
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
        // Get the stationary door
        Arcade_Door sDoor = stationaryDoorIndex < 0 ? stationary.GetAvaliableDoor() : stationary.GetDoor(stationaryDoorIndex);
        // Get the opposing door
        Arcade_Door oDoor = otherDoorIndex < 0 ? other.GetAvaliableDoor() : other.GetDoor(otherDoorIndex);

        // Allign door rotations
        Vector3 od_Forward = (oDoor.GetOutFace_World());
        Vector3 sd_Forward = (sDoor.GetOutFace_World());
        other.transform.rotation = Quaternion.LookRotation(Quaternion.FromToRotation(od_Forward, -sd_Forward) * Vector3.forward, Vector3.up);

        // Connect the doors
        // -> Set the other position equal to the stationary door
        other.transform.position = sDoor.transform.position;
        // -> Offset others position by the difference between oDoor and center
        other.transform.position += sDoor.transform.position - stationary.transform.position;
        // -> Correct based on door height
        other.transform.position -= Vector3.up * sDoor.GetSize().y;

        // Set door states
        oDoor.SetUnavaliable();
        sDoor.SetUnavaliable();
    }
    #endregion
    #endregion

    #region Generation Debugging
    private void GenDebug_ConnectionTest()
    {
        // Place 5 tiles
        PlaceTile(tileset.GetRandom_Room(), false);
        PlaceTile(tileset.GetRandom_Room(), false);
        PlaceTile(tileset.GetRandom_Room(), false);
        PlaceTile(tileset.GetRandom_Room(), false);
        PlaceTile(tileset.GetRandom_Room(), false);

        // Run connections
        ConnectRooms(rooms[1], rooms[0], ct_otherDoorIndex, 0);
        ConnectRooms(rooms[2], rooms[0], ct_otherDoorIndex, 1);
        ConnectRooms(rooms[3], rooms[0], ct_otherDoorIndex, 2);
        ConnectRooms(rooms[4], rooms[0], ct_otherDoorIndex, 3);
    }
    #endregion
}
