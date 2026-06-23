using UnityEngine;

public class ASM_Movement : AbilitySlotModifier
{
    public AbilitySlot.AdjacentDirection[] chainDirections = new AbilitySlot.AdjacentDirection[1];

    public override void ApplyToSlot(AbilitySlot slot)
    {
        base.ApplyToSlot(slot);

        slot.chainDirections.AddRange(chainDirections);
    }
}
