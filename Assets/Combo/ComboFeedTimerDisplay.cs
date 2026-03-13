using UnityEngine;
using UnityEngine.UI;

public class ComboFeedTimerDisplay : MonoBehaviour
{
    private InventoryHandler inventoryHandler;
    [SerializeField] private Image maskImage;
    [SerializeField] private float timerBuffer = 1.2f;

    private void Start()
    {
        SetFeedHeight(0);
    }

    /// <summary>
    ///     Updates the feed timer
    /// </summary>
    public void TickFeedTimer() { SetFeedHeight(Mathf.Clamp01(GetInventoryHandler().GetCurrentComboTimerPercentage() * timerBuffer)); }

    /// <summary>
    ///     Sets the feed height based on time
    /// </summary>
    /// <param name="time">Input time</param>
    private void SetFeedHeight(float time) { maskImage.fillAmount = time; }

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
