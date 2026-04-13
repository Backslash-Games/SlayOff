using UnityEngine;
using UnityEngine.Events;

/// <summary>
///     Developer tool... Makes a simple spawn method visible by events
///     <br></br>
///     <br>Luke Wittbrodt :: lwittbrodt87@gmail.com :: halfhand870</br>
/// </summary>
public class Developer_SpawnPrefab : MonoBehaviour
{
    public GameObject prefab;
    public Transform destination;
    [Space]
    public UnityEvent<GameObject> UE_OnPrefabSpawned;

    /// <summary>
    ///     Spawns a prefab as a child at a destination
    /// </summary>
    public void SpawnPrefab()
    {
        // Check if objects are set properly
        if (prefab == null || destination == null)
            Debug.LogError("Please make sure that both the prefab and destination are set");

        // Spawn the object
        GameObject spawned = Instantiate(prefab, destination);
        spawned.transform.localPosition = Vector3.zero;

        // Call event
        UE_OnPrefabSpawned?.Invoke(spawned);
    }
}
