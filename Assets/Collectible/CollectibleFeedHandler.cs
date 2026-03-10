using UnityEngine;

public class CollectibleFeedHandler : MonoBehaviour
{
    [SerializeField] private Transform collectibleFeedEntriesParent;
    [SerializeField] private GameObject collectibleFeedEntryPrefab;
    [SerializeField] private int collectibleFeedSpawnAmount;
    private int collectibleFeedTarget = 0;
    [Space]
    [SerializeField] private float spawnRotation = 0;
    [SerializeField] private float initialRotation = 0;
    [SerializeField] private float rotationSeperation = -5f;

    private CollectibleFeedEntry[] collectibleFeedEntries;
    private bool[] awakeEntries;

    #region Unity Method
    private void Awake()
    {
        SpawnCollectibleFeedEntires();
    }
    #endregion
    #region Initialize
    /// <summary>
    ///     Spawns collectible feed entries
    /// </summary>
    private void SpawnCollectibleFeedEntires()
    {
        // Initialize field
        collectibleFeedEntries = new CollectibleFeedEntry[collectibleFeedSpawnAmount];

        // Spawn each entry
        for(int i = 0; i < collectibleFeedSpawnAmount; i++)
        {
            // Spawn and log entry
            GameObject spawnedObject = Instantiate(collectibleFeedEntryPrefab, collectibleFeedEntriesParent);
            CollectibleFeedEntry entry = spawnedObject.GetComponent<CollectibleFeedEntry>();
            collectibleFeedEntries[i] = entry;

            // Set up entry
            collectibleFeedEntries[i].Initialize(spawnRotation);
        }
        // Set initial variables
        collectibleFeedTarget = 0;
        awakeEntries = new bool[collectibleFeedSpawnAmount];
    }

    public void RequestNewFeed(uint binary)
    {
        CreateNewFeed(binary);
    }

    private void CreateNewFeed(uint binary)
    {
        // Pull the index for the current active feed member
        // -> Wake up
        awakeEntries[collectibleFeedTarget] = true;
        // -> Bump all
        BumpActiveFeed();
        // -> Reset position and move to inital
        collectibleFeedEntries[collectibleFeedTarget].ForceRotation(spawnRotation);
        collectibleFeedEntries[collectibleFeedTarget].SetRotationOverTime(initialRotation);

        // Render the collectible
        collectibleFeedEntries[collectibleFeedTarget].RenderCollectible(binary);

        // Increase target
        collectibleFeedTarget = (collectibleFeedTarget + 1) % collectibleFeedSpawnAmount;

    }
    private void BumpActiveFeed()
    {
        // Bump all active feeds
        for(int i = 0; i < collectibleFeedEntries.Length; i++)
            if (awakeEntries[i])
                collectibleFeedEntries[i].AddRotationOverTime(rotationSeperation);
    }
    #endregion
}
