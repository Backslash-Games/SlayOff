using UnityEngine;

public class EntitySpawnNode : MonoBehaviour, IActivatable
{
    [SerializeField] private EntityData node_entity = null;
    [SerializeField] private EntityData spawn_entity = null;
    [Space]
    [SerializeField] private GameObject[] possible_spawns = new GameObject[3];
    [SerializeField] private Transform spawn_point;
    [Space]
    [SerializeField] private float spawnForce = 5;
    [SerializeField] private float spawnTorque = 5;
    [Space]
    [SerializeField] private bool debug_spawnEnemy = false;

    private static byte s_LastSpawnID = 0;

    /// <summary>
    ///     Spawns enemy on node
    /// </summary>
    public EntityData Spawn()
    {
        // Make sure entities are set properly
        if(node_entity == null)
        {
            Debug.Log("Node entity is null");
            return null;
        }

        // Break barrier
        node_entity.Kill("Spawn Node");
        node_entity.ResetConstraints();
        // Spawn the entity
        GameObject spawned = Instantiate(GetSpawn(), spawn_point.transform.position, Quaternion.identity, transform);
        spawn_entity = spawned.GetComponent<EntityData>();

        // Make sure entities are set properly
        if (spawn_entity == null)
        {
            Debug.Log("Spawned entity is null");
            return null;
        }

        // Apply random force
        Vector2 rDirection = Random.insideUnitCircle;
        node_entity.ApplyForce(Vector3.up + new Vector3(rDirection.x, 0, rDirection.y), spawnForce, ForceMode.Impulse, "Spawn Node");
        node_entity.ApplyTorque(Random.insideUnitSphere, spawnTorque, ForceMode.Impulse, "Spawn Node");

        spawn_entity.enabled = true;
        spawn_entity.ResetConstraints();

        // Retuns spawn entity
        return spawn_entity;
    }

    #region Unity Methods
    private void Start()
    {
        // Set the initial state of objects
        if (node_entity != null && spawn_entity != null)
        {
            node_entity.SetConstraints(RigidbodyConstraints.FreezeAll);

            spawn_entity.SetConstraints(RigidbodyConstraints.FreezeAll);
            spawn_entity.enabled = false;
        }
    }
    private void Update()
    {
        Debug_Spawn();
    }
    #endregion

    #region Get Methods
    public EntityData GetSpawnedEntity()
    {
        return spawn_entity;
    }

    public GameObject GetSpawn()
    {
        // Pull a random number
        int rng = Random.Range(0, possible_spawns.Length);
        // Check if it is a unique spawn
        if (rng == s_LastSpawnID) 
            rng = (s_LastSpawnID + 1) % possible_spawns.Length;
        // Set last spawn
        s_LastSpawnID = (byte)rng;

        return possible_spawns[rng];
    }
    #endregion
    #region Exposed Methods
    /// <summary>
    ///     Exposed for Unity Events
    /// </summary>
    public void Event_Spawn()
    {
        Spawn();
    }
    #endregion
    #region Interface
    /// <summary>
    ///     IActivatable implementation
    /// </summary>
    public void Activate()
    {
        Spawn();
    }
    #endregion

    #region Debug
    private void Debug_Spawn()
    {
        if (debug_spawnEnemy)
        {
            Spawn();
            debug_spawnEnemy = false;
        }
    }
    #endregion
}
