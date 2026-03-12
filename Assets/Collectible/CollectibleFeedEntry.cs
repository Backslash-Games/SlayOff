using UnityEngine;

public class CollectibleFeedEntry : FeedEntry
{
    [SerializeField] private CollectibleRenderer collectibleRenderer;

    /// <summary>
    ///     Updates the renderer
    /// </summary>
    /// <typeparam name="T">Generic type - Must be uInt</typeparam>
    /// <param name="value">uInt Binary</param>
    public override void TickRenderer<T>(T value)
    {
        // Get the integer value
        if (uint.TryParse(value.ToString(), out uint binary))
        {
            RenderCollectible(binary);
            return;
        }
        Debug.LogError($"Attempted a call to Tick Renderer with value of type {value.GetType()}");
    }
    /// <summary>
    ///     Handles the bump logic for Collectible Feed Entry
    /// </summary>
    /// <param name="amount">Value</param>
    public override void Bump(float amount) { AddRotationOverTime(amount); }


    /// <summary>
    ///     Renders collectible
    /// </summary>
    /// <param name="binary">Input collectible</param>
    public void RenderCollectible(uint binary) { collectibleRenderer.RenderCollectible(binary); }
}
