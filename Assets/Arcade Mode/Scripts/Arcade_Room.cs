using UnityEngine;

public class Arcade_Room : MonoBehaviour
{
    [Header("Attributes")]
    [SerializeField] private Bounds bounds;

    [Header("Components")]
    [SerializeField] private Arcade_Door[] doors;

    #region Unity Methods
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.forestGreen;
        Gizmos.DrawWireCube(bounds.center + transform.position, transform.rotation * bounds.size);
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
        return doors[index % doors.Length];
    }

    /// <summary>
    ///     Gets the total count of all doors
    /// </summary>
    /// <returns>Door Count</returns>
    public int GetDoorCount() { return doors.Length; }
    #endregion
}
