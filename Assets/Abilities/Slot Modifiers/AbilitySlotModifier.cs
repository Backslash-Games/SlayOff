using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HFHandyUtils.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
public class AbilitySlotModifier : DraggableComponent
{
    /// <summary>
    ///     Modifier sprite
    /// </summary>
    public Sprite sprite = null;

    /// <summary>
    ///     Object renderer
    /// </summary>
    private Image _image = null;


    #region Unity Methods
    protected override void Awake()
    {
        base.Awake();
        _image = GetComponent<Image>();
        _image.sprite = sprite;
    }
    #endregion

    #region Virtual
    /// <summary>
    ///     Applies information to a slot
    /// </summary>
    /// <param name="slot">working slot</param>
    public virtual void ApplyToSlot(AbilitySlot slot)
    {
        slot.SetModifier(this);
        slot.chainDirections.Clear();
    }
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
                ApplyToSlot(slot);
                slot.SetHighlightConnected(true);
                return true;
            }
        }
        return false;
    }
    #endregion
}
