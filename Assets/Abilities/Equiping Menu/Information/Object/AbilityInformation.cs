
using HFHandyUtils;
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
    ///     Connection delay information
    /// </summary>
    [SerializeField] private TextMeshProUGUI _cooldownInformation;

    /// <summary>
    ///     Connection arrow
    /// </summary>
    [SerializeField] private GameObject _arrow;
    /// <summary>
    ///     Connection arrow graphic
    /// </summary>
    [SerializeField] private Image _arrowGraphic;
    /// <summary>
    ///     Connection delay information
    /// </summary>
    [SerializeField] private TextMeshProUGUI _delayInformation;

    /// <summary>
    ///     Sets information from ability
    /// </summary>
    /// <param name="ability">Ability</param>
    public void SetAbility(Ability ability, float reductionRate)
    {
        if (ability == null) return;

        _icon.sprite = ability.icon;
        _name.text = ability.name;

        _cooldownInformation.text = $"<color=grey><size=8>[{ability.cooldownTime}s]</size></color>\n{(ability.cooldownTime / reductionRate).ToString("F2")}s";
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

        // Write delay information strength
        // -> Evaluate strength and convert to style consistent value
        Color strengthColor = HFColor.errorGradient.Evaluate((1 - (delayTime.Milliseconds / 1000f)) + delayTime.Seconds);
        Color.RGBToHSV(strengthColor, out float h, out _, out _);
        strengthColor = Color.HSVToRGB(h, 0.61f, 1);
        // -> Set color
        _arrowGraphic.color = _delayInformation.color = strengthColor;
    }
}
