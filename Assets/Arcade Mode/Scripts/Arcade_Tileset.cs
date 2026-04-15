using System.Collections.Generic;
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
            return GetRandomTile_Room();
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
            return GetRandomTile_Room();
        return goalRoom;
    }
    #endregion
    #region Random
    #region Tile
    /// <summary>
    ///     Gets a random room tile
    /// </summary>
    /// <returns>Arcade Tile - Room</returns>
    public Arcade_Tile GetRandomTile_Room()
    {
        // Get a random arcade tile
        Arcade_Tile rTile = GetRandomTile(rooms);

        // Error check
        if (rTile == null)
            Debug.Log("Room values are not set properly");
        return rTile;
    }
    /// <summary>
    ///     Gets a random hall tile
    /// </summary>
    /// <returns>Arcade Tile - Hall</returns>
    public Arcade_Tile GetRandomTile_Hall()
    {
        // Get a random arcade tile
        Arcade_Tile rTile = GetRandomTile(halls);

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
    private Arcade_Tile GetRandomTile(Arcade_Tile[] values)
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
    #region Collection
    /// <summary>
    ///     Returns a list of rooms in a random order
    /// </summary>
    /// <returns>List of arcade tiles</returns>
    public List<Arcade_Tile> GetRandomCollection_Room()
    {
        // Get a random arcade tile
        List<Arcade_Tile> tiles = GetRandomCollection(rooms);

        // Error check
        if (tiles.Count <= 0)
            Debug.Log("Room values are not set properly");
        return tiles;
    }
    /// <summary>
    ///     Returns a list of halls in a random order
    /// </summary>
    /// <returns>List of arcade tiles</returns>
    public List<Arcade_Tile> GetRandomCollection_Hall()
    {
        // Get a random arcade tile
        List<Arcade_Tile> tiles = GetRandomCollection(halls);

        // Error check
        if (tiles.Count <= 0)
            Debug.Log("Hall values are not set properly");
        return tiles;
    }

    /// <summary>
    ///     Returns the list of provided values in a random order
    /// </summary>
    /// <param name="values">Arcade Tile Array</param>
    /// <returns>List of Arcade Tiles</returns>
    private List<Arcade_Tile> GetRandomCollection(Arcade_Tile[] values)
    {
        // Establish a lists
        // -> Input List
        List<Arcade_Tile> iTiles = new List<Arcade_Tile>(values);
        // -> Final List
        List<Arcade_Tile> fTiles = new List<Arcade_Tile>();
        
        // Randomly pull all tiles out of input and push into final
        while(iTiles.Count > 0)
        {
            // Select a random tile
            int index = Random.Range(0, iTiles.Count);
            fTiles.Add(iTiles[index]);
            // Remove tile from input
            iTiles.RemoveAt(index);
        }

        // Return final list
        return fTiles;
    }
    #endregion
    #endregion
    #endregion
}
