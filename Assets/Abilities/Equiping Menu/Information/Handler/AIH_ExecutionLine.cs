using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AIH_ExecutionLine : MonoBehaviour
{
    /// <summary>
    ///     Primary layout group
    /// </summary>
    [SerializeField] private HorizontalLayoutGroup _layout;
    /// <summary>
    ///     Primary scroll rect
    /// </summary>
    [SerializeField] private ScrollRect _scrollRect;
    /// <summary>
    ///     Information prefab
    /// </summary>
    [SerializeField] private GameObject _informationContainer;
    /// <summary>
    ///     Read out for total execution time
    /// </summary>
    [SerializeField] private TextMeshProUGUI _totalTimeInformation;
    /// <summary>
    ///     Read out for total execution time
    /// </summary>
    [SerializeField] private TextMeshProUGUI _totalLengthInformation;

    /// <summary>
    ///     Tracks information requests
    /// </summary>
    private List<Ability> _informationRequests = new List<Ability>();
    /// <summary>
    ///     Active ability information
    /// </summary>
    private List<AbilityInformation> _activeInformation = new List<AbilityInformation>();
    /// <summary>
    ///     Routine that tracks addition
    /// </summary>
    private Coroutine _additionRoutine = null;

    /// <summary>
    ///     Time stamp for last addition
    /// </summary>
    private DateTime _additionTimeStamp = DateTime.Now;
    /// <summary>
    ///     Logged delay time spawn
    /// </summary>
    private TimeSpan _delayTimeSpan = TimeSpan.Zero;
    /// <summary>
    ///     Total logged execution time
    /// </summary>
    private TimeSpan _totalExecutionTime = TimeSpan.Zero;

    /// <summary>
    ///     Tracks chain length
    /// </summary>
    private int _chainLength = 0;

    /// <summary>
    ///     Adds ability information to the execution line
    /// </summary>
    /// <param name="ability"></param>
    public void AddInformation(Ability ability, AbilityTrace trace)
    {
        // Add ability to active requests
        _informationRequests.Add(ability);

        // Check if we need to start up the coroutine
        if (_additionRoutine != null) return;
        _additionRoutine = StartCoroutine(Enum_AddInformation(trace));
    }

    /// <summary>
    ///     Scrolls to the end of the scroll rect
    /// </summary>
    /// <returns>Wait for end of frame</returns>
    private IEnumerator Enum_AddInformation(AbilityTrace trace)
    {
        while (_informationRequests.Count > 0)
        {
            Ability ability = _informationRequests[0];
            // Spawn a new container
            AbilityInformation information = Instantiate(_informationContainer, _layout.transform).GetComponent<AbilityInformation>();
            information.SetAbility(ability, trace);
            // Calculate timespan
            _delayTimeSpan = DateTime.Now - _additionTimeStamp;
            _additionTimeStamp = DateTime.Now;
            _totalExecutionTime += _delayTimeSpan;
            // Update chain length
            _chainLength++;

            // WAIT
            yield return new WaitForEndOfFrame();

            // Connect last container
            int length = _activeInformation.Count;
            if (length > 0)
            {
                _activeInformation[length - 1].ConnectTo(information, _delayTimeSpan);
            }

            // Add ability information
            _activeInformation.Add(information);
            // Scroll to end
            _scrollRect.normalizedPosition = new Vector2(1, 0.5f);

            // Update readouts
            _totalTimeInformation.text = _totalExecutionTime.Seconds.ToString() + '.' + _totalExecutionTime.Milliseconds.ToString() + 's';
            _totalLengthInformation.text = _chainLength.ToString();

            // Remove information request
            _informationRequests.RemoveAt(0);
        }
        _additionRoutine = null;
    }

    /// <summary>
    ///     Resets active information
    /// </summary>
    public void ResetInformation()
    {
        while (_activeInformation.Count > 0)
        {
            Destroy(_activeInformation[0].gameObject);
            _activeInformation.RemoveAt(0);
        }

        _additionTimeStamp = DateTime.Now;
        _totalExecutionTime = TimeSpan.Zero;

        _chainLength = 0;
    }
}
