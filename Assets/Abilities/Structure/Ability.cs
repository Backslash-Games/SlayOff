using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class Ability : AbilitySlotEquipment
{
    #region Interfaces
    public virtual void OnTriggerAbility(AbilityTrace trace)
    {
        // Add ability to execution information chain
        AbilityInformationHandler.Instance.executionLine.AddInformation(this, trace);
    }
    #endregion
    #region Overrides
    public bool Equip(AbilitySlot slot)
    {
        // Check if an ability already exists here
        if (slot.boundAbility != null)
        {
            CancelEquip();
            return false;
        }
        // Bind the ability
        slot.BindAbility(this);
        return true;
    }
    protected override bool OnEquip(AbilitySlot slot) { return Equip(slot); }
    protected override void OnDisplayPopup()
    {
        AbilityInformationHandler.Instance.SetPopup(null, this, null, transform.position);
    }
    #endregion
}
