using UnityEngine;

[System.Serializable]
public class ArcadeProgression
{
    #region Entry Struct
    [System.Serializable]
    /// <summary>
    ///     Defines an entry within Arcade Progression
    /// </summary>
    public struct ArcadeProgressionEntry
    {
        public string identification;
        [Space]
        public Arcade_Tileset tileset;
        public int floor;
    }
    #endregion

    [Tooltip("Entries must be organized by floor number")]
    [SerializeField] private ArcadeProgressionEntry[] arcadeProgressionEntries;

    /// <summary>
    ///     Pulls the arcade tileset based on the floor
    /// </summary>
    /// <param name="floor">Input floor</param>
    /// <returns>Arcade tileset</returns>
    public Arcade_Tileset GetTileset(int floor)
    {
        // Get the floor tileset
        return GetEntry(floor).tileset;
    }

    /// <summary>
    ///     Pulls the arcade progression entry based on the floor
    /// </summary>
    /// <param name="floor">Input floor</param>
    /// <returns>Arcade tileset</returns>
    public ArcadeProgressionEntry GetEntry(int floor)
    {
        // Hold the last valid entry... defaults to first
        ArcadeProgressionEntry validEntry = arcadeProgressionEntries[0];
        // Roll through entries
        for(int i = 1; i < arcadeProgressionEntries.Length; i++)
        {
            // Check if the floor is greater than stored in the entry
            if (arcadeProgressionEntries[i].floor <= floor)
                validEntry = arcadeProgressionEntries[i];
            else
                break;
        }
        return validEntry;
    }
}