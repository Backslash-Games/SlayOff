using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AIH_ExecutionTree : MonoBehaviour
{
    public AbilityTrace currentTrace = null;

    /// <summary>
    ///     Layout
    /// </summary>
    [SerializeField] private Transform _layout;
    /// <summary>
    ///     Scroll rect
    /// </summary>
    [SerializeField] private ScrollRect _scrollRect = null;

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

        currentTrace = trace;
        List<AbilityTrace.TraceStep> steps = trace.trace;
        // Build steps
        foreach (AbilityTrace.TraceStep step in steps)
        {
            AbilityInformationColumn column = Instantiate(_columnContainer, _layout).GetComponent<AbilityInformationColumn>();
            column.Add(step, trace);
            _activeColumns.Add(column);
        }
        // Connect columns
        for(int i = 0; i < _activeColumns.Count - 1; i++)
        {
            _activeColumns[i].Connect(_activeColumns[i + 1], trace);
        }


        // Center scroll region
        _scrollRect.normalizedPosition = Vector2.one * 0.5f;
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
