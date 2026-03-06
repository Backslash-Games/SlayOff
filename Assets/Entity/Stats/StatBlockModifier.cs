// The stat block modifier allows for a quick and easy way for items (and other features)
//   to influence stat blocks. They make for clean implementation and concise readability
//   due to their lightweight nature.

using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StatBlockModifier
{
    // Holds a reference to all possible stat attributes
    // -> Basic - Impacts the base value of the stat block. Used for things like equipment
    // -> Additive - Impacts the flat additive of the stat
    // -> Percentage - Impacts the percentage additive of the stat
    // -> Current - Impacts the current value of the stat block. Used for things like ailments
    // --> Directs the stat to look at a value
    [System.Serializable]
    public enum Attribute { basic, additive, percentage, current };

    // Holds a reference to an entry within the block modifier
    // -> Block modifiers can impact multiple values at once
    [System.Serializable]
    public struct Entry
    {
        // Reference to the modifier tag
        public Stat.Tag tag;
        // Reference to the modifier attribute
        public Attribute attribute;
        // Reference to the influence
        public float value;

        // Pull the primary value
        public float GetValue()
        {
            // Return the value
            return value;
        }
    };



    // Lists all entries in the block modifier
    public List<Entry> entries = new List<Entry>();
}
