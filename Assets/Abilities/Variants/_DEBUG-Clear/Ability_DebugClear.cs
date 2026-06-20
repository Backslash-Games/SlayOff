using HFHandyUtils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Ability_DebugClear : Ability
{
    public override void OnTrigger() { }

    protected override bool OnDrop(PointerEventData eventData)
    {
        List<RaycastResult> results = GetHoveringResults(eventData);
        foreach (RaycastResult result in results)
        {
            // Check if the result is an ability slot
            AbilitySlot slot = result.gameObject.GetComponent<AbilitySlot>();
            if (slot != null && slot.PointInClickRegion(eventData.position))
            {
                slot.BindAbility(null);
                return true;
            }
        }
        return false;
    }
}
