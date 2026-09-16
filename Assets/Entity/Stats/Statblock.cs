using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Statblock
{
    [SerializeField] private string blockName;
    [Space]
    [SerializeField] private Stat[] stats = new Stat[]
    {
        new Stat("Health", Stat.Tag.maxHealth, new Vector2(1, 9999)),
        new Stat("Size", Stat.Tag.size, new Vector2(0.25f, 9999)),
        new Stat("Weight", Stat.Tag.weight, new Vector2(-9999, 9999)),
        new Stat("Balance", Stat.Tag.balance, new Vector2(-9999, 9999)),

        new Stat("Attack - Lower", Stat.Tag.attackLower, new Vector2(0.1f, 9999)),
        new Stat("Attack - Higher", Stat.Tag.attackHigher, new Vector2(0.1f, 9999)),
        new Stat("Fire Rate", Stat.Tag.fireRate, new Vector2(0.01f, 9999)),
        new Stat("Reload Speed", Stat.Tag.reloadSpeed, new Vector2(0.01f, 9999)),

        new Stat("Speed", Stat.Tag.speed, new Vector2(-9999, 9999)),
        new Stat("Luck", Stat.Tag.luck, new Vector2(-9999, 9999)),
        new Stat("Entropy", Stat.Tag.entropy, new Vector2(-9999, 9999))
    };

    #region Constructors
    // Constructor for blank stat block
    public Statblock()
    {
        Reset();
    }
    // Constructor for copied stat block
    public Statblock(Statblock other)
    {
        blockName = other.blockName;
        stats = other.stats;
    }
    #endregion

    #region Data Handling
    // Resets the stat block
    public void Reset()
    {
        foreach (Stat stat in stats) stat.Reset();
    }

    // Method for ensuring that stats are set properly
    public void Validate()
    {
        foreach (Stat stat in stats) stat.Validate();
    }

    // Method for ensuring that stats are calculated properly
    public void Recalculate()
    {
        foreach (Stat stat in stats) stat.Recalculate();
    }

    #region Set
    public void SetBasic(Stat.Tag tag, float value)
    {
        Stat cStat = GetStatWithTag(tag);
        cStat.SetBasic(value);
    }
    public void SetAdditive(Stat.Tag tag, float value)
    {
        Stat cStat = GetStatWithTag(tag);
        cStat.SetAdditive(value);
    }
    public void SetPercentage(Stat.Tag tag, float value)
    {
        Stat cStat = GetStatWithTag(tag);
        cStat.SetPercentage(value);
    }
    public void SetCurrent(Stat.Tag tag, float value)
    {
        Stat cStat = GetStatWithTag(tag);
        cStat.SetCurrent(value);
    }
    #endregion
    #region Add
    public void AddBasic(Stat.Tag tag, float value)
    {
        Stat cStat = GetStatWithTag(tag);
        cStat.AddBasic(value);
    }
    public void AddAdditive(Stat.Tag tag, float value)
    {
        Stat cStat = GetStatWithTag(tag);
        cStat.AddAdditive(value);
    }
    public void AddPercentage(Stat.Tag tag, float value)
    {
        Stat cStat = GetStatWithTag(tag);
        cStat.AddPercentage(value);
    }
    public void AddCurrent(Stat.Tag tag, float value)
    {
        Stat cStat = GetStatWithTag(tag);
        cStat.AddCurrent(value);
    }
    #endregion
    #region Subtract
    public void SubtractBasic(Stat.Tag tag, float value)
    {
        Stat cStat = GetStatWithTag(tag);
        cStat.SubtractBasic(value);
    }
    public void SubtractAdditive(Stat.Tag tag, float value)
    {
        Stat cStat = GetStatWithTag(tag);
        cStat.SubtractAdditive(value);
    }
    public void SubtractPercentage(Stat.Tag tag, float value)
    {
        Stat cStat = GetStatWithTag(tag);
        cStat.SubtractPercentage(value);
    }
    public void SubtractCurrent(Stat.Tag tag, float value)
    {
        Stat cStat = GetStatWithTag(tag);
        cStat.SubtractCurrent(value);
    }
    #endregion
    #endregion

    #region Get Methods
    // Returns a stat that contains a tag
    public Stat GetStatWithTag(Stat.Tag tag)
    {
        // Access the dictionary and spit out the proper stat
        return stats[(int)tag];
    }

    #region Basic Get Methods
    // Gets the basic value
    public float GetBasic(Stat.Tag tag)
    {
        // Get the stat with tag
        return GetStatWithTag(tag).GetBasic();
    }
    // Gets the additive value
    public float GetAdditive(Stat.Tag tag)
    {
        // Get the stat with tag
        return GetStatWithTag(tag).GetAdditive();
    }
    // Gets the percentage value
    public float GetPercentage(Stat.Tag tag)
    {
        // Get the stat with tag
        return GetStatWithTag(tag).GetPercentage();
    }


    // Gets the current value
    public float GetValue(Stat.Tag tag)
    {
        // Get the stat with tag
        return GetStatWithTag(tag).Value();
    }
    // Gets the current value range
    public float GetValueRange(Stat.Tag lowerTag,  Stat.Tag upperTag)
    {
        Stat lower = GetStatWithTag(lowerTag);
        Stat upper = GetStatWithTag(upperTag);

        if (lower.Value() == upper.Value())
            return lower.Value();
        return Random.Range(lower.Value(), upper.Value());
    }
    #endregion
    #endregion
    #region Quick Get Methods
    public float GetHealth() { return GetValue(Stat.Tag.maxHealth); }
    public float GetSize() { return GetValue(Stat.Tag.size); }
    public float GetWeight() { return GetValue(Stat.Tag.weight); }
    public float GetBalance() { return GetValue(Stat.Tag.balance); }



    public float GetAttack() { return GetValueRange(Stat.Tag.attackLower, Stat.Tag.attackHigher); }
    public float GetFireRate() { return GetValue(Stat.Tag.fireRate); }
    public float GetReloadSpeed() { return GetValue(Stat.Tag.reloadSpeed); }



    public float GetSpeed() { return GetValue(Stat.Tag.speed); }
    public float GetLuck() { return GetValue(Stat.Tag.luck); }
    public float GetEntropy() { return GetValue(Stat.Tag.entropy); }
    #endregion

    #region Set Methods
    // Adds a modifer to the stat block
    public void ApplyModifer(StatBlockModifier modifier)
    {
        AdjustModifier(modifier, 1);
    }
    // Removes a modifier from the stat block
    public void RemoveModifier(StatBlockModifier modifier)
    {
        AdjustModifier(modifier, -1);
    }
    // Adjusts the stats in the modifier based on scale
    // -> Implemented to give parallel funcionality of apply and remove modifier
    private void AdjustModifier(StatBlockModifier modifier, float scale)
    {
        foreach (StatBlockModifier.Entry entry in modifier.entries)
        {
            float cValue = entry.GetValue() * scale;
            // Modify stat
            switch (entry.attribute)
            {
                case StatBlockModifier.Attribute.basic:
                    AddBasic(entry.tag, cValue);
                    break;
                case StatBlockModifier.Attribute.additive:
                    AddAdditive(entry.tag, cValue);
                    break;
                case StatBlockModifier.Attribute.percentage:
                    AddPercentage(entry.tag, cValue);
                    break;
                default: // Current
                    AddCurrent(entry.tag, cValue);
                    break;
            }
        }
        Recalculate();
    }
    #endregion


    // String output
    public override string ToString()
    {
        string retStr = $"<b>Statblock: {blockName}</b>";

        foreach (var stat in stats) retStr += $"- {stat}";

        return retStr;
    }
}
 