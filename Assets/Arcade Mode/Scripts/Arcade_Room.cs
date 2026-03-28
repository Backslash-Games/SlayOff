using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class Arcade_Room : MonoBehaviour
{
    [Header("Attributes")]
    [SerializeField] private Bounds bounds;
    private static readonly float player_padding = 0.8f;

    [Header("Assets")]
    [SerializeField] private bool skipAssetClear = false;
    [Space]
    [SerializeField] private GameObject mesh_parent = null;
    [SerializeField] private GameObject prop_parent = null;
    [Space]
    [SerializeField] private CardboardBox[] contained_boxes = new CardboardBox[0];

    [Header("Combat")]
    [SerializeField] private bool hasCombat = false;
    private enum CombatState { Inactive, Initial, Sealing, Spawning, Active, Cleared, Failure };
    [SerializeField] private CombatState combatState = CombatState.Inactive;
    [Space]
    [SerializeField] private EntitySpawnNode[] enemy_spawnNodes = new EntitySpawnNode[0];
    private List<EntityData> activeEnemies = new List<EntityData>();

    [Header("Components")]
    [SerializeField] private Arcade_Door[] doors;

    private int lastDoorInteracted = 0;

    public delegate void RoomState();
    public event RoomState OnRoomCleared;

    #region Unity Methods
    private void Awake()
    {
        ClearAllAssets();
        OnRoomCleared += ExplodeAllBoxes;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.forestGreen;
        Gizmos.DrawWireCube(GetBounds_World().center, GetBounds_World().size);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(GetPlayerBounds_World().center, GetPlayerBounds_World().size);
    }
    private void OnDestroy()
    {
        OnRoomCleared -= ExplodeAllBoxes;
    }
    private void LateUpdate()
    {
        Tick_Combat();
    }
    #endregion

    #region Asset Management
    /// <summary>
    ///     Sets all assets as inactive
    /// </summary>
    private void ClearAllAssets()
    {
        if (skipAssetClear)
            return;

        mesh_parent.SetActive(false);
        prop_parent.SetActive(false);
    }

    /// <summary>
    ///     Initializes mesh assets
    /// </summary>
    public void InitializeMeshAssets()
    {
        mesh_parent.SetActive(true);
    }
    /// <summary>
    ///     Initializes prop assets
    /// </summary>
    public void InitializePropAssets()
    {
        prop_parent.SetActive(true);
    }
    #endregion
    #region Door Management
    private void OpenAllConnectedDoors()
    {
        // Roll through and open all doors that are unavaliable and connectors
        for(int i = 0; i <doors.Length; i++)
        {
            Arcade_Door cDoor = GetDoor(i);
            if (!cDoor.isAvaliable() && cDoor.isConnector())
                cDoor.SetOpen();
        }
    }
    private void CloseAllDoors()
    {
        // Roll through and open all doors that are unavaliable and connectors
        for (int i = 0; i < doors.Length; i++)
        {
            Arcade_Door cDoor = GetDoor(i);
            cDoor.SetClosed();
        }
    }

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
    public void Error_LastDoor()
    {
        GetDoor(lastDoorInteracted).SetError();
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
    public Bounds GetPlayerBounds_World()
    {
        Bounds pBounds = GetBounds_World();
        pBounds.size = pBounds.size - new Vector3(player_padding, 0, player_padding);

        return pBounds;
    }

    /// <summary>
    ///     Checks for an overlap
    /// </summary>
    /// <param name="other">Input bounds</param>
    /// <returns>True if there is an overlap</returns>
    public bool CheckOverlap(Bounds other) { return GetBounds_World().Intersects(other); }
    #endregion

    #region Box Management
    private void ExplodeAllBoxes() { StartCoroutine(enum_ExplodeAllBoxes()); }
    private IEnumerator enum_ExplodeAllBoxes()
    {
        float stallThreshold = contained_boxes.Length * 0.05f;
        // Roll through each value in boxes
        for(int i = 0; i < contained_boxes.Length; i++)
        {
            if (contained_boxes[i] != null && contained_boxes[i].isActiveAndEnabled)
                contained_boxes[i].ApplyForce(Random.insideUnitSphere, 5, ForceMode.Impulse, "Room Cleared");
            if(i > stallThreshold)
            {
                yield return new WaitForSeconds(0.05f);
                stallThreshold += contained_boxes.Length * 0.05f;
            }
        }
    }
    #endregion

    #region Combat Management
    /// <summary>
    ///     Sequencing for combat
    /// </summary>
    public void RequestCombat()
    {
        // Check if combat is allowed
        Log("Requesting Combat");
        if (combatState.Equals(CombatState.Cleared))
        {
            Log("Room is cleared");
            return;
        }

        // Move into initial stage
        RequestCombatState(CombatState.Initial, true);
        if (!hasCombat)
        {
            Log("Room does not have combat");
            RequestCombatState(CombatState.Failure);
            return;
        }

        // Seal all the doors
        Log("Sealing Doors");
        RequestCombatState(CombatState.Sealing);
        CloseAllDoors();

        // Spawn enemies
        Log("Spawning enemies");
        Combat_SpawnEnemies();

        // Set combat to active
        RequestCombatState(CombatState.Active);



        // If the combat state has resulted in a failure, open the doors
        if (combatState.Equals(CombatState.Failure))
            OpenAllConnectedDoors();
    }
    /// <summary>
    ///     Updates combat when active
    /// </summary>
    private void Tick_Combat()
    {
        // Check if we are active
        if (!combatState.Equals(CombatState.Active))
            return;

        // Track active enemies
        for(int i = 0; i < activeEnemies.Count; i++)
        {
            // Check if the entity is null
            if (activeEnemies[i] == null || activeEnemies[i].isDead())
            {
                activeEnemies.RemoveAt(i);
                i--;
            }
        }

        // Finish combat if active enemies is 0
        if (activeEnemies.Count == 0)
            EndCombat();
    }
    /// <summary>
    ///     Ends the active combat
    /// </summary>
    public void EndCombat()
    {
        // Check if we are active
        if (!combatState.Equals(CombatState.Active))
        {
            Log("Could not end inactive combat");
            return;
        }
        
        // Clear combat and open doors
        RequestCombatState(CombatState.Cleared);
        OpenAllConnectedDoors();

        OnRoomCleared?.Invoke();
    }

    /// <summary>
    ///     Spawns combat enemies
    /// </summary>
    public void Combat_SpawnEnemies()
    {
        // Set the combat state
        RequestCombatState(CombatState.Spawning);
        // Error check
        if(enemy_spawnNodes.Length <= 0)
        {
            RequestCombatState(CombatState.Failure);
            return;
        }

        // Roll through spawn nodes and spawn enemies
        foreach (EntitySpawnNode node in enemy_spawnNodes)
            activeEnemies.Add(node.Spawn());
    }
    #region State Handling
    /// <summary>
    ///     Requests a combat state change
    /// </summary>
    /// <param name="state">New state</param>
    /// <param name="force">Forces the current state</param>
    private void RequestCombatState(CombatState state, bool force = false)
    {
        // If the current state has failed ignore change
        if (combatState.Equals(CombatState.Failure) && !force)
            return;
        // Check for an error
        if (state.Equals(CombatState.Failure))
            Log($"Failed combat setup on {combatState} step. Moving to failure state");

        // Set the combat state
        combatState = state;
    }
    #endregion
    #endregion

    #region Debug
    /// <summary>
    ///     Local quick call for logging information
    /// </summary>
    /// <param name="information">Logged information</param>
    private void Log(string information)
    {
        Debug.Log($"ArcadeRoom::{name} -> {information}");
    }
    #endregion
}
