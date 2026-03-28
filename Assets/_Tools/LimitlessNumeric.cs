using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LimitlessNumeric
{
    [SerializeField] private List<ushort> numeric;
    private static readonly int max_numeric_container = 1000;

    #region Constructors
    public LimitlessNumeric(int value)
    {
        SetValue(value);
    }
    #endregion

    #region Operations
    /// <summary>
    ///     Adds another value to our limitless numeric
    /// </summary>
    /// <param name="other">Other Value</param>
    public void Add(int other)
    {

    }
    /// <summary>
    ///     Adds another limitless numeric to our current
    /// </summary>
    /// <param name="other">Limitless numeric</param>
    public void Add(LimitlessNumeric other)
    {

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
    #endregion

    #region Tools
    /// <summary>
    ///     Converts an integer to a limitless numeric
    /// </summary>
    /// <param name="value"></param>
    /// <returns>List of ushort</returns>
    private List<ushort> IntToNumeric(int value)
    {
        float wValue = value;
        int mIndex = 0;
        // Roll through working value and find the maximum index
        while(wValue >= max_numeric_container)
        {
            // Increase our current index
            mIndex++;
            // Reduce the working value
            wValue /= (float)max_numeric_container;
        }

        // Build a list of ushorts
        ushort[] cNumeric = new ushort[mIndex + 1];
        for(int i = mIndex; i >= 0; i--)
        {
            ushort cValue = (ushort)Mathf.FloorToInt(wValue);
            cNumeric[i] = cValue;

            wValue -= cValue;
            wValue *= max_numeric_container;
        }

        // Return the new list
        return new List<ushort>(cNumeric);
    }
    #endregion
}