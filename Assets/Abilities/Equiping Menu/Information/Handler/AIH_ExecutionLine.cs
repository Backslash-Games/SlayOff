using System;
using System.Collections;
using System.Collections.Generic;
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

    private DateTime _additionTimeStamp = DateTime.Now;
    private TimeSpan _delayTimeSpan = TimeSpan.Zero;

    /// <summary>
    ///     Adds ability information to the execution line
    /// </summary>
    /// <param name="ability"></param>
    public void AddInformation(Ability ability)
    {
        // Add ability to active requests
        _informationRequests.Add(ability);

        // Check if we need to start up the coroutine
        if (_additionRoutine != null) return;
        _additionRoutine = StartCoroutine(Enum_AddInformation());
    }

    /// <summary>
    ///     Scrolls to the end of the scroll rect
    /// </summary>
    /// <returns>Wait for end of frame</returns>
    private IEnumerator Enum_AddInformation()
    {
        while (_informationRequests.Count > 0)
        {
            Ability ability = _informationRequests[0];
            // Spawn a new container
            AbilityInformation information = Instantiate(_informationContainer, _layout.transform).GetComponent<AbilityInformation>();
            information.SetAbility(ability);
            // Calculate timespan
            _delayTimeSpan = DateTime.Now - _additionTimeStamp;
            _additionTimeStamp = DateTime.Now;

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
    }
}
