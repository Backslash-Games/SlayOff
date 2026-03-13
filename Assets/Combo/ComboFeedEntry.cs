using UnityEngine;
using TMPro;

public class ComboFeedEntry : FeedEntry
{
    [SerializeField] private TextMeshProUGUI comboText;

    /// <summary>
    ///     Updates the renderer
    /// </summary>
    /// <typeparam name="T">Generic type - Must be uInt</typeparam>
    /// <param name="value">uInt Binary</param>
    public override void TickRenderer<T>(T value)
    {
        // Set the string value
        SetText($"+ {value} .......... $1");
    }
    /// <summary>
    ///     Handles the bump logic for Collectible Feed Entry
    /// </summary>
    /// <param name="amount">Value</param>
    public override void Bump(float amount) { AddPositionOverTime(Vector3.up * amount); }

    /// <summary>
    ///     Sets the combo text
    /// </summary>
    /// <param name="text">Input text</param>
    private void SetText(string text) { comboText.text = text; }
}
