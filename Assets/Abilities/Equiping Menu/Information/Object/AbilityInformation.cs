
using HFHandyUtils;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class AbilityInformation : MonoBehaviour
{
    /// <summary>
    ///     Rect transform
    /// </summary>
    public RectTransform rectTransform;
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
    ///     Connection Arrow Prefab
    /// </summary>
    [SerializeField] private GameObject _arrowPrefab = null;
    /// <summary>
    ///     Connection Arrow
    /// </summary>
    [SerializeField] private AI_Arrow _connectionArrow = null;



    private void Update()
    {
        if (_connectionArrow != null) _connectionArrow.TickTransform();
    }


    public void SetSlot(AbilitySlot slot, AbilityTrace trace)
    {
        SetAbility(slot.boundAbility, trace);
    }
    /// <summary>
    ///     Sets information from ability
    /// </summary>
    /// <param name="ability">Ability</param>
    public void SetAbility(Ability ability, AbilityTrace trace)
    {
        if (ability == null) return;

        _icon.sprite = ability.icon;
        _name.text = ability.name;

        _cooldownInformation.text = $"<color=grey><size=8>[{ability.cooldownTime}s]</size></color>\n{(ability.cooldownTime / trace.reductionRate).ToString("F2")}s";
    }

    /// <summary>
    ///     Connects two ability informations
    /// </summary>
    /// <param name="ability">Other ability</param>
    public void ConnectTo(AbilityInformation ability, TimeSpan delayTime)
    {
        // Create connection arrow
        if (_connectionArrow == null) _connectionArrow = Instantiate(_arrowPrefab, transform).GetComponent<AI_Arrow>();
        // Update connection arrow
        _connectionArrow.Connect(this, ability);
        _connectionArrow.SetDelayInformation(delayTime);
        _connectionArrow.SetColor(delayTime);
    }
}
