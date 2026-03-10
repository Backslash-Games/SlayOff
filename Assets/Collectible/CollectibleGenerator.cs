using UnityEngine;

public class CollectibleGenerator : MonoBehaviour
{
    // Singleton
    private static CollectibleGenerator _instance;
    public static CollectibleGenerator Instance { get { return _instance; } }

    // Variables
    [SerializeField] private uint collectibleIdentifier = 0;
    [SerializeField] private CollectibleDefinition[] gameCollectibles = new CollectibleDefinition[0];
    [Space]
    [SerializeField] private Collectible storedCollectible = null;

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

    #region Generation
    /// <summary>
    ///     Generates a new collectible from random list and attributes
    /// </summary>
    /// <returns>Collectible</returns>
    public Collectible GenerateNewCollectible()
    {
        storedCollectible = new Collectible(GetRandomCollectibleBinary());
        collectibleIdentifier = Collectible.CollectibleToBinary(storedCollectible);
        return storedCollectible;
    }
    /// <summary>
    ///     Gets a random collectible from gameCollectibles
    /// </summary>
    /// <returns>CollectibleDefinition</returns>
    private byte GetRandomCollectibleDefinition() { return (byte)(Random.Range(0, gameCollectibles.Length)); }
    /// <summary>
    ///     Creates a random binary for the collectible
    /// </summary>
    /// <returns>Collectible Binary</returns>
    public uint GetRandomCollectibleBinary()
    {
        return Collectible.CollectibleToBinary((byte)Random.Range(0, gameCollectibles.Length), (byte)Collectible.GetRandomQuality(), (byte)Collectible.GetRandomMaterial(), (byte)Collectible.GetRandomDefect(), (byte)Collectible.GetRandomAnomaly());
    }
    #endregion
    #region Get Methods
    /// <summary>
    ///     Gets a collectible definition from id
    /// </summary>
    /// <param name="id">Collectible ID</param>
    /// <returns>Definition</returns>
    public CollectibleDefinition GetDefinitionFromId(byte id) 
    {
        // Check if the id is in range
        if (id >= gameCollectibles.Length || id < 0)
            return null;
        return gameCollectibles[id];
    }
    public uint GetCollectibleIdentifier() { return collectibleIdentifier; }
    #endregion

    #region String Handling
    public override string ToString()
    {
        string output = "";

        output += $"Collectible Identifier: {Mathm.IntToBinaryString(collectibleIdentifier)} ({collectibleIdentifier})\n";
        output += $". > Definition: {Mathm.GetBinaryRange(collectibleIdentifier, 0, 8)}\n";
        output += $". > Quality: {Mathm.GetBinaryRange(collectibleIdentifier, 8, 4)}\n";
        output += $". > Material: {Mathm.GetBinaryRange(collectibleIdentifier, 12, 4)}\n";
        output += $". > Defect: {Mathm.GetBinaryRange(collectibleIdentifier, 16, 4)}\n";
        output += $". > Other: {Mathm.GetBinaryRange(collectibleIdentifier, 20, 4)}\n";
        output += $". > Quantity: {Mathm.GetBinaryRange(collectibleIdentifier, 24, 8)}\n";
        output += "\n";

        output += $"Stored Collectible: {(storedCollectible != null ? storedCollectible.ToString() : "Null")}\n";

        return output;
    }
    #endregion
}
