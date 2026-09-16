using HFHandyUtils.Data.Spawning;
using UnityEngine;

public class SpawnableObject_AttributeLogic : MonoBehaviour
{
    public virtual void OnInitialization(SpawnableObject spawnable) { }
    public virtual void OnUpdate(SpawnableObject spawnable) { }
    public virtual void OnFixedUpdate(SpawnableObject spawnable) { }
    public virtual void OnLateUpdate(SpawnableObject spawnable) { }
    public virtual void OnImpact(SpawnableObject spawnable) { }
    public virtual void OnImpactEntity(SpawnableObject spawnable, EntityData entity) { }
    public virtual void OnBreak(SpawnableObject spawnable) { }

    #region Spawnable Effect
    /// <summary>
    ///     Spawns a spawnable effect
    /// </summary>
    /// <param name="hit">Hit point</param>
    /// <returns>Spawnable Effect</returns>
    protected SpawnableEffect Spawn_SpawnableEffect(string id, SpawnableObject spawnable, RaycastHit hit)
    {
        return Spawn_SpawnableEffect(id, spawnable, hit.point, hit.normal);
    }
    /// <summary>
    ///     Spawns a spawnable effect
    /// </summary>
    /// <param name="hit">Hit point</param>
    /// <returns>Spawnable Effect</returns>
    protected SpawnableEffect Spawn_SpawnableEffect(string id, SpawnableObject spawnable, Vector3 position, Vector3 effectUp)
    {
        // Spawn a spawnable effect
        GameObject spawned = ObjectPooler.Instance.Spawn(id, position, Vector3.zero, true);
        spawned.transform.up = effectUp;

        // Pull effect
        SpawnableEffect spawnableEffect = spawned.GetComponent<SpawnableEffect>();
        spawnableEffect.Force_OnEnable();
        // Set team
        spawnableEffect.ChangeTeam(spawnable.team);

        return spawnableEffect;
    }
    #endregion
}
