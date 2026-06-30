using HFHandyUtils.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using HFHandyUtils;

public class AbilitySlotEquipment : DraggableComponent
{
    [Header("Info")]
    [SerializeField] private string _name = string.Empty;
    [Multiline, SerializeField] private string _description = string.Empty;

    [Header("Data")]
    public Sprite icon;
    public Color color = Color.white;
    public float cooldownTime = 1f;

    /// <summary>
    ///     Image that renders the icon
    /// </summary>
    private Image _image = null;

    protected override void Awake()
    {
        base.Awake();
        _image = GetComponent<Image>();
        if(_image != null) _image.sprite = icon;
    }

    #region Control
    /// <summary>
    ///     Method that handles canceling slotting equippment
    /// </summary>
    protected void CancelEquip()
    {
        HFLogger.Log("Canceling equipping function of " + name);
        ForceCancel(AbilityInputHandler.Instance.pointerEventData);
    }
    #endregion

    #region Virutals
    /// <summary>
    ///     Logic run when the equipment is placed in a slot
    /// </summary>
    /// <param name="slot">Found Slot</param>
    protected virtual bool OnEquip(AbilitySlot slot) { return false; }
    /// <summary>
    ///     Logic run for displaying the popup
    /// </summary>
    protected virtual void OnDisplayPopup() { }
    /// <summary>
    ///     Formats information for output
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    protected virtual string EquipmentFormat(string text) { return text; }
    #endregion
    #region Overrides
    protected override bool OnDrop(PointerEventData eventData)
    {
        List<RaycastResult> results = GetHoveringResults(eventData);
        foreach (RaycastResult result in results)
        {
            // Check if the result is an ability slot
            AbilitySlot slot = result.gameObject.GetComponent<AbilitySlot>();
            if (slot != null && slot.PointInClickRegion(eventData.position))
            {
                if (slot.locked) return false;
                return OnEquip(slot);
            }
        }
        return false;
    }
    #endregion

    #region Format
    public string GetName()
    {
        return EquipmentFormat(AbilityInformationHandler.Instance.Format(_name));
    }
    public string GetDescription()
    {
        return EquipmentFormat(AbilityInformationHandler.Instance.Format(_description));
    }
    #endregion
    #region Popup
    protected override void OnClickDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right) return;
        OnDisplayPopup();
    }
    #endregion
}
