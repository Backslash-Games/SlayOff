using HFHandyUtils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AbilityTrace : MonoBehaviour
{
    /// <summary>
    ///     Maximum size of a trace
    /// </summary>
    private readonly int _maxTraceSize = 1000;
    private readonly float _rateReductionPercentage = 0.975f;

    /// <summary>
    ///     String to track trace id
    /// </summary>
    public string source = "NO SOURCE";
    /// <summary>
    ///     Primary data structure
    /// </summary>
    public List<AbilitySlot> trace = new List<AbilitySlot>();
    /// <summary>
    ///     Primary data structure
    /// </summary>
    public List<AbilitySlot> trackedSlots = new List<AbilitySlot>();
    /// <summary>
    ///     Current working reduction rate
    /// </summary>
    public float reductionRate = 1;
    // Eventually we will have information to track modifiers between abilities

    #region Constructors
    public AbilityTrace(string source)
    {
        this.source = source;
    }
    #endregion

    /// <summary>
    ///     Adds a new ability slot to the trace
    /// </summary>
    /// <param name="slot"></param>
    public void Add(AbilitySlot slot)
    {
        reductionRate = Mathf.Pow(_rateReductionPercentage, trace.Count);
        trace.Add(slot);
        if (!trackedSlots.Contains(slot)) trackedSlots.Add(slot);
    }

    /// <summary>
    ///     Checks if the trace is still alive
    /// </summary>
    /// <returns>True if alive</returns>
    public bool isAlive() { return trace.Count < _maxTraceSize; }

    public void CheckFinish()
    {
        foreach(AbilitySlot slot in trackedSlots)
            if (slot.onCooldown) return;
        foreach (AbilitySlot slot in trackedSlots)
            slot.EnableSlot(this);
    }
}
