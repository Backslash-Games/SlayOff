using UnityEngine;
using TMPro;

public class ComboFeedTextDisplay : MonoBehaviour
{
    [SerializeField] private InventoryHandler inventoryHandler;
    [SerializeField] private TextMeshProUGUI feedDisplay;

    /// <summary>
    ///     Updates the feed display
    /// </summary>
    public void UpdateFeedDisplay() { SetComboDisplay(GetInventoryHandler().GetCurrentCombo()); }
    /// <summary>
    ///     Sets the combo display to "COMBO {amount}"
    /// </summary>
    /// <param name="amount">Input amount</param>
    private void SetComboDisplay(uint amount) { feedDisplay.text = $"COMBO {amount}"; }

    /// <summary>
    ///     Gets the inventory handler
    /// </summary>
    /// <returns>Inventory handler</returns>
    private InventoryHandler GetInventoryHandler()
    {
        if (inventoryHandler == null)
            inventoryHandler = InventoryHandler.Instance;
        return inventoryHandler;
    }
}
