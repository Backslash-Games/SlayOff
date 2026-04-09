using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class FeedHandler : MonoBehaviour
{
    private enum FeedStates { CollectableGot };

    [SerializeField] private Transform parent;
    [SerializeField] private GameObject entryPrefab;
    [SerializeField] private int feedSpawnAmount;
    private int feedTarget = 0;
    [Space]
    [SerializeField] private Vector3 spawnPosition = Vector3.zero; // Spawned position
    [SerializeField] private Vector3 initialPosition = Vector3.zero; // Position moved into after spawning
    [Space]
    [SerializeField] private float spawnRotation = 0; // Spawned rotation
    [SerializeField] private float initialRotation = 0; // Rotation moved into after spawning
    [Space]
    [SerializeField] private float bumpAmount = -5f;
    [Space]
    [SerializeField] private EffectLibrary<FeedStates, AudioClip, EffectComponent_Audio.AudioParameters> audioLibrary;

    private FeedEntry[] collectibleFeedEntries;
    private bool[] awakeEntries;

    private Queue<string> queuedFeed = new Queue<string>();
    private static readonly float queueDelay = 0.2f;
    private static readonly float queueCountDelayScale = 0.4f;
    private bool queueActive = false;

    private bool hidden = false;

    [Space]
    public UnityEvent OnFeedChanged = new UnityEvent();
    public UnityEvent OnRunFeedElements = new UnityEvent();

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
        collectibleFeedEntries = new FeedEntry[feedSpawnAmount];

        // Spawn each entry
        for(int i = 0; i < feedSpawnAmount; i++)
        {
            // Spawn and log entry
            GameObject spawnedObject = Instantiate(entryPrefab, parent);
            FeedEntry entry = spawnedObject.GetComponent<FeedEntry>();
            collectibleFeedEntries[i] = entry;

            // Set up entry
            collectibleFeedEntries[i].Initialize(spawnPosition, spawnRotation);
        }
        // Set initial variables
        feedTarget = 0;
        awakeEntries = new bool[feedSpawnAmount];
    }

    public void RequestNewFeed(string value)
    {
        // Enqueue Binary
        queuedFeed.Enqueue(value);
        // Set unhidden
        hidden = false;

        // Check if we need to run the feed
        if (!queueActive)
            StartCoroutine(RunFeed());
    }

    private IEnumerator RunFeed()
    {
        // Mark queue as active
        queueActive = true;

        // Display information
        while(queuedFeed.Count > 0)
        {
            CreateNewFeed(queuedFeed.Dequeue().ToString());
            yield return new WaitForSeconds(queueDelay / Mathf.Clamp(queuedFeed.Count * queueCountDelayScale, 1, int.MaxValue));
        }
        // Unlock queue
        queueActive = false;
    }
    private void CreateNewFeed(string data, bool play_audio = true)
    {
        // Pull the index for the current active feed member
        // -> Wake up
        awakeEntries[feedTarget] = true;
        // -> Bump all
        BumpActiveFeed();

        // -> Reset position and move to inital
        collectibleFeedEntries[feedTarget].ForcePosition(spawnPosition);
        collectibleFeedEntries[feedTarget].SetPositionOverTime(initialPosition);
        // -> Reset rotation and move to inital
        collectibleFeedEntries[feedTarget].ForceRotation(spawnRotation);
        collectibleFeedEntries[feedTarget].SetRotationOverTime(initialRotation);

        // Render the collectible
        collectibleFeedEntries[feedTarget].TickRenderer(data);

        // Increase target
        feedTarget = (feedTarget + 1) % feedSpawnAmount;

        // Play audio
        EffectManager.Instance.Play(audioLibrary, FeedStates.CollectableGot);


        // Call on feed changed
        OnFeedChanged.Invoke();
    }

    public void CollapseFeed()
    {
        if (!queueActive)
            return;

        StopAllCoroutines();


        // Display information
        while (queuedFeed.Count > 0)
        {
            CreateNewFeed(queuedFeed.Dequeue().ToString());
        }
    }

    private void BumpActiveFeed()
    {
        // Bump all active feeds
        for(int i = 0; i < collectibleFeedEntries.Length; i++)
            if (awakeEntries[i])
                collectibleFeedEntries[i].Bump(bumpAmount);
    }
    /// <summary>
    ///     Hides all active entires
    /// </summary>
    public void HideAllFeedEntries() 
    {
        if (hidden)
            return;
        for (int i = 0; i < collectibleFeedEntries.Length; i++)
        {
            FeedEntry entry = collectibleFeedEntries[i];

            entry.ForcePosition(spawnPosition);
            entry.ForceRotation(spawnRotation);

            awakeEntries[i] = false;
            feedTarget = 0;
        }
        hidden = true;
    }
    #endregion
}
