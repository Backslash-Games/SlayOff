using UnityEngine;
public static class Randomu
{
    #region Weighted Randomness
    /// <summary>
    ///     Quick call to grab the total weight based on a weighted list.
    /// </summary>
    /// <param name="weights">Weighted list</param>
    /// <returns>Combined total of weighted list</returns>
    public static int GetWeightedTotal(int[] weights)
    {
        // Return with error if weights has a null length
        if (weights.Length <= 0)
        {
            Debug.LogError("Length of weights is invalid (<= 0)");
            return 0;
        }

        // Return early if weights is of length 1
        if (weights.Length == 1)
            return weights[0];

            
        // Establish a total
        int total = 0;
        // For each value in weights add to the total
        foreach(int value in weights)
        {
            total += value;
        }
        // Return the total
        return total;
    }

    /// <summary>
    ///     Quick call to grab an index based on a weighted list.
    /// </summary>
    /// <param name="weights">Weighted list</param>
    /// <returns>Randomly chosen index based on weights.</returns>
    public static int GetWeightedIndex(int[] weights)
    {
        // Return with error if weights has a null length
        if(weights.Length <= 0)
        {
            Debug.LogError("Length of weights is invalid (<= 0)");
            return 0;
        }

        // Return early if weights is of length 1
        if (weights.Length == 1)
            return 0;


        // Get the total of the weighted list
        int total = GetWeightedTotal(weights);
        // Get a random value within range
        int rng = Random.Range(0, total);

        // Find the chosen value from the list; ignore the last value
        // -> The last value is used as the default return
        for (int i = 0; i < weights.Length - 1; i++)
        {
            // Check if rng is less than the current index
            // TRUE: Return the current index
            if (rng < weights[i])
                return i;
            // FALSE: Reduce rng by the value of the current index
            rng -= weights[i];
        }

        return weights.Length - 1;
    }
    #endregion
}
