
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityInformation : MonoBehaviour
{
    /// <summary>
    ///     Ability Icon
    /// </summary>
    [SerializeField] private Image _icon;
    /// <summary>
    ///     Ability Name
    /// </summary>
    [SerializeField] private TextMeshProUGUI _name;

    /// <summary>
    ///     Connection arrow
    /// </summary>
    [SerializeField] private GameObject _arrow;
    /// <summary>
    ///     Connection delay information
    /// </summary>
    [SerializeField] private TextMeshProUGUI _delayInformation;


    /// <summary>
    ///     Sets information from ability
    /// </summary>
    /// <param name="ability">Ability</param>
    public void SetAbility(Ability ability)
    {
        if (ability == null) return;

        _icon.sprite = ability.icon;
        _name.text = ability.name;
    }

    /// <summary>
    ///     Connects two ability informations
    /// </summary>
    /// <param name="ability">Other ability</param>
    public void ConnectTo(AbilityInformation ability, TimeSpan delayTime)
    {
        // Set the arrow active
        _arrow.SetActive(true);
        // Position the parent
        _arrow.transform.localPosition = new Vector3((ability.transform.position.x - transform.position.x) / 2, _arrow.transform.localPosition.y, 0);

        // Write delay information
        _delayInformation.text = delayTime.Seconds.ToString();
        if (delayTime.Milliseconds > 0.0099f) _delayInformation.text += '.' + delayTime.ToString("fff").Substring(0, 2);
        _delayInformation.text += 's';
    }
}
