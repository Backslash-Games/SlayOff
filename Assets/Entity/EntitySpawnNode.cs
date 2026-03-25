using UnityEngine;

public class EntitySpawnNode : MonoBehaviour
{
    [SerializeField] private EntityData node_entity = null;
    [SerializeField] private EntityData spawn_entity = null;
    [Space]
    [SerializeField] private float spawnForce = 5;
    [SerializeField] private float spawnTorque = 5;
    [Space]
    [SerializeField] private bool debug_spawnEnemy = false;

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
        if (spawn_entity == null)
        {
            Debug.Log("Node entity is null");
            return null;
        }

        // Spawn the entity
        node_entity.Kill("Spawn Node");
        node_entity.ResetConstraints();
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
