using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class AbilityInformationColumn : MonoBehaviour
{
    /// <summary>
    ///     Contained layout
    /// </summary>
    public Transform layout = null;
    /// <summary>
    ///     Step information
    /// </summary>
    public AbilityTrace.TraceStep step;

    /// <summary>
    ///     Information prefab
    /// </summary>
    [SerializeField] private GameObject _informationContainer;
    /// <summary>
    ///     Current contained information
    /// </summary>
    private List<AbilityInformation> _containedInformation = new List<AbilityInformation>();

    /// <summary>
    ///     Adds information to the column
    /// </summary>
    public void Add(AbilityTrace.TraceStep data, AbilityTrace trace)
    {
        step = data;
        Build(trace);
    }

    private void Build(AbilityTrace trace)
    {
        // -> Add data
        foreach (AbilityTrace.TraceData value in step.data)
        {
            // Pull information
            AbilitySlot slot = trace.GetSlot(value.slotIndex);
            if (slot == null) return;

            // Spawn and log
            AbilityInformation information = Instantiate(_informationContainer, layout).GetComponent<AbilityInformation>();
            _containedInformation.Add(information);

            // Set information
            information.SetData(value, trace);
        }
    }

    /// <summary>
    ///     Connects two columns to each other
    /// </summary>
    /// <param name="other">Other column</param>
    /// <param name="trace">Active trace</param>
    public void Connect(AbilityInformationColumn other, AbilityTrace trace)
    {
        foreach(AbilityInformation information in _containedInformation)
        {
            foreach(AbilityTrace.TraceConnection connection in information.data.connections_out)
            {
                if (information.data.connections_in.Contains(connection)) continue;
                // Find target information in other
                AbilityInformation target = other.GetAbilityInformation(connection.data.slotIndex);
                information.ConnectTo(target, new System.TimeSpan());
            }
        }
    }

    /// <summary>
    ///     Pulls ability information from slot index
    /// </summary>
    /// <param name="slotIndex">Input slot index</param>
    /// <returns>Ability information with corresponding slot index</returns>
    public AbilityInformation GetAbilityInformation(int slotIndex)
    {
        foreach (AbilityInformation information in _containedInformation) 
            if (information.data.slotIndex == slotIndex) return information;
        return null;
    }
}
