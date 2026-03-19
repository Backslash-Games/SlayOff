using System.Collections.Generic;
using UnityEngine;

public class Arcade_Room : MonoBehaviour
{
    [Header("Attributes")]
    [SerializeField] private Bounds bounds;

    [Header("Components")]
    [SerializeField] private Arcade_Door[] doors;

    private int lastDoorInteracted = 0;

    #region Unity Methods
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.forestGreen;
        Gizmos.DrawWireCube(GetBounds_World().center, GetBounds_World().size);
    }
    #endregion

    #region Door Management
    /// <summary>
    ///     Runs through and finds an avaliable door
    /// </summary>
    /// <returns>Avaliable door</returns>
    public Arcade_Door GetAvaliableDoor()
    {
        // Start at a random door index
        int dIndex = Random.Range(0, doors.Length);

        // Roll through each and check for an avaliable
        for(int i = 0; i < doors.Length; i++)
        {
            Arcade_Door cDoor = GetDoor(i + dIndex);
            if (cDoor.isAvaliable())
                return cDoor;
        }
        return null;
    }

    /// <summary>
    ///     Gets a door at index, ensuring that it will always remain in bounds
    /// </summary>
    /// <param name="index">Index</param>
    /// <returns>Arcade Door</returns>
    public Arcade_Door GetDoor(int index)
    {
        if (doors.Length <= 0)
            return null;

        int mIndex = index % doors.Length;
        lastDoorInteracted = mIndex;
        return doors[mIndex];
    }

    /// <summary>
    ///     Gets the total count of all doors
    /// </summary>
    /// <returns>Door Count</returns>
    public int GetDoorCount() { return doors.Length; }
    /// <summary>
    ///     Closes the last door interacted with
    /// </summary>
    public void SealLastDoor()
    {
        GetDoor(lastDoorInteracted).SetSealed();
    }
    #endregion
    #region Bound Management
    /// <summary>
    ///     Gets the bounds of the room
    /// </summary>
    /// <returns>Bounds</returns>
    public Bounds GetBounds() { return bounds; }
    /// <summary>
    ///     Gets the bounds of the room
    /// </summary>
    /// <returns>Bounds</returns>
    public Bounds GetBounds_World()
    {
        Bounds wBounds = bounds;
        wBounds.center += transform.position;
        wBounds.size = transform.rotation * wBounds.size;
        wBounds.size = new Vector3(Mathf.Abs(wBounds.size.x), Mathf.Abs(wBounds.size.y), Mathf.Abs(wBounds.size.z));

        return wBounds;
    }

    /// <summary>
    ///     Checks for an overlap
    /// </summary>
    /// <param name="other">Input bounds</param>
    /// <returns>True if there is an overlap</returns>
    public bool CheckOverlap(Bounds other) { return GetBounds_World().Intersects(other); }
    #endregion
}
