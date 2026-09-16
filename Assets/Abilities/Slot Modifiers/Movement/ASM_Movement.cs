using System.Text.RegularExpressions;
using UnityEngine;

public class ASM_Movement : AbilitySlotModifier
{
    public AbilitySlot.AdjacentDirection[] chainDirections = new AbilitySlot.AdjacentDirection[1];

    public override void ApplyToSlot(AbilitySlot slot)
    {
        base.ApplyToSlot(slot);

        slot.chainDirections.AddRange(chainDirections);
    }


    protected override string EquipmentFormat(string text)
    {
        text = Regex.Replace(text, "<direction>", CollectDirections());
        return text;
    }
    private string CollectDirections()
    {
        // Length 0
        if (chainDirections.Length <= 0) return "";
        // Length 1
        if (chainDirections.Length == 1) return GetDirectionString(0);
        // Length 2
        if (chainDirections.Length == 2) return $"{GetDirectionString(0)} & {GetDirectionString(1)}";
        // Length 2+
        string output = GetDirectionString(0);
        for (int i = 1; i < chainDirections.Length - 1; i++)
        {
            output += ", " + GetDirectionString(i);
        }
        output += " & " + GetDirectionString(chainDirections.Length - 1);
        return output;
    }
    private string GetDirectionString(int index)
    {
        if (index < 0 || index >= chainDirections.Length) return "ERROR";
        return chainDirections[index].ToString().Replace('_', ' ');
    }
}
