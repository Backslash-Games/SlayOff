using UnityEngine;

public class EntitySpawnNode : MonoBehaviour
{
    [SerializeField] private EntityData node_entity = null;
    [SerializeField] private EntityData spawn_entity = null;
    [Space]
    [SerializeField] private bool debug_spawnEnemy = false;

    /// <summary>
    ///     Spawns enemy on node
    /// </summary>
    private void Spawn()
    {
        // Make sure entities are set properly
        if(node_entity == null)
        {
            Debug.Log("Node entity is null");
            return;
        }
        if (spawn_entity == null)
        {
            Debug.Log("Node entity is null");
            return;
        }

        // Spawn the entity
        node_entity.Kill("Spawn Node");
        spawn_entity.enabled = true;
    }

    #region Unity Methods
    private void Awake()
    {
        if (spawn_entity != null)
            spawn_entity.enabled = false;
    }
    private void Update()
    {
        Debug_Spawn();
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
