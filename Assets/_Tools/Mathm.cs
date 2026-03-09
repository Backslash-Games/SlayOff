using System.Collections.Generic;
using UnityEngine;

public class Mathm
{
    #region Vector Calculations
    /// <summary>
    ///     Calculates a perfect spread of X positions around the edge of a unit circle. Angle between each position is uniform.
    /// </summary>
    /// <param name="amount">Positions around the unit circle</param>
    /// <returns>List of positions</returns>
    public static List<Vector3> SpreadPositions(int amount)
    {
        // Calculate degree seperation between each position
        float dSeperation = 360 / (float)amount;
        // -> Convert to radians
        float rSeperation = Mathf.Deg2Rad * dSeperation;

        // Hold variables for total positions and current rotation
        List<Vector3> positions = new List<Vector3>();
        float currentRotation = 0;

        // Roll through each entry and add a position
        for(int x = 0; x < amount; x++)
        {
            // Add a new position
            positions.Add(new Vector3(Mathf.Sin(currentRotation), 0, Mathf.Cos(currentRotation)));

            // Increase current rotation
            currentRotation += rSeperation;
        }

        // Return the list
        return positions;
    }

    /// <summary>
    ///     Gets the closest object to a position
    /// </summary>
    /// <typeparam name="T">Object Type that extends Monobehaviour</typeparam>
    /// <param name="input">Input objects</param>
    /// <param name="position">Current position</param>
    /// <returns>Closest Object</returns>
    public static T GetClosestObject<T>(List<T> input, Vector3 position) where T : MonoBehaviour
    {
        // Error check
        if (input.Count <= 0)
        {
            Debug.LogError("Tried to get closest without inputting any objects");
            return null;
        }

        // Hold a reference to initial length and object
        float minDistance = Vector3.Distance(position, input[0].transform.position);
        T cObject = input[0];
        // Run a loop to check the rest of the objects
        for(int i = 1; i < input.Count; i++)
        {
            // Do a check if distance is 0, return early. Minor Opimization
            if (minDistance == 0)
                return cObject;



            // Get the current minimum distance
            float cMinDistance = Vector3.Distance(position, input[0].transform.position);

            // Compare loop minimum with method minimum
            if(cMinDistance < minDistance)
            {
                // Log information
                minDistance = cMinDistance;
                cObject = input[i];
            }
        }

        // Return the current object
        return cObject;
    }

    /// <summary>
    ///     Gets the seperation percent between two vectors
    /// </summary>
    /// <returns>Value between 0-1, 1 being two identical vectors and 0 being opposites</returns>
    public static float GetVectorAccuracy(Vector3 primary, Vector3 other)
    {
        // Get the distance between the two vectors
        float cDistance = Vector3.Distance(primary.normalized, other.normalized);
        // Get the accuracy
        return Mathf.Clamp01(1 - (cDistance / 2));
    }

    /// <summary>
    ///     Removes the vertical axis from a vector
    /// </summary>
    /// <param name="input">Input</param>
    /// <returns>Vector with only x and z</returns>
    public static Vector3 RemoveVerticalAxis(Vector3 input)
    {
        return new Vector3(input.x, 0, input.z);
    }
    #endregion

    #region Binary Calculations
    /// <summary>
    ///     Converts an integer into a binary string
    /// </summary>
    /// <param name="input">Integer</param>
    /// <returns>Binary string</returns>
    public static string IntToBinaryString(uint input)
    {
        string binaryString = System.Convert.ToString(input, 2);
        int inserts = 0;

        // Space every 4th character
        for (int i = 0; i < binaryString.Length; i++)
        {
            // Check if we are pointing to the forth character
            if((i - inserts) % 4 == 0 && i != 0)
            {
                binaryString = binaryString.Insert(i, " ");
                inserts++;
                i++;
            }
        }

        return binaryString;
    }
    public static int GetBinaryLength(uint input)
    {
        // Track count
        int count = 0;
        uint modifided = input;
        // Keep right shifting until 0
        while (modifided != 0)
        {
            modifided >>= 1;
            count++;
        }
        // Return count
        return count;
    }

    /// <summary>
    ///     Gets a set of bits from a range
    /// </summary>
    /// <param name="input">Input</param>
    /// <param name="size">Binary Size</param>
    /// <param name="startIndex">Start Index</param>
    /// <param name="length">Range Length</param>
    /// <returns>Binary</returns>
    public static uint GetBinaryRange(uint input, int startIndex, int length)
    {
        // Shift the input right by the start index
        var shiftedBinary = input >> startIndex;
        // Get a mask defined by length
        var mask = (1 << length) - 1;
        // Get the final variable
        var outputBinary = shiftedBinary & mask;
        return (uint)outputBinary;
    }
    /// <summary>
    ///     Sets the binary range
    /// </summary>
    /// <param name="startIndex">Start index of the ser</param>
    /// <param name="set">Override integer</param>
    /// <param name="parent">Variable to set</param>
    /// <returns></returns>
    public static void SetBinaryRange(int startIndex, uint set, ref uint parent)
    {
        var setLength = GetBinaryLength(set);
        // Get the right and left side of the parent
        var parentRight = (uint)((1 << startIndex) - 1) & parent;

        // Get a modified final
        var parentModified = parent >> startIndex + setLength; // Setup the left side
        parentModified <<= setLength; // Reveal override bits
        parentModified += set; // Apply new bits
        parentModified <<= startIndex; // Reveal start index
        parentModified += parentRight; // Restore right side

        // Set the parent
        parent = parentModified;
    }
    /// <summary>
    ///     Clears a binary range
    /// </summary>
    /// <param name="startIndex">Start index</param>
    /// <param name="length">Length</param>
    /// <param name="parent">Variable to set</param>
    public static void ClearBinaryRange(int startIndex, int length, ref uint parent)
    {
        // Get the right and left side of the parent
        var parentRight = (uint)((1 << startIndex) - 1) & parent;

        // Get a modified final
        var parentModified = parent >> startIndex + length; // Setup the left side
        parentModified <<= length; // Reveal override bits
        parentModified <<= startIndex; // Reveal start index
        parentModified += parentRight; // Restore right side

        // Set the parent
        parent = parentModified;
    }
    #endregion
}
