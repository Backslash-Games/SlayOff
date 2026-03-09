using UnityEngine;

[CreateAssetMenu(fileName = "New Collectible", menuName = "SlayOff/Content/Collectible")]
public class CollectibleDefinition : ScriptableObject
{
    [SerializeField] private string name;
    [SerializeField] private int price;
    [SerializeField] private Sprite sprite;

    #region Get Methods
    /// <summary>
    ///     Gets the name of the collectible
    /// </summary>
    /// <returns>string</returns>
    public string GetName() { return name; }
    /// <summary>
    ///     Gets the price of the collectible
    /// </summary>
    /// <returns>int</returns>
    public int GetPrice() { return price; }
    /// <summary>
    ///     Gets the image of the collectible
    /// </summary>
    /// <returns>Texture2D</returns>
    public Sprite GetSprite() { return sprite; }
    #endregion
}
