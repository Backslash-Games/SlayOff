using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Statblock
{
    [SerializeField] private string blockName;
    [Space]
    [SerializeField] private Stat attackLower = new Stat(Stat.Tag.attackLower, new Vector2(0.1f, 9999));
    [SerializeField] private Stat attackHigher = new Stat(Stat.Tag.attackHigher, new Vector2(0.1f, 9999));
    [SerializeField] private Stat fireRate = new Stat(Stat.Tag.fireRate, new Vector2(0.01f, 9999));
    [SerializeField] private Stat shotSpeed = new Stat(Stat.Tag.shotSpeed, new Vector2(.01f, 9999));
    [Space]
    [SerializeField] private Stat speed = new Stat(Stat.Tag.speed, new Vector2(-9999, 9999));
    [Space]
    [SerializeField] private Stat luck = new Stat(Stat.Tag.luck, new Vector2(-9999, 9999));
    [Space]
    [SerializeField] private Stat maxHealth = new Stat(Stat.Tag.maxHealth, new Vector2(1, 9999));
    [SerializeField] private Stat defense = new Stat(Stat.Tag.defense, new Vector2(0, 9999));
    [Space]
    [SerializeField] private Stat reloadSpeed = new Stat(Stat.Tag.reloadSpeed, new Vector2(0.01f, 9999)); // Reload speed impacts the percent speed in which the player can reload.

    private Dictionary<Stat.Tag, Stat> statIndex = null;


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
        attackLower = other.attackLower;
        attackHigher = other.attackHigher;
        fireRate = other.fireRate;
        shotSpeed = other.shotSpeed;
        speed = other.speed;
        luck = other.luck;
        maxHealth = other.maxHealth;
        defense = other.defense;
        reloadSpeed = other.reloadSpeed;
    }
    // Constructor for bullet stat block
    public Statblock(string blockName, Vector2 attack, float shotSpeed) : this(blockName, attack, 0, shotSpeed, 0, 0, 0, 0, 0)
    {
        this.blockName = blockName;

        attackLower.SetBasic(attack.x, true);
        attackHigher.SetBasic(attack.y, true);
        this.shotSpeed.SetBasic(shotSpeed, true);
    }
    // Constructor for full stat block using basic variables
    public Statblock(string blockName, Vector2 attack, float fireRate, float shotSpeed, float speed, float luck, float health, float defense, float reloadSpeed)
    {
        this.blockName = blockName;

        this.maxHealth.SetBasic(health);
        this.defense.SetBasic(defense, true);

        attackLower.SetBasic(attack.x, true);
        attackHigher.SetBasic(attack.y, true);

        this.speed.SetBasic(speed, true);
        this.luck.SetBasic(luck, true);

        this.fireRate.SetBasic(fireRate, true);
        this.shotSpeed.SetBasic(shotSpeed, true);
        this.reloadSpeed.SetBasic(reloadSpeed, true);
    }
    // Constructor for full stat block using stats
    #endregion

    #region Data Handling
    // Resets the stat block
    public void Reset()
    {
        attackLower.Reset();
        attackHigher.Reset();
        fireRate.Reset();
        shotSpeed.Reset();
        speed.Reset();
        luck.Reset();
        maxHealth.Reset();
        defense.Reset();
        reloadSpeed.Reset();
    }

    // Method for ensuring that stats are set properly
    public void Validate()
    {
        maxHealth.Validate();
        defense.Validate();

        speed.Validate();

        luck.Validate();

        attackLower.Validate();
        attackHigher.Validate();

        fireRate.Validate();
        reloadSpeed.Validate();
        shotSpeed.Validate();
    }

    // Method for ensuring that stats are calculated properly
    public void Recalculate()
    {
        maxHealth.Recalculate();
        defense.Recalculate();

        speed.Recalculate();

        luck.Recalculate();

        attackLower.Recalculate();
        attackHigher.Recalculate();

        fireRate.Recalculate();
        reloadSpeed.Recalculate();
        shotSpeed.Recalculate();
    }

    // Method for creating the stat dictionary for quick calls
    private void CreateDictionary()
    {
        // If the stat index has already been created then end early
        if (statIndex != null)
            return;
        // Initialize the dictionary
        statIndex = new Dictionary<Stat.Tag, Stat>();

        // Index all elements
        statIndex.Add(attackLower.GetTag(), attackLower);
        statIndex.Add(attackHigher.GetTag(), attackHigher);
        statIndex.Add(fireRate.GetTag(), fireRate);
        statIndex.Add(shotSpeed.GetTag(), shotSpeed);
        statIndex.Add(speed.GetTag(), speed);
        statIndex.Add(luck.GetTag(), luck);
        statIndex.Add(maxHealth.GetTag(), maxHealth);
        statIndex.Add(defense.GetTag(), defense);
        statIndex.Add(reloadSpeed.GetTag(), reloadSpeed);
    }

    #region Copy
    /// <summary>
    ///     Copies only the basic values from another statblock
    /// </summary>
    /// <param name="other">The other stat block</param>
    /// <param name="cleanExtras"> When set to true, runs reset before setting basic </param>
    public void CopyBasic(Statblock other, bool cleanExtras = false)
    {
        attackLower.SetBasic(other.attackLower.GetBasic(), cleanExtras);
        attackHigher.SetBasic(other.attackHigher.GetBasic(), cleanExtras);
        fireRate.SetBasic(other.fireRate.GetBasic(), cleanExtras);
        shotSpeed.SetBasic(other.shotSpeed.GetBasic(), cleanExtras);

        speed.SetBasic(other.speed.GetBasic(), cleanExtras);

        luck.SetBasic(other.luck.GetBasic(), cleanExtras);

        maxHealth.SetBasic(other.maxHealth.GetBasic(), cleanExtras);
        defense.SetBasic(other.defense.GetBasic(), cleanExtras);

        reloadSpeed.SetBasic(other.reloadSpeed.GetBasic(), cleanExtras);
    }
    #endregion
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

    /// <summary>
    ///     Checks if the current reload speed is the minmum
    /// </summary>
    /// <returns>True if current reload speed is at its minimum</returns>
    public bool isMinimumReloadSpeed()
    {
        return reloadSpeed.Value() <= reloadSpeed.GetRange().x;
    }

    // Returns a stat that contains a tag
    public Stat GetStatWithTag(Stat.Tag tag)
    {
        // Check if dictionary has been made
        CreateDictionary();

        // Access the dictionary and spit out the proper stat
        return statIndex[tag];
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
    public float GetCurrent(Stat.Tag tag)
    {
        // Get the stat with tag
        return GetStatWithTag(tag).Value();
    }
    #endregion
    #region Current Value Calls
    // Get attack
    public float GetAttack()
    {
        if (attackLower.Value() == attackHigher.Value())
            return attackLower.Value();

        return Random.Range(attackLower.Value(), attackHigher.Value());
    }
    // Returns specific attack values
    public float GetAttackLower()
    {
        return attackLower.Value();
    }
    // Returns specific attack values
    public float GetAttackHigher()
    {
        return attackHigher.Value();
    }
    // Returns the attack range
    public Vector2 GetAttackRange()
    {
        return new Vector2(attackLower.Value(), attackHigher.Value());
    }

    // Get fire rate
    public float GetFireRate()
    {
        return fireRate.Value();
    }
    // Get Shot Speed
    public float GetShotSpeed()
    {
        return shotSpeed.Value();
    }

    // Get Speed
    public float GetSpeed()
    {
        return speed.Value();
    }

    // Get Luck
    public float GetLuck()
    {
        return luck.Value();
    }

    // Get Max Health
    public float GetHealth()
    {
        return maxHealth.Value();
    }

    // Get Defense
    public float GetDefense()
    {
        return defense.Value();
    }

    // Get Reload Speed
    public float GetReloadSpeed()
    {
        return reloadSpeed.Value();
    }
    #endregion
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
        string retStr = "";

        retStr += $"----- {blockName} -----\n";

        retStr += $". > {attackLower}\n" +
                  $". > {attackHigher}\n";
        retStr += $". > {fireRate}\n";
        retStr += $". > {shotSpeed}\n";
        retStr += $". > {speed}\n";
        retStr += $". > {luck}\n";
        retStr += $". > {maxHealth}\n";
        retStr += $". > {defense}\n";
        retStr += $". > {reloadSpeed}";

        return retStr;
    }
}
