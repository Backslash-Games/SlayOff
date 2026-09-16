using HFHandyUtils.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AbilitySlotPickupDummy : AbilitySlotEquipment
{
    [SerializeField] private Sprite _blankSprite = null;
    [Space]
    [SerializeField] private AbilitySlot _origin = null;
    [SerializeField] private Ability _ability = null;
    [SerializeField] private AbilitySlotModifier _modifier = null;
    [Space]
    [SerializeField] private Image _abilityImage = null;
    [SerializeField] private Image _modifierImage = null;


    /// <summary>
    ///     Picks up the dummy with established information
    /// </summary>
    /// <param name="eventData">Input event data</param>
    /// <param name="position">Initial position</param>
    public void Pickup(PointerEventData eventData, AbilitySlot origin)
    {
        ResetContainer();
        _origin = origin;
        transform.position = origin.transform.position;
        resetPosition = origin.transform.position;
        ForcePickup(eventData);
    }

    #region Overrides
    private bool settleReset = false;
    protected override bool OnDrop(PointerEventData eventData)
    {
        bool flag = base.OnDrop(eventData);
        settleReset = !flag;
        if (!settleReset) ResetContainer();
        return flag;
    }
    protected override bool OnEquip(AbilitySlot slot)
    {
        if (_ability != null) return _ability.Equip(slot);
        if (_modifier != null) return _modifier.Equip(slot);
        return false;
    }
    protected override void OnSettle()
    {
        if (!settleReset) return;
        if (_origin != null) OnEquip(_origin);
        ResetContainer();
    }
    #endregion

    #region Graphical
    /// <summary>
    ///     Updates contained graphics
    /// </summary>
    private void TickGraphics()
    {
        _abilityImage.sprite = _ability == null ? _blankSprite : _ability.icon;
        _modifierImage.sprite = _modifier == null ? _blankSprite : _modifier.icon;
    }
    #endregion
    #region Data Management
    /// <summary>
    ///     Sets the ability of the dummy
    /// </summary>
    /// <param name="ability">New ability</param>
    public void SetAbility(Ability ability) 
    { 
        _ability = ability;
        TickGraphics();
    }
    /// <summary>
    ///     Sets the modifier of the dummy
    /// </summary>
    /// <param name="modifier">New Modifier</param>
    public void SetModifier(AbilitySlotModifier modifier) 
    { 
        _modifier = modifier;
        TickGraphics();
    }
    /// <summary>
    ///     Resets container information
    /// </summary>
    public void ResetContainer()
    {
        _ability = null;
        _modifier = null;
        TickGraphics();
    }
    #endregion
}
