using System.Collections.Generic;
using UnityEngine;

public class AIH_ExecutionTree : MonoBehaviour
{
    /// <summary>
    ///     Layout
    /// </summary>
    [SerializeField] private Transform _layout;

    /// <summary>
    ///     Column prefab
    /// </summary>
    [SerializeField] private GameObject _columnContainer;


    /// <summary>
    ///     Active ability information
    /// </summary>
    private List<AbilityInformationColumn> _activeColumns = new List<AbilityInformationColumn>();

    public void Build(AbilityTrace trace)
    {
        ResetAll();

        List<AbilityTrace.TraceStep> steps = trace.trace;
        foreach (AbilityTrace.TraceStep step in steps)
        {
            AbilityInformationColumn column = Instantiate(_columnContainer, _layout).GetComponent<AbilityInformationColumn>();
            _activeColumns.Add(column);
            // -> Add data
            foreach (AbilityTrace.TraceData data in step.data)
            {
                column.Add(data, trace);
            }
        }
    }

    public void ResetAll()
    {
        // -> COLUMNS
        while (_activeColumns.Count > 0)
        {
            Destroy(_activeColumns[0].gameObject);
            _activeColumns.RemoveAt(0);
        }
    }
}
