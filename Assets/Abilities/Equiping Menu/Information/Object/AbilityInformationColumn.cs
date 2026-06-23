using System.Collections.Generic;
using UnityEngine;

public class AbilityInformationColumn : MonoBehaviour
{
    /// <summary>
    ///     Contained layout
    /// </summary>
    public Transform layout = null;

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
    public void Add(AbilityTrace.TraceData data, AbilityTrace trace)
    {
        // Pull information
        AbilitySlot slot = trace.GetSlot(data.slotIndex);
        if (slot.boundAbility == null) return; 

        // Spawn and log
        AbilityInformation information = Instantiate(_informationContainer, layout).GetComponent<AbilityInformation>();
        _containedInformation.Add(information);

        // Set information
        information.SetSlot(slot, trace);
    }
}
