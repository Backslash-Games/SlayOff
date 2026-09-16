using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class AbilityInformation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    /// <summary>
    ///     Rect transform
    /// </summary>
    public RectTransform rectTransform;
    /// <summary>
    ///     Trace data
    /// </summary>
    public AbilityTrace.TraceData data;
    [Space]

    /// <summary>
    ///     Blank sprite
    /// </summary>
    [SerializeField] private Sprite _blankSprite;
    /// <summary>
    ///     Ability Icon
    /// </summary>
    [SerializeField] private Image _abilityIcon;
    /// <summary>
    ///     Ability Icon
    /// </summary>
    [SerializeField] private Image _ringIcon;
    /// <summary>
    ///     Ability Name
    /// </summary>
    [SerializeField] private TextMeshProUGUI _name;
    /// <summary>
    ///     Ability Name
    /// </summary>
    [SerializeField] private TextMeshProUGUI _keyText;
    /// <summary>
    ///     Connection delay information
    /// </summary>
    [SerializeField] private TextMeshProUGUI _cooldownInformation;
    [Space]


    /// <summary>
    ///     Connection Arrow Prefab
    /// </summary>
    [SerializeField] private GameObject _arrowPrefab = null;
    [SerializeField] private List<AI_Arrow> _connectionArrows = new List<AI_Arrow>();

    [Space]
    /// <summary>
    ///     Connected slot
    /// </summary>
    [SerializeField] private AbilitySlot _connectedSlot = null;


    private void Update()
    {
        foreach(AI_Arrow arrow in _connectionArrows) arrow.TickTransform();
    }


    /// <summary>
    ///     Sets data for container
    /// </summary>
    /// <param name="data">Input data</param>
    /// <param name="trace">Trace</param>
    public void SetData(AbilityTrace.TraceData data, AbilityTrace trace)
    {
        this.data = data;
        SetSlot(trace.GetSlot(data.slotIndex), trace);
    }
    /// <summary>
    ///     Sets information based on slot
    /// </summary>
    /// <param name="slot">Input slot</param>
    /// <param name="trace">Trace</param>
    public void SetSlot(AbilitySlot slot, AbilityTrace trace)
    {
        if (slot == null) return;

        SetKey(slot, trace);
        SetAbility(slot.boundAbility, trace);
        SetModifier(slot.modifier, trace);
    }
    #region Sets
    /// <summary>
    ///     Sets the key information
    /// </summary>
    /// <param name="slot">Slot</param>
    /// <param name="trace">Trace</param>
    public void SetKey(AbilitySlot slot, AbilityTrace trace)
    {
        _keyText.text = slot.bindingName;
    }
    /// <summary>
    ///     Sets information from ability
    /// </summary>
    /// <param name="ability">Ability</param>
    /// <param name="trace">Trace</param>
    public void SetAbility(Ability ability, AbilityTrace trace)
    {
        if (ability == null)
        {
            _abilityIcon.sprite = _blankSprite;
            _name.text = "";
            _cooldownInformation.text = "";
            return;
        }

        _abilityIcon.sprite = ability.icon;
        _name.text = ability.GetName();

        _cooldownInformation.text = $"<color=grey><size=8>[{ability.cooldownTime}s]</size></color>\n{(ability.cooldownTime / trace.reductionRate).ToString("F2")}s";
    }
    /// <summary>
    ///     Sets information from the modifier
    /// </summary>
    /// <param name="modifier">Modifier</param>
    /// <param name="trace">Trace</param>
    public void SetModifier(AbilitySlotModifier modifier, AbilityTrace trace)
    {
        if (modifier == null)
        {
            _ringIcon.sprite = _blankSprite;
            return;
        }

        _ringIcon.sprite = modifier.icon;
    }
    #endregion

    /// <summary>
    ///     Connects two ability informations
    /// </summary>
    /// <param name="ability">Other ability</param>
    public void ConnectTo(AbilityInformation ability, TimeSpan delayTime)
    {
        // Create connection arrow
        AI_Arrow arrow = Instantiate(_arrowPrefab, transform).GetComponent<AI_Arrow>();
        // Update connection arrow
        arrow.Connect(this, ability);
        arrow.SetDelayInformation(delayTime);
        arrow.SetColor(delayTime);

        // Add to list
        _connectionArrows.Add(arrow);
    }

    private void SetConnectedSlotHighlight(bool state)
    {
        // Find connected slot
        if (_connectedSlot == null) _connectedSlot = AbilityInformationHandler.Instance.executionTree.currentTrace.GetSlot(data.slotIndex);
        if (_connectedSlot == null) return;

        // Highlight slot
        _connectedSlot.connectedHovering = state;
        _connectedSlot.SetHighlight(state);
        _connectedSlot.SetHighlightConnected(state);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetConnectedSlotHighlight(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetConnectedSlotHighlight(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right) return;
        AbilityInformationHandler.Instance.SetPopup(_connectedSlot, transform.position);
    }
}
