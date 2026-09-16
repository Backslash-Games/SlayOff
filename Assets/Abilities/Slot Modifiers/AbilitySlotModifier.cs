using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class AbilitySlotModifier : AbilitySlotEquipment
{
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
    /// <summary>
    ///     Removes information from a slot
    /// </summary>
    /// <param name="slot">working slot</param>
    public virtual void RemoveFromSlot(AbilitySlot slot)
    {
        slot.SetModifier(null);
        slot.chainDirections.Clear();
    }
    #endregion
    #region Overrides
    public bool Equip(AbilitySlot slot)
    {
        if (slot.modifier != null)
        {
            CancelEquip();
            return false;
        }

        slot.SetHighlightConnected(false);
        ApplyToSlot(slot);
        slot.SetHighlightConnected(true);
        return true;
    }
    protected override bool OnEquip(AbilitySlot slot) { return Equip(slot); }
    protected override void OnDisplayPopup()
    {
        AbilityInformationHandler.Instance.SetPopup(null, null, this, transform.position);
    }
    #endregion
}
