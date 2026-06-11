using HFHandyUtils;
using HFHandyUtils.Time;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.UI;

public class AbilitySlot : MonoBehaviour, ITrigger
{
    public enum AdjacentDirection { Top_Right, Right, Bottom_Right, Top_Left, Left, Bottom_Left };
    public enum SlotType { Empty, Letter, Special, Function, Navigation, Arrow, Numpad };

    [Header("Information")]
    [SerializeField] private string bindingName = "";
    [SerializeField] private string actionName = "";
    [SerializeField] public SlotType slotType = SlotType.Empty;
    [Space]
    [SerializeField] public bool onCooldown = false;
    [SerializeField] private bool locked = false;

    [Header("Graphical")]
    [SerializeField] private TextMeshProUGUI displayName = null;
    [SerializeField] private Image displayImage = null;
    [SerializeField] private Image abilityIcon = null;
    [SerializeField] public Color color = Color.white;

    [Header("Binding")]
    [SerializeField] private Ability boundAbility;
    [SerializeField] private HFHandyUtils.Time.Cooldown abilityCooldown;
    [SerializeField] private AbilitySlot[] adjacentSlots = new AbilitySlot[6];

    [Header("Chaining")]
    [SerializeField] private List<AdjacentDirection> chainDirections = new List<AdjacentDirection>();

    private static readonly bool s_ShowGizmos = false;

    private static readonly float s_AdjacentRadius = 100;
    private static readonly float s_TopBottomThreshold = 10f;

    private static readonly float s_AnimationResetThreshold = 1f;

    private static readonly float s_ClickAnimationResetSpeed = 10f;
    private static readonly float s_ClickAnimationVerticalOffset = 15;

    private static readonly float s_AbilityAnimationResetSpeed = 1.85f;
    private static readonly float s_AbilityAnimationFadeoutDelay = 4f;
    private static readonly float s_AbilityAnimationVerticalOffset = 75;

    private static readonly float s_TextColorScale = 0.282353f;

    private static readonly float s_DefaultCooldownTime = 1.25f;
    private static readonly float s_ChainDelay = 0.24f;

    // Slot Data
    private static Sprite s_DefaultAbilityIcon = null;

    private Color _ClickVisualColor = Color.black;
    private Color _AbilityActivationResetColor = Color.black;


    // Records
    private float _ObjectVerticalPosition = 0;
    private float _AbilityIconVerticalOffset = 0;

    #region Sequencing
    private void OnEnable()
    {
        // Setup information
        // -> Transform
        _ObjectVerticalPosition = transform.localPosition.y;
        _AbilityIconVerticalOffset = abilityIcon.transform.localPosition.y;
        // -> Click
        _ClickVisualColor = new Color(color.r * 0.6f, color.g * 0.6f, color.b * 0.6f, 1);
        // -> Ability
        if (s_DefaultAbilityIcon == null) s_DefaultAbilityIcon = abilityIcon.sprite;
        _AbilityActivationResetColor = abilityIcon.color = new Color(abilityIcon.color.r, abilityIcon.color.g, abilityIcon.color.b, 0);

        // Setup cooldown
        float cooldownTime = boundAbility == null ? s_DefaultCooldownTime : boundAbility.cooldownTime;
        abilityCooldown = new HFHandyUtils.Time.Cooldown(this, cooldownTime, 1);
        abilityCooldown.OnStart += () => { onCooldown = true; };
        abilityCooldown.OnEnd += () => { onCooldown = false; };
        abilityCooldown.OnUpdate += UpdateFill;
    }
    private void OnDisable()
    {
        if(abilityCooldown != null) abilityCooldown.RemoveAllListeners();
    }
    #endregion
    #region Graphical
    /// <summary>
    ///     Sets the name of the slot
    /// </summary>
    /// <param name="bindingName">Slot name</param>
    public void SetName(string bindingName, string actionName)
    {
        this.bindingName = bindingName;
        this.actionName = actionName;

        if (displayName != null)
            displayName.text = bindingName;

        name = actionName + " - " + bindingName;
    }

    /// <summary>
    ///     Sets the color of the slot
    /// </summary>
    /// <param name="color">New color</param>
    public void SetColor(Color color)
    {
        this.color = color;
        UpdateColor(color);
    }
    /// <summary>
    ///     Updates the color of the slot
    /// </summary>
    public void UpdateColor(Color color)
    {
        displayImage.color = color;
        displayName.color = new Color(color.r * s_TextColorScale, color.g * s_TextColorScale, color.b * s_TextColorScale, 1f);
    }
    /// <summary>
    ///     Updates the fill of the key
    /// </summary>
    public void UpdateFill()
    {
        displayImage.fillAmount = abilityCooldown.GetPercentComplete();
    }
    #endregion
    #region Trigger
    public void OnTrigger()
    {
        // Check if the slot is locked
        if (locked) return;
        if (onCooldown) return;
        abilityCooldown.Start();

        // Debug
        HFLogger.Log("Triggered " + bindingName);

        // Trigger ability
        if (boundAbility != null) boundAbility.OnTrigger();

        // Run click visual
        StartClickAnimation(new List<AbilitySlot>(), s_ClickAnimationVerticalOffset);
        StartAbilityActivatedAnimation();

        // Chain
        StartChain();
    }
    Coroutine c_Cotrouine = null;
    private void StartChain()
    {
        if(c_Cotrouine != null) StopCoroutine(c_Cotrouine);
        c_Cotrouine = StartCoroutine(RunChain());
    }
    private IEnumerator RunChain()
    {
        yield return new WaitForSecondsRealtime(s_ChainDelay);
        foreach (AdjacentDirection direction in chainDirections)
        {
            int index = (int)direction;
            if (index < 0 || index >= adjacentSlots.Length) continue;
            if (adjacentSlots[index] != null) adjacentSlots[index].OnTrigger();
        }
    }
    #endregion

    #region Adjacent Handling
    /// <summary>
    ///     Setup the adjacents
    /// </summary>
    public void SetupAdjacents()
    {
        // Find adjacent keys
        AbilitySlot[] allSlots = FindObjectsByType<AbilitySlot>(FindObjectsSortMode.None);
        AbilitySlot[] nearSlots = new AbilitySlot[6];
        adjacentSlots = new AbilitySlot[6];

        int adjIndex = 0;
        for (int i = 0; i < allSlots.Length; i++)
        {
            if (allSlots[i] != this && Vector3.Distance(transform.position, allSlots[i].transform.position) <= s_AdjacentRadius)
            {
                nearSlots[adjIndex] = allSlots[i];
                adjIndex++;
                if (adjIndex >= nearSlots.Length) break;
            }
        }

        // { Top_Right, Right, Bottom_Right, Top_Left, Left, Bottom_Left }
        // Order keys
        foreach (AbilitySlot slot in nearSlots)
        {
            if (slot == null) continue;

            // Check for Top
            if(slot.transform.position.y > transform.position.y + s_TopBottomThreshold)
            {
                // Right
                if(slot.transform.position.x > transform.position.x)
                {
                    adjacentSlots[0] = slot;
                    continue;
                }
                // Left
                else
                {
                    adjacentSlots[3] = slot;
                    continue;
                }
            }
            // Check for Bottom
            else if (slot.transform.position.y < transform.position.y - s_TopBottomThreshold)
            {
                // Right
                if (slot.transform.position.x > transform.position.x)
                {
                    adjacentSlots[2] = slot;
                    continue;
                }
                // Left
                else
                {
                    adjacentSlots[5] = slot;
                    continue;
                }
            }
            // Check for side
            else
            {
                // Right
                if (slot.transform.position.x > transform.position.x)
                {
                    adjacentSlots[1] = slot;
                    continue;
                }
                // Left
                else
                {
                    adjacentSlots[4] = slot;
                    continue;
                }
            }
        }
    }

    private void ClickAdjacent(List<AbilitySlot> trace, float amount)
    {
        if (amount <= s_AnimationResetThreshold) return;

        foreach (AbilitySlot slot in adjacentSlots)
        {
            if (slot != null && !trace.Contains(slot)) slot.StartClickAnimation(trace, amount, 0.075f);
        }
    }
    #endregion
    #region Animation
    #region Trigger
    /// <summary>
    ///     Starts the click animation
    /// </summary>
    /// <param name="trace">Ripple trace</param>
    /// <param name="amount">Animation depth</param>
    /// <param name="delay">Delay before animation</param>
    public void StartClickAnimation(List<AbilitySlot> trace, float amount, float delay = 0)
    {
        if (cv_Coroutine != null) StopCoroutine(cv_Coroutine);
        cv_Coroutine = StartCoroutine(ClickAnimation(trace, amount, delay));
    }

    Coroutine cv_Coroutine = null;
    /// <summary>
    ///     Runs the animation for the clicking effect 
    /// </summary>
    /// <param name="trace">Ripple trace</param>
    /// <param name="amount">Animation depth</param>
    /// <param name="delay">Delay before animation</param>
    /// <returns>Wait</returns>
    private IEnumerator ClickAnimation(List<AbilitySlot> trace, float amount, float delay = 0)
    {
        trace.Add(this);
        yield return new WaitForSecondsRealtime(delay);

        // Set lowered position and color
        UpdateColor(Color.Lerp(_ClickVisualColor, color, (s_ClickAnimationVerticalOffset - amount) / s_ClickAnimationVerticalOffset));
        displayImage.color = new Color(displayImage.color.r, displayImage.color.g, displayImage.color.b, 1);
        transform.localPosition = new Vector3(transform.localPosition.x, _ObjectVerticalPosition - amount, transform.localPosition.z);

        yield return new WaitForEndOfFrame();
        ClickAdjacent(trace, amount / 3);

        while (Mathf.Abs(transform.localPosition.y - _ObjectVerticalPosition) > s_AnimationResetThreshold)
        {
            yield return new WaitForEndOfFrame();
            UpdateColor(Color.Lerp(displayImage.color, color, Time.deltaTime * s_ClickAnimationResetSpeed));
            transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(transform.localPosition.x, _ObjectVerticalPosition, transform.localPosition.z), Time.deltaTime * s_ClickAnimationResetSpeed);
        }
        // Reset
        UpdateColor(color);
        transform.localPosition = new Vector3(transform.localPosition.x, _ObjectVerticalPosition, transform.localPosition.z);
    }
    #endregion
    #region Ability Activation
    Coroutine aa_Coroutine = null;
    public void StartAbilityActivatedAnimation()
    {
        if (aa_Coroutine != null) StopCoroutine(aa_Coroutine);
        aa_Coroutine = StartCoroutine(AbilityActivatedAnimation());
    }

    private IEnumerator AbilityActivatedAnimation()
    {
        // Set sprite
        if (boundAbility == null || boundAbility.icon == null) abilityIcon.sprite = s_DefaultAbilityIcon;
        else abilityIcon.sprite = boundAbility.icon;

        // Position and set visual
        Color cColor = abilityIcon.color = new Color(abilityIcon.color.r, abilityIcon.color.g, abilityIcon.color.b, 1);
        abilityIcon.transform.localPosition = new Vector3(abilityIcon.transform.localPosition.x, s_ClickAnimationVerticalOffset + _AbilityIconVerticalOffset, abilityIcon.transform.localPosition.z);

        Vector3 cPosition = abilityIcon.transform.localPosition;
        Vector3 tPosition = new Vector3(abilityIcon.transform.localPosition.x, _AbilityIconVerticalOffset + s_AbilityAnimationVerticalOffset, abilityIcon.transform.localPosition.z);

        // Move icon
        float cDistance = Vector3.Distance(abilityIcon.transform.localPosition, tPosition);
        while (cDistance > s_AnimationResetThreshold)
        {
            yield return new WaitForEndOfFrame();
            abilityIcon.color = Color.Lerp(_AbilityActivationResetColor, cColor, (cDistance * s_AbilityAnimationFadeoutDelay) / s_AbilityAnimationVerticalOffset);
            // Set position
            cPosition = Vector3.Lerp(cPosition, tPosition, Time.deltaTime * s_AbilityAnimationResetSpeed);
            abilityIcon.transform.localPosition = cPosition + Mathf.Abs(transform.localPosition.y - _ObjectVerticalPosition) * Vector3.up;

            cDistance = Vector3.Distance(abilityIcon.transform.localPosition, tPosition);
        }
        abilityIcon.color = _AbilityActivationResetColor;
    }
    #endregion
    #endregion

    #region Locking
    public void LockSlot()
    {
        locked = true;
        SetColor(Color.red);
    }
    #endregion

    private void OnDrawGizmosSelected()
    {
        if (!s_ShowGizmos) return;

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, s_AdjacentRadius);

        Gizmos.color = Color.green;
        foreach (AbilitySlot slot in adjacentSlots)
        {
            if (slot != null) Gizmos.DrawWireSphere(slot.transform.position, 10f);
        }
    }
}
