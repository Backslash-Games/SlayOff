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

    [SerializeField] private ComboObjective[] comboObjectives = new ComboObjective[0];
    [SerializeField] private Dictionary<string, ushort> comboObjectiveIDs = new Dictionary<string, ushort>();
    [SerializeField] private uint currentCombo = 0;
    [Space]
    [SerializeField] private bool writeStoredCollectibleString = true;
    [SerializeField] private bool writeComboString = true;

    #region Combo Struct
    [System.Serializable]
    private struct ComboObjective
    {
        [SerializeField] private string name; // Name of the objective
        [SerializeField] private int quota; // Quota towards the objective
        [Space]
        [SerializeField] private int progress; // The current progress towards the objective

        public ComboObjective(string name, int quota)
        {
            this.name = name;
            this.quota = quota;

            progress = 0;
        }

        /// <summary>
        ///     Increases progress by 1
        /// </summary>
        public void IncreaseProgress() { IncreaseProgress(1); }
        /// <summary>
        ///     Increases progress by amount
        /// </summary>
        /// <param name="amount">Amount</param>
        public void IncreaseProgress(int amount) { progress += amount; }
        
        /// <summary>
        ///     Checks if the objective is complete, if so then compete objective
        /// </summary>
        /// <returns>True if complete</returns>
        public bool isComplete() 
        {
            bool completion = progress >= quota;
            if (completion)
                CompleteObjective();
            return completion; 
        }

        /// <summary>
        ///     Completes the objective
        /// </summary>
        private void CompleteObjective() { progress -= quota; }

        /// <summary>
        ///     Gets the name of the objective
        /// </summary>
        /// <returns>Objective Name</returns>
        public string GetName() { return name; }

        // To String
        public override string ToString()
        {
            string output = "";

            output += $"{name} : {progress}/{quota}\n";

            return output;
        }
    }
    #endregion

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
    #region Combo Management
    public void RewardComboProgress()
    {
        currentCombo++;
    }

    #region Objective Management
    /// <summary>
    ///     Increases combo objective (by id) by 1
    /// </summary>
    /// <param name="id">String ID</param>
    public void AddObjectiveProgress(string id)
    {
        // Gets the objective and increases progress
        CreateObjectiveDictionary();
        comboObjectives[comboObjectiveIDs[id]].IncreaseProgress();

        // Check if the objective was complete
        if (comboObjectives[comboObjectiveIDs[id]].isComplete())
            RewardComboProgress();
    }

    /// <summary>
    ///     Gets the objective from a predefined list... Doesnt work for some reason?
    /// </summary>
    /// <param name="id">String ID</param>
    /// <returns>Combo Objective</returns>
    private ComboObjective GetObjective(string id)
    {
        // Try and create objective dictionary
        CreateObjectiveDictionary();
        // Check if the objective exists
        if (!comboObjectiveIDs.ContainsKey(id))
        {
            Debug.LogError($"Objective with name \"{id}\" does not exist");
            return new ComboObjective("NULL", 0);
        }

        return comboObjectives[comboObjectiveIDs[id]];
    }
    /// <summary>
    ///     Creates the objective dictionary if it has not yet been created
    /// </summary>
    private void CreateObjectiveDictionary()
    {
        // Check if dictionary is the right size
        if (comboObjectiveIDs.Count == comboObjectives.Length)
            return;

        // Reset the dictionary
        comboObjectiveIDs.Clear();
        // Add all components to the dictionary
        for (int i = 0; i < comboObjectives.Length; i++)
            // Add dictionary entry
            comboObjectiveIDs.Add(comboObjectives[i].GetName(), (ushort)i);
    }
    #endregion
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
        output += ComboToString();
        output += ComboObjectivesToString();

        return output;
    }

    private string StoredCollectiblesToString()
    {
        if (!writeStoredCollectibleString)
            return "Write Stored Collectibles set to False\n\n";

        string output = "";

        Dictionary<uint, ushort>.KeyCollection keys = storedCollectibles.Keys;

        output += "Stored Collectibles\n";
        foreach (uint key in keys)
            output += $". > {(new Collectible(key)).GetName()} [{storedCollectibles[key]}]({key})\n";
        output += "\n";

        return output;
    }

    private string ComboToString()
    {
        if (!writeComboString)
            return "Write Combo String set to False\n\n";

        string output = "";

        output += $"Current Combo: {currentCombo}\n";
        output += "\n";

        return output;
    }

    private string ComboObjectivesToString()
    {
        if (!writeComboString)
            return "Write Combo String set to False\n\n";

        string output = "";

        for (int i = 0; i < comboObjectives.Length; i++)
            output += $"{comboObjectives[i]}\n";

        return output;
    }
    #endregion
}