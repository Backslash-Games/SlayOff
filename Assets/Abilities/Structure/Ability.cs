using HFHandyUtils;
using HFHandyUtils.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class Ability : DraggableComponent, ITrigger
{
    [Header("Data")]
    [SerializeField] public Sprite icon;
    [SerializeField] public Color color = Color.white;
    [SerializeField] public float cooldownTime = 1f;

    /// <summary>
    ///     Image that renders the icon
    /// </summary>
    private Image _image = null;

    protected override void Awake()
    {
        base.Awake();
        _image = GetComponent<Image>();
        _image.sprite = icon;
    }

    #region Interfaces
    public virtual void OnTrigger()
    {
        HFLogger.Log($"Blank Ability: \'{name}\'");
    }
    #endregion
    #region Overrides
    protected override bool OnDrop(PointerEventData eventData)
    {
        List<RaycastResult> results = GetHoveringResults(eventData);
        foreach (RaycastResult result in results)
        {
            // Check if the result is an ability slot
            AbilitySlot slot = result.gameObject.GetComponent<AbilitySlot>();
            if (slot != null && slot.PointInClickRegion(eventData.position))
            {
                slot.BindAbility(this);
                return true;
            }
        }
        return false;
    }
    #endregion
}
