// Calculation: (Base + Additive) * (1 + Percentage) = Current

using UnityEngine;

[System.Serializable]
public class Stat
{
    public enum Tag { attackLower, attackHigher, fireRate, shotSpeed, speed, luck, currentHealth, maxHealth, defense, reloadSpeed };
    // Stat Tag
    [SerializeField] private Tag tag;
    // Allowed range for stat
    private Vector2 range;
    [Space]
    // Basic - Impacts the base value of the stat block. Used for things like equipment
    [SerializeField] private float basic;
    // Additive - Impacts the flat additive of the stat
    [SerializeField] private float additive;
    // Percentage - Impacts the percentage additive of the stat
    [SerializeField] private float percentage;
    // Current - Impacts the current value of the stat block. Used for things like ailments
    [SerializeField] private float current;


    // Constructors used for stats
    #region Constructors
    // Blank constructor
    public Stat() { }

    // Tag defintiion
    public Stat(Tag tag) 
    {
        SetTag(tag);
    }
    // Tag and Range defintiion
    public Stat(Tag tag, Vector2 range)
    {
        SetTag(tag);
        SetRange(range);
    }
    #endregion

    // Methods used to get values
    #region Get Methods

    // Quick call for current
    public float Value()
    {
        return GetCurrent();
    }
    // Gets the range value
    public Vector2 GetRange()
    {
        return range;
    }
    // Gets the tag
    public Tag GetTag()
    {
        return tag;
    }


    // Gets the basic value
    public float GetBasic()
    {
        return basic;
    }
    // Gets the additive value
    public float GetAdditive()
    {
        return additive;
    }
    // Gets the percentage value
    public float GetPercentage()
    {
        return percentage;
    }
    // Gets the percentage value
    public float GetCurrent()
    {
        return current;
    }
    #endregion

    // Methods used to set values
    #region Set Methods
    // Sets the current tag
    public void SetTag(Tag tag)
    {
        this.tag = tag;
    }
    // Sets the current range
    public void SetRange(Vector2 range)
    {
        this.range = range;
    }



    /// <summary>
    ///     Sets the basic value
    /// </summary>
    /// <param name="cleanExtras"> When set to true, runs reset before setting basic </param>
    public void SetBasic(float value, bool cleanExtras = false)
    {
        // Check for clean extras
        if (cleanExtras)
            Reset();


        // Set basic
        basic = value;
        Recalculate();
    }
    // Sets the basic value
    public void SetAdditive(float value)
    {
        additive = value;
        Recalculate();
    }
    // Sets the basic value
    public void SetPercentage(float value)
    {
        percentage = value;
        Recalculate();
    }
    // Sets the basic value
    public void SetCurrent(float value)
    {
        current = value;
        Validate();
    }
    #endregion

    // Methods used to compare values
    #region Compare Methods
    // Compares input tag to current tag
    public bool HasSimilarTag(Tag input)
    {
        return GetTag().Equals(input);
    }
    #endregion

    // Methods used in basic math
    #region Math Methods
    #region Basic
    public void AddBasic(float value)
    {
        basic += value;
        Recalculate();
    }
    public void SubtractBasic(float value)
    {
        basic -= value;
        Recalculate();
    }
    #endregion
    #region Additive
    public void AddAdditive(float value)
    {
        additive += value;
        Recalculate();
    }
    public void SubtractAdditive(float value)
    {
        additive -= value;
        Recalculate();
    }
    #endregion
    #region Percentage
    public void AddPercentage(float value)
    {
        percentage += value;
        Recalculate();
    }
    public void SubtractPercentage(float value)
    {
        percentage -= value;
        Recalculate();
    }
    #endregion
    #region Current
    public void AddCurrent(float value)
    {
        current += value;
        Validate();
    }
    public void SubtractCurrent(float value)
    {
        current -= value;
        Validate();
    }
    #endregion
    #endregion

    // Main Data handling methods
    #region Data Handling
    // Calculates the current stat based on the given value
    public void Recalculate()
    {
        // Basic + additive * (1 + percentage)
        current = (basic + additive) * (1 + percentage);
        Validate();
    }
    

    // Validates the current value.
    // (Ensures the value is in range)
    public void Validate()
    {
        // Clamp current to range
        current = Mathf.Clamp(current, range.x, range.y);
    }


    // Resets the Stat Values
    public void Reset()
    {
        basic = 0;
        additive = 0;
        percentage = 0;
        current = 0;
    }
    // Hard Resets EVERYTHING
    public void ResetAll()
    {
        tag = 0;
        range = Vector2.zero;
        Reset();
    }
    #endregion

    // Debug Methods
    #region Debug
    public override string ToString()
    {
        string output = "";

        output += $"{tag} > {current} ... {range} ... basic: {basic} > add: {additive} > percentage: {percentage}";

        return output;
    }
    #endregion
}
