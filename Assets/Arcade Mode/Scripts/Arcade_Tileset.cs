using UnityEngine;

[CreateAssetMenu(fileName = "New Tileset", menuName = "SlayOff/Arcade/New Tileset")]
public class Arcade_Tileset : ScriptableObject
{
    [Header("Specifications")]
    [SerializeField] private Arcade_Tile spawnRoom;
    [SerializeField] private Arcade_Tile goalRoom;

    [Header("Spawn Pool")]
    [SerializeField] private Arcade_Tile[] rooms;
    [SerializeField] private Arcade_Tile[] halls;

    #region Get Methods
    #region Specifications
    /// <summary>
    ///     Gets the spawn room
    /// </summary>
    /// <returns>Arcade Tile - Spawn Room</returns>
    public Arcade_Tile GetSpawnRoom()
    {
        // Check if the spawn room is set
        if (spawnRoom == null)
            return GetRandom_Room();
        return spawnRoom;
    }
    /// <summary>
    ///     Gets the spawn room
    /// </summary>
    /// <returns>Arcade Tile - Goal Room</returns>
    public Arcade_Tile GetGoalRoom()
    {
        // Check if the spawn room is set
        if (goalRoom == null)
            return GetRandom_Room();
        return goalRoom;
    }
    #endregion
    #region Random
    /// <summary>
    ///     Gets a random room tile
    /// </summary>
    /// <returns>Arcade Tile - Room</returns>
    public Arcade_Tile GetRandom_Room()
    {
        // Get a random arcade tile
        Arcade_Tile rTile = GetRandom(rooms);

        // Error check
        if (rTile == null)
            Debug.Log("Room values are not set properly");
        return rTile;
    }
    /// <summary>
    ///     Gets a random hall tile
    /// </summary>
    /// <returns>Arcade Tile - Hall</returns>
    public Arcade_Tile GetRandom_Hall()
    {
        // Get a random arcade tile
        Arcade_Tile rTile = GetRandom(halls);

        // Error check
        if (rTile == null)
            Debug.Log("Hall values are not set properly");
        return rTile;
    }
    
    /// <summary>
    ///     Pulls a random tile from a set of values
    /// </summary>
    /// <param name="values">Arcade Tiles - Array</param>
    /// <returns>Arcade Tile - Single</returns>
    private Arcade_Tile GetRandom(Arcade_Tile[] values)
    {
        // Error check
        if(values.Length <= 0)
        {
            Debug.LogError("Input values are not set properly");
            return null;
        }
        // Return Random Value
        return values[Random.Range(0, values.Length)];
    }
    #endregion
    #endregion
}
