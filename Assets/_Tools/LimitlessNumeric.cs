using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LimitlessNumeric
{
    [SerializeField] private List<ushort> numeric;
    private static readonly ushort max_numeric_container = 1000;

    #region Constructors
    public LimitlessNumeric(int value)
    {
        SetValue(value);
    }
    public LimitlessNumeric(LimitlessNumeric numeric)
    {
        this.numeric = new List<ushort>(numeric.GetNumeric());
    }
    #endregion

    #region Operations
    /// <summary>
    ///     Adds another value to our limitless numeric
    /// </summary>
    /// <param name="other">Other Value</param>
    public void Add(int other)
    {
        Add(new LimitlessNumeric(other));
    }
    /// <summary>
    ///     Adds another limitless numeric to our current
    /// </summary>
    /// <param name="other">Limitless numeric</param>
    public void Add(LimitlessNumeric other)
    {
        // Pull other numeric list
        List<ushort> oList = new List<ushort>(other.GetNumeric());

        // Increase contents of current list to equal other list
        while (oList.Count > numeric.Count)
            numeric.Add(0);

        // Add each value from other into current
        for (int i = 0; i < oList.Count; i++)
        {
            numeric[i] += oList[i];
        }

        // Refactor list
        Refactor();
    }

    public void Multiply(int amount)
    {
        for(int i = 0; i < amount - 1; i++)
            Add(this);
    }
    #endregion
    #region Get/Set Methods
    /// <summary>
    ///     Sets the current numeric to a value
    /// </summary>
    /// <param name="value">New Value</param>
    public void SetValue(int value)
    {
        numeric = new List<ushort>(IntToNumeric(value));
    }
    /// <summary>
    ///     Gets the full numeric
    /// </summary>
    /// <returns>List of uShort</returns>
    public List<ushort> GetNumeric()
    {
        return numeric;
    }

    /// <summary>
    ///     Outputs text in a pretty format :3
    /// </summary>
    /// <returns>String</returns>
    public string PrettyPrint()
    {
        // Ensure we have a numeric to print
        if (numeric.Count <= 0)
            return "0";

        // Set up pretty print
        string output = GetValueString(numeric.Count - 1, false);
        for(int i = numeric.Count - 2; i >= 0; i--)
            output += $",{GetValueString(i)}";

        // Output print
        return output;
    }
    /// <summary>
    ///     Pulls a value from numeric and evaluates it as a string
    /// </summary>
    /// <returns>String</returns>
    private string GetValueString(int index, bool includeZeroes = true)
    {
        // Make sure we are in bounds
        if (index >= numeric.Count || index < 0)
            return $"[{index}::OOB]";

        // Get current string
        ushort cValue = numeric[index];
        string cString = cValue.ToString();
        // Check if we are including zeroes
        if (!includeZeroes)
            return cString;

        // Otherwise include zeroes
        if (cValue == 0)
            cString = "000";
        else if (cValue < 10)
            cString = $"00{cString}";
        else if (cValue < 100)
            cString = $"0{cString}";
        return cString;
    }
    #endregion

    #region Tools
    /// <summary>
    ///     Converts an integer to a limitless numeric
    /// </summary>
    /// <param name="value"></param>
    /// <returns>List of ushort</returns>
    private List<ushort> IntToNumeric(int value)
    {
        decimal wValue = value;
        int mIndex = 0;
        // Roll through working value and find the maximum index
        while(wValue >= max_numeric_container)
        {
            // Increase our current index
            mIndex++;
            // Reduce the working value
            wValue /= (decimal)max_numeric_container;
        }

        // Build a list of ushorts
        ushort[] cNumeric = new ushort[mIndex + 1];
        for(int i = mIndex; i >= 0; i--)
        {
            int cValue = Mathf.FloorToInt((float)wValue);
            cNumeric[i] = (ushort)cValue;

            wValue -= cValue;
            wValue *= max_numeric_container;
        }

        // Return the new list
        return new List<ushort>(cNumeric);
    }

    /// <summary>
    ///     Collapses each entry in the limitless numeric to ensure it doesn't exceede bounds
    /// </summary>
    private void Refactor()
    {
        for(int i = 0; i < numeric.Count; i++)
        {
            // Check if we have overflowed
            if(numeric[i] >= max_numeric_container)
            {
                // The amount we have overflowed by
                ushort amount = (ushort)Mathf.FloorToInt(numeric[i] / max_numeric_container);
                numeric[i] -= (ushort)(amount * max_numeric_container);

                // Increase next
                if (i + 1 < numeric.Count)
                    numeric[i + 1] += amount;
                else
                    numeric.Add(amount);
            }
        }
    }
    #endregion
}