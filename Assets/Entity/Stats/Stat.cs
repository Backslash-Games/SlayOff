// Calculation: (Base + Additive) * (1 + Percentage) = Current

using UnityEngine;

[System.Serializable]
public class Stat
{
    public enum Tag 
    { 
        maxHealth = 0, 
        size = 1, 
        weight = 2, 
        balance = 3, 
        
        attackLower = 4, 
        attackHigher = 5, 
        fireRate = 6, 
        reloadSpeed = 7, 
        speed = 8, 
        
        luck = 9, 
        entropy = 10
    };
    [SerializeField] private string title;
    // Stat Tag
    [SerializeField] private Tag tag;
    // Allowed range for stat
    private Vector2 range;
    [Space]

    // Basic - Impacts the base value of the stat block. Used for things like equipment
    // Additive - Impacts the flat BAP.y of the stat
    // Percentage - Impacts the BAP.z BAP.y of the stat
    [SerializeField] private Vector3 BAP;
    // Current - Impacts the current value of the stat block. Used for things like ailments
    [SerializeField] private float current;


    // Constructors used for stats
    #region Constructors
    // Blank constructor
    public Stat() { }

    // Tag defintiion
    public Stat(string title, Tag tag) 
    {
        this.title = title;
        SetTag(tag);
    }
    // Tag and Range defintiion
    public Stat(string title, Tag tag, Vector2 range)
    {
        this.title = title;
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


    // Gets the BAP.x value
    public float GetBasic()
    {
        return BAP.x;
    }
    // Gets the BAP.y value
    public float GetAdditive()
    {
        return BAP.y;
    }
    // Gets the BAP.z value
    public float GetPercentage()
    {
        return BAP.z;
    }
    // Gets the BAP.z value
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
    ///     Sets the BAP.x value
    /// </summary>
    /// <param name="cleanExtras"> When set to true, runs reset before setting BAP.x </param>
    public void SetBasic(float value, bool cleanExtras = false)
    {
        // Check for clean extras
        if (cleanExtras)
            Reset();


        // Set BAP.x
        BAP.x = value;
        Recalculate();
    }
    // Sets the BAP.x value
    public void SetAdditive(float value)
    {
        BAP.y = value;
        Recalculate();
    }
    // Sets the BAP.x value
    public void SetPercentage(float value)
    {
        BAP.z = value;
        Recalculate();
    }
    // Sets the BAP.x value
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

    // Methods used in BAP.x math
    #region Math Methods
    #region Basic
    public void AddBasic(float value)
    {
        BAP.x += value;
        Recalculate();
    }
    public void SubtractBasic(float value)
    {
        BAP.x -= value;
        Recalculate();
    }
    #endregion
    #region Additive
    public void AddAdditive(float value)
    {
        BAP.y += value;
        Recalculate();
    }
    public void SubtractAdditive(float value)
    {
        BAP.y -= value;
        Recalculate();
    }
    #endregion
    #region Percentage
    public void AddPercentage(float value)
    {
        BAP.z += value;
        Recalculate();
    }
    public void SubtractPercentage(float value)
    {
        BAP.z -= value;
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
        // Basic + BAP.y * (1 + BAP.z)
        current = (BAP.x + BAP.y) * (1 + BAP.z);
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
        BAP.x = 0;
        BAP.y = 0;
        BAP.z = 0;
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

        output += $"{title}::{tag}: {current} [BAP:{BAP}]";

        return output;
    }
    #endregion
}
