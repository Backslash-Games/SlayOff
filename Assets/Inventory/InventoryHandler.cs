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

    [Header("Score")]
    [SerializeField] private int score_debug = 100000;
    [SerializeField] private LimitlessNumeric score;
    
    [Header("Combo")]
    [SerializeField] private ComboObjective[] comboObjectives = new ComboObjective[0];
    [SerializeField] private Dictionary<string, ushort> comboObjectiveIDs = new Dictionary<string, ushort>();
    [Space]
    [SerializeField] private uint currentCombo = 0;
    [SerializeField] private float currentComboTime = 0;
    [SerializeField] private Vector2 comboTimeRange = Vector2.up;
    [SerializeField] private uint comboTimeMinimumThreshold = 100;
    [Space]
    [SerializeField] private bool writeStoredCollectibleString = true;
    [SerializeField] private bool writeComboString = true;

    #region Combo Struct
    [System.Serializable]
    private struct ComboObjective
    {
        [SerializeField] private string name; // Name of the objective
        [SerializeField] private string feedDescription; // Description that is displayed on feed
        [SerializeField] private int quota; // Quota towards the objective
        [SerializeField] private float value; // The percentage value of the combo
        [Space]
        [SerializeField] private int progress; // The current progress towards the objective
        [SerializeField] private int timesCompleted; // The total times the player has completed an objective

        public ComboObjective(string name, string feedDescription, int quota, float value)
        {
            this.name = name;
            this.feedDescription = feedDescription;
            this.quota = quota;
            this.value = value;

            progress = 0;
            timesCompleted = 0;
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
        ///     Resets the objective progress
        /// </summary>
        public void ResetProgress() { progress = 0; }

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
        private void CompleteObjective() 
        { 
            progress -= quota; 
            timesCompleted++; 
        }

        /// <summary>
        ///     Gets the name of the objective
        /// </summary>
        /// <returns>Objective Name</returns>
        public string GetName() { return name; }
        /// <summary>
        ///     Gets the feed description of the objective
        /// </summary>
        /// <returns>Feed Description</returns>
        public string GetFeedDescription() { return feedDescription; }
        /// <summary>
        ///     Gets the value of the objective
        /// </summary>
        /// <returns>Objective Value</returns>
        public float GetValue() { return value; }

        // To String
        public override string ToString()
        {
            string output = "";

            output += $"{name} : {progress}/{quota} : <{timesCompleted}>\n";

            return output;
        }
    }
    #endregion

    #region Unity Methods
    private void Awake()
    {
        CreateSingleton();
        score = new LimitlessNumeric(score_debug);
    }

    private void Update()
    {
        TickComboTime();
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
        AddObjectiveProgress("Collectable Get");
        AddBinaryCollectible(binary); // Adds the binary to stored collectibles
        GetCollectibleFeedHandler().RequestNewFeed(binary.ToString()); // Requests pop up feed for binary
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
    /// <summary>
    ///     Adds progress to the current combo
    /// </summary>
    /// <param name="source">Completed combo</param>
    public void RewardComboProgress(ushort source)
    {
        // Reward combo progress
        currentCombo++;
        //GetComboFeedHandler().RequestNewFeed(comboObjectives[source].GetFeedDescription()); // Requests pop up feed for binary

        // Reset combo timer if not running
        if (currentComboTime <= 0)
            ResetComboTime();
        else
            AddComboTime(comboObjectives[source].GetValue());
    }
    /// <summary>
    ///     Resets the current combo
    /// </summary>
    private void ResetCombo() 
    {
        // Reset current combo
        currentCombo = 0;
        // Reset all objectives
        ResetAllObjectiveProgress();

        // Reset feed display
        //GetComboFeedHandler().OnFeedChanged.Invoke();
    }

    /// <summary>
    ///     Updates the combo timer
    /// </summary>
    private void TickComboTime() 
    {
        // Check if our combo timer is set
        if (currentComboTime <= 0)
        {
            //GetComboFeedHandler().HideAllFeedEntries();
            return;
        }
        // Decrease combo time
        currentComboTime -= Time.deltaTime;
        // Check if the combo is done
        if (currentComboTime <= 0)
            ResetCombo();

        // Run feed elements
        //GetComboFeedHandler().OnRunFeedElements.Invoke();
    }
    /// <summary>
    ///     Sets the current combo timer to max
    /// </summary>
    private void ResetComboTime() { currentComboTime = Mathf.Lerp(comboTimeRange.y, comboTimeRange.x, (float)currentCombo / comboTimeMinimumThreshold); }
    /// <summary>
    ///     Adds time to the combo timer
    /// </summary>
    /// <param name="value">The total value of the completed combo</param>
    private void AddComboTime(float value) 
    { 
        // Get the current max time
        float currentMaxTime = GetCurrentComboMaxTime();
        // Add time
        currentComboTime += currentMaxTime * value;
        // Clamp time
        currentComboTime = Mathf.Clamp(currentComboTime, 0, currentMaxTime);
    }

    /// <summary>
    ///     Gets the current combo
    /// </summary>
    /// <returns>uInteger current combo</returns>
    public uint GetCurrentCombo() { return currentCombo; }

    /// <summary>
    ///     Gets the current combo max time
    /// </summary>
    /// <returns></returns>
    public float GetCurrentComboMaxTime() { return Mathf.Lerp(comboTimeRange.y, comboTimeRange.x, (float)currentCombo / comboTimeMinimumThreshold); }
    /// <summary>
    ///     
    /// </summary>
    /// <returns></returns>
    public float GetCurrentComboTimerPercentage() { return currentComboTime / GetCurrentComboMaxTime(); }

    #region Objective Management
    /// <summary>
    ///     Increases combo objective (by id) by 1
    /// </summary>
    /// <param name="id">String ID</param>
    public void AddObjectiveProgress(string id)
    {
        CreateObjectiveDictionary();
        // Check if key exists
        if (!comboObjectiveIDs.ContainsKey(id))
            return;

        // Gets the objective and increases progress
        comboObjectives[comboObjectiveIDs[id]].IncreaseProgress();

        // Check if the objective was complete
        if (comboObjectives[comboObjectiveIDs[id]].isComplete())
            RewardComboProgress(comboObjectiveIDs[id]);
    }
    /// <summary>
    ///     Resets all objectives progress
    /// </summary>
    private void ResetAllObjectiveProgress()
    {
        // Roll through each objective and reset it
        for (int i = 0; i < comboObjectives.Length; i++)
            comboObjectives[i].ResetProgress();
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
            return new ComboObjective("NULL", "NONE", 0, 0);
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

    #region Get Feed Handlers
    private FeedHandler collectibleFeedHandler = null;
    private FeedHandler GetCollectibleFeedHandler()
    {
        if (collectibleFeedHandler == null)
            collectibleFeedHandler = GameObject.Find("Collectible Feed").GetComponent<FeedHandler>();
        return collectibleFeedHandler;
    }
    private FeedHandler comboFeedHandler = null;
    private FeedHandler GetComboFeedHandler()
    {
        if (comboFeedHandler == null)
            comboFeedHandler = GameObject.Find("Combo Feed").GetComponent<FeedHandler>();
        return comboFeedHandler;
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
        output += $"Current Time: {currentComboTime}\n";
        output += "\n";

        return output;
    }

    private string ComboObjectivesToString()
    {
        if (!writeComboString)
            return "Write Combo String set to False\n\n";

        string output = "";

        for (int i = 0; i < comboObjectives.Length; i++)
            output += $"{comboObjectives[i]}";
        output += "\n";

        return output;
    }
    #endregion
}