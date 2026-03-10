using System.Collections.Generic;
using UnityEngine;

public class InventoryHandler : MonoBehaviour
{
    // Singleton
    private static InventoryHandler _instance;
    public static InventoryHandler Instance { get { return _instance; } }


    /// <summary>
    ///     Defines stored collectibles. Key is the collectible binart, Value is the quantity
    /// </summary>
    [SerializeField] private Dictionary<uint, ushort> storedCollectibles = new Dictionary<uint, ushort>();

    #region Unity Methods
    private void Awake()
    {
        CreateSingleton();
    }
    #endregion
    #region Singleton
    private void CreateSingleton()
    {
        // -> Pulled from Out on the Red Sea
        // Checks if the instance of CollectibleGenerator is first of its type
        // If CollectibleGenerator is not unique, destroy current instance
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        // Declares this script as current
        else
        {
            _instance = this;
        }
    }
    #endregion

    #region Collectible Management
    /// <summary>
    ///     Pulls a random collectible binary from the generator
    /// </summary>
    public void RewardRandomCollectible()
    {
        RewardCollectible(CollectibleGenerator.Instance.GetRandomCollectibleBinary());
    }
    /// <summary>
    ///     Pulls a set of random collectible binarys from the generator
    /// </summary>
    public void RewardRandomCollectibles(int amount)
    {
        for(int i = 0; i < amount; i++)
            RewardCollectible(CollectibleGenerator.Instance.GetRandomCollectibleBinary());
    }

    /// <summary>
    ///     Rewards a collectable from binary
    /// </summary>
    /// <param name="binary"></param>
    public void RewardCollectible(uint binary)
    {
        AddBinaryCollectible(binary); // Adds the binary to stored collectibles
        GetCollectibleFeedHandler().RequestNewFeed(binary); // Requests pop up feed for binary
    }

    private void AddBinaryCollectible(uint binary)
    {
        // Add key if it has not been added yet
        if (!storedCollectibles.ContainsKey(binary))
            storedCollectibles.Add(binary, 1);
        // Otherwise try increase quantity
        else
            storedCollectibles[binary]++;
    }
    #endregion

    #region Get Method
    private CollectibleFeedHandler feedHandler = null;
    private CollectibleFeedHandler GetCollectibleFeedHandler()
    {
        if (feedHandler == null)
            feedHandler = FindAnyObjectByType<CollectibleFeedHandler>();
        return feedHandler;
    }
    #endregion
    #region Debug
    public override string ToString()
    {
        string output = "";

        output += StoredCollectiblesToString();

        return output;
    }

    private string StoredCollectiblesToString()
    {
        string output = "";

        Dictionary<uint, ushort>.KeyCollection keys = storedCollectibles.Keys;

        output += "Stored Collectibles\n";
        foreach (uint key in keys)
            output += $". > {(new Collectible(key)).GetName()} [{storedCollectibles[key]}]({key})\n";

        return output;
    }
    #endregion
}
