using HFHandyUtils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AbilityTrace : MonoBehaviour
{
    /// <summary>
    ///     Maximum size of a trace
    /// </summary>
    private readonly int _maxTraceSize = 100;
    /// <summary>
    ///     Standard reduction of rates
    /// </summary>
    private readonly float _rateReductionPercentage = 0.975f;

    /// <summary>
    ///     String to track trace id
    /// </summary>
    public string source = "NO SOURCE";
    /// <summary>
    ///     Trace tree
    /// </summary>
    public List<TraceStep> trace = new List<TraceStep>();
    /// <summary>
    ///     Primary data structure
    /// </summary>
    public List<AbilitySlot> trackedSlots = new List<AbilitySlot>();
    /// <summary>
    ///     Current working reduction rate
    /// </summary>
    public float reductionRate = 1;

    #region Structs
    #region Trace Step
    /// <summary>
    ///     Container of all information contained in a single step of a trace - Displayed as column
    /// </summary>
    public struct TraceStep
    {
        /// <summary>
        ///     Step number
        /// </summary>
        public byte stepNumber;
        /// <summary>
        ///     Contained data points
        /// </summary>
        public List<TraceData> data;

        /// <summary>
        ///     Default constructor
        /// </summary>
        /// <param name="stepNumber">Step number</param>
        /// <param name="data">Input data</param>
        public TraceStep(int stepNumber, List<TraceData> data)
        {
            this.stepNumber = (byte)stepNumber;
            this.data = data;
        }
    }
    #endregion
    #region Trace Data
    /// <summary>
    ///     Container of all information contained in a single execution of a slot - Displayed as information blurb
    /// </summary>
    public struct TraceData
    {
        /// <summary>
        ///     Trace data head point
        /// </summary>
        public byte slotIndex;
        /// <summary>
        ///     Trace connections
        /// </summary>
        public List<TraceConnection> connections;

        /// <summary>
        ///     Constructor for trace data
        /// </summary>
        /// <param name="slotIndex">Tracked slots index</param>
        /// <param name="connections">Tracked connections</param>
        public TraceData(int slotIndex, List<TraceConnection> connections)
        {
            this.slotIndex = (byte)slotIndex;
            this.connections = connections;
        }
    }
    #endregion
    #region Trace Connection
    /// <summary>
    ///     Conatiner of all connection information - Displayed as an arrow
    /// </summary>
    public struct TraceConnection
    {
        /// <summary>
        ///     Trace Data connection index
        /// </summary>
        public byte slotIndex;
    }
    #endregion
    #endregion



    public AbilityTrace(string source, AbilitySlot head)
    {
        this.source = source;
        Calculate(head);
    }

    /// <summary>
    ///     Rolls through all impacted abilities from head and pull data
    /// </summary>
    /// <param name="head">Head slot</param>
    private void Calculate(AbilitySlot head)
    {
        // Hold reference to current and queued slots
        List<AbilitySlot> currentSlots = new List<AbilitySlot> { head };
        List<AbilitySlot> queuedSlots = new List<AbilitySlot>(head.GetAllConnected());

        // Run as long as we have queued slots 
        int errorCheck = 0;
        while(currentSlots.Count > 0 && errorCheck < 255)
        {
            // On all current information...
            foreach(AbilitySlot slot in currentSlots)
            {
                // Add to tracking
                AddTracking(slot);
            }
            // Push slots into a step
            TraceStep step = SlotsToStep(currentSlots);
            trace.Add(step);

            // Move queued information into current information
            currentSlots = new List<AbilitySlot>(queuedSlots);
            queuedSlots.Clear();

            // Gather new queued information
            foreach(AbilitySlot slot in currentSlots)
            {
                List<AbilitySlot> nQueues = slot.GetAllConnected();
                // Check if we are already tracked, if not add to queued
                foreach (AbilitySlot queue in nQueues) if (!trackedSlots.Contains(queue)) queuedSlots.Add(queue);
            }

            // Increase error check
            errorCheck++;
        }
        if (errorCheck >= 255) HFLogger.LogError("ERROR CHECK OVERFLOWN");
    }
    #region Trace data conversion
    /// <summary>
    ///     Converts a list of slots to a trace step
    /// </summary>
    /// <param name="slots">Input slots</param>
    /// <returns>Trace step</returns>
    private TraceStep SlotsToStep(List<AbilitySlot> slots)
    {
        TraceStep step = new TraceStep(trace.Count, new List<TraceData>());
        foreach (AbilitySlot slot in slots) step.data.Add(SlotToData(slot));
        return step;
    }
    /// <summary>
    ///     Converts a slot to a trace data point
    /// </summary>
    /// <param name="trackedIndex">Tracked index</param>
    /// <param name="slot">Input ability slot</param>
    /// <returns>Trace data</returns>
    private TraceData SlotToData(AbilitySlot slot)
    {
        TraceData data = new TraceData(GetSlotIndex(slot), new List<TraceConnection>());
        return data;
    }
    #endregion
    #region Tracking data
    /// <summary>
    ///     Adds slot to tracking data
    /// </summary>
    /// <param name="slot">Input slot</param>
    private void AddTracking(AbilitySlot slot)
    {
        // Return early if this slot isnt tracked yet
        if (trackedSlots.Contains(slot)) return;
        if (!slot.enabled) return;

        // Add slot
        slot.DisableSlot(this);
        trackedSlots.Add(slot);
    }
    #endregion

    /// <summary>
    ///     Adds a new ability slot to the trace
    /// </summary>
    /// <param name="slot"></param>
    public void Add(AbilitySlot slot)
    {
        /*reductionRate = Mathf.Pow(_rateReductionPercentage, trace.Count);
        trace.Add(slot);
        if (!trackedSlots.Contains(slot)) trackedSlots.Add(slot);*/
    }

    /// <summary>
    ///     Checks if the trace is still alive
    /// </summary>
    /// <returns>True if alive</returns>
    public bool isAlive() { return trace.Count < _maxTraceSize; }
    /// <summary>
    ///     Checks for trace finish
    /// </summary>
    public void CheckFinish()
    {
        foreach(AbilitySlot slot in trackedSlots)
            if (slot.onCooldown) return;
        foreach (AbilitySlot slot in trackedSlots)
            slot.EnableSlot(this);
    }

    /// <summary>
    ///     Gets the tracked ability slot
    /// </summary>
    /// <param name="index">Input slot index</param>
    /// <returns>Ability slot</returns>
    public AbilitySlot GetSlot(int index) 
    {
        if (index < 0 || index >= trackedSlots.Count) return null;
        return trackedSlots[index]; 
    }
    /// <summary>
    ///     Gets the tracked slot index
    /// </summary>
    /// <param name="slot">Input slot</param>
    /// <returns>Slot index</returns>
    public int GetSlotIndex(AbilitySlot slot)
    {
        for(int i = 0; i < trackedSlots.Count; i++) if (trackedSlots[i] == slot) return i;
        return -1;
    }
}
