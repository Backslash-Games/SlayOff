using HFHandyUtils;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilitySlot : MonoBehaviour
{
    public enum AdjacentDirection { Top_Right, Right, Bottom_Right, Top_Left, Left, Bottom_Left };
    public enum SlotType { Empty, Letter, Special, Function, Navigation, Arrow, Numpad };

    [Header("Information")]
    [SerializeField] private string bindingName = "";
    [SerializeField] private string actionName = "";
    [SerializeField] public SlotType slotType = SlotType.Empty;
    [Space]
    [SerializeField] public bool onCooldown = false;
    [SerializeField] public bool locked = false;
    [SerializeField] public bool enabled = true;

    [Header("Graphical")]
    [SerializeField] private TextMeshProUGUI displayName = null;
    [SerializeField] private Image displayImage = null;
    [SerializeField] private Image modifierIcon = null;
    [SerializeField] private Image abilityIcon = null;
    [SerializeField] private Image abilityIcon_shadow = null;
    [SerializeField] private Image abilityIcon_animation = null;
    [SerializeField] public Color color = Color.white;

    [Header("Binding")]
    [SerializeField] private Ability boundAbility;
    [SerializeField] private HFHandyUtils.Time.Cooldown abilityCooldown;
    [SerializeField] private AbilitySlot[] adjacentSlots = new AbilitySlot[6];

    [Header("Modfiers")]
    [SerializeField] private AbilitySlotModifier modifier = null;

    [Header("Chaining")]
    [SerializeField] public List<AdjacentDirection> chainDirections = new List<AdjacentDirection>();

    #region Private Variables
    // Slot Data
    private static Sprite s_BlankAbilityIcon = null;
    private static Sprite s_DefaultAbilityIcon = null;

    private Color _ClickVisualColor = Color.black;
    private Color _AbilityActivationResetColor = Color.black;

    // Records
    private float _ObjectVerticalPosition = 0;
    private float _AbilityIconVerticalOffset = 0;

    // Last click information
    private Vector2 _lastClick = Vector2.zero;
    #endregion
    #region Private Static Variables

    #region Debug
    /// <summary>
    ///     Flag that dictates if we are showing gizmos
    /// </summary>
    private static readonly bool s_ShowGizmos = true;
    #endregion

    #region Adjacent Settings
    /// <summary>
    ///     Search radius for adjacent assignments
    /// </summary>
    private static readonly float s_AdjacentRadius = 100;
    /// <summary>
    ///     Threshold for a top/bottom check
    /// </summary>
    private static readonly float s_TopBottomThreshold = 10f;
    #endregion

    #region Animation - General
    /// <summary>
    ///     Threshold for resetting animations
    /// </summary>
    private static readonly float s_AnimationResetThreshold = 1f;
    #endregion
    #region Animation - Click
    /// <summary>
    ///     Click animation's reset speed
    /// </summary>
    private static readonly float s_ClickAnimationResetSpeed = 10f;
    /// <summary>
    ///     Click animation default vertical offset
    /// </summary>
    private static readonly float s_ClickAnimationVerticalOffset = 15;
    #endregion
    #region Animation - Ability
    /// <summary>
    ///     Ability animation reset speed
    /// </summary>
    private static readonly float s_AbilityAnimationResetSpeed = 1.85f;
    /// <summary>
    ///     Ability animation fadeout delay
    /// </summary>
    private static readonly float s_AbilityAnimationFadeoutDelay = 4f;
    /// <summary>
    ///     Ability animation default vertical offset
    /// </summary>
    private static readonly float s_AbilityAnimationVerticalOffset = 75;
    #endregion

    #region Color
    /// <summary>
    ///     Darken scale for text color
    /// </summary>
    private static readonly float s_TextColorScale = 0.282353f;
    /// <summary>
    ///     Darken scale for text color
    /// </summary>
    private static readonly Color s_LockColor = new Color(0.3f, 0.3f, 0.3f, 1);
    #endregion

    #region Cooldown
    /// <summary>
    ///     Default cooldown time
    /// </summary>
    private static readonly float s_DefaultCooldownTime = 1.25f;
    /// <summary>
    ///     Ability slot chain delay
    /// </summary>
    private static readonly float s_ChainDelay = 0.24f;
    #endregion
    #region Interaction
    /// <summary>
    ///     Ability interaction distance
    /// </summary>
    private static readonly float s_ClickDistance = 30f;
    /// <summary>
    ///     Correction for ability interactions
    /// </summary>
    private static readonly Vector2 s_ClickOffset = Vector2.up * 2.5f;
    #endregion

    #endregion



    #region Sequencing
    private void Start()
    {
        // Setup information
        // -> Transform
        _ObjectVerticalPosition = transform.localPosition.y;
        _AbilityIconVerticalOffset = abilityIcon_animation.transform.localPosition.y;
        // -> Click
        _ClickVisualColor = new Color(color.r * 0.6f, color.g * 0.6f, color.b * 0.6f, 1);
        // -> Ability
        if (s_BlankAbilityIcon == null) s_BlankAbilityIcon = (Sprite)Resources.Load("Fallbacks/Sprites/Blank", typeof(Sprite));
        if (s_DefaultAbilityIcon == null) s_DefaultAbilityIcon = (Sprite)Resources.Load("Fallbacks/Sprites/Ability_Icon_Default", typeof(Sprite));
        _AbilityActivationResetColor = abilityIcon_animation.color = new Color(abilityIcon_animation.color.r, abilityIcon_animation.color.g, abilityIcon_animation.color.b, 0);

        // Setup cooldown
        float cooldownTime = boundAbility == null ? s_DefaultCooldownTime : boundAbility.cooldownTime;
        abilityCooldown = new HFHandyUtils.Time.Cooldown(this, cooldownTime, 1);
        abilityCooldown.OnStart += () => { onCooldown = true; };
        abilityCooldown.OnEnd += () => { onCooldown = false; };
        abilityCooldown.OnUpdate += UpdateFill;

        // Ensure ability is not set
        BindAbility(null);
    }
    private void OnDestroy()
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
        displayImage.fillAmount = abilityIcon.fillAmount = abilityCooldown.GetPercentComplete();
    }

    /// <summary>
    ///     Sets the modifier icon
    /// </summary>
    /// <param name="sprite">New sprite</param>
    private void SetModifierIcon(Sprite sprite)
    {
        if (modifierIcon == null) return;
        modifierIcon.sprite = sprite;
    }
    #endregion
    #region Trigger
    public void OnTriggerSlot(AbilityTrace trace)
    {
        // Check if the slot is locked
        if (locked) return;
        if (onCooldown) return;
        if (!trace.isAlive()) return;

        DisableSlot(trace);
        abilityCooldown.SetReductionRate(trace.reductionRate);
        // Start cooldown
        abilityCooldown.Start();

        // Add slot to trace
        trace.Add(this);

        // Trigger ability
        if (boundAbility != null) boundAbility.OnTriggerAbility(trace);

        // Run click visual
        StartClickAnimation(new List<AbilitySlot>(), s_ClickAnimationVerticalOffset);
        StartAbilityActivatedAnimation();

        // Chain
        StartChain(trace);
    }
    Coroutine c_Cotrouine = null;
    private void StartChain(AbilityTrace trace)
    {
        if(c_Cotrouine != null) StopCoroutine(c_Cotrouine);
        c_Cotrouine = StartCoroutine(RunChain(trace));
    }
    private IEnumerator RunChain(AbilityTrace trace)
    {
        yield return new WaitForSecondsRealtime(s_ChainDelay);
        foreach (AdjacentDirection direction in chainDirections)
        {
            int index = (int)direction;
            if (index < 0 || index >= adjacentSlots.Length) continue;
            if (adjacentSlots[index] != null && (adjacentSlots[index].enabled || trace.trackedSlots.Contains(adjacentSlots[index]))) adjacentSlots[index].OnTriggerSlot(trace);
        }
    }
    #endregion
    #region Ability
    /// <summary>
    ///     Binds a new ability to the slot
    /// </summary>
    /// <param name="ability">New ability</param>
    public void BindAbility(Ability ability)
    {
        boundAbility = ability;
        abilityIcon.sprite = abilityIcon_shadow.sprite = abilityIcon_animation.sprite = ability == null || ability.icon == null ? s_BlankAbilityIcon : ability.icon;
        abilityCooldown.SetBasicRate(ability == null ? s_DefaultCooldownTime : ability.cooldownTime);
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
        // Position and set visual
        Color cColor = abilityIcon_animation.color = new Color(abilityIcon_animation.color.r, abilityIcon_animation.color.g, abilityIcon_animation.color.b, 1);
        abilityIcon_animation.transform.localPosition = new Vector3(abilityIcon_animation.transform.localPosition.x, s_ClickAnimationVerticalOffset + _AbilityIconVerticalOffset, abilityIcon_animation.transform.localPosition.z);

        Vector3 cPosition = abilityIcon_animation.transform.localPosition;
        Vector3 tPosition = new Vector3(abilityIcon_animation.transform.localPosition.x, _AbilityIconVerticalOffset + s_AbilityAnimationVerticalOffset, abilityIcon_animation.transform.localPosition.z);

        // Move icon
        float cDistance = Vector3.Distance(abilityIcon_animation.transform.localPosition, tPosition);
        while (cDistance > s_AnimationResetThreshold)
        {
            yield return new WaitForEndOfFrame();
            abilityIcon_animation.color = Color.Lerp(_AbilityActivationResetColor, cColor, (cDistance * s_AbilityAnimationFadeoutDelay) / s_AbilityAnimationVerticalOffset);
            // Set position
            cPosition = Vector3.Lerp(cPosition, tPosition, Time.deltaTime * s_AbilityAnimationResetSpeed);
            abilityIcon_animation.transform.localPosition = cPosition + Mathf.Abs(transform.localPosition.y - _ObjectVerticalPosition) * Vector3.up;

            cDistance = Vector3.Distance(abilityIcon_animation.transform.localPosition, tPosition);
        }
        abilityIcon_animation.color = _AbilityActivationResetColor;
    }
    #endregion
    #endregion

    #region Click Interactions
    /// <summary>
    ///     Checks if a given point is in our click region
    /// </summary>
    /// <param name="point">Point</param>
    /// <returns>True if in region</returns>
    public bool PointInClickRegion(Vector2 point)
    {
        Vector2 cPoint = (Vector2)transform.position + s_ClickOffset;
        _lastClick = point;
        return Vector2.Distance(cPoint, point) <= s_ClickDistance;
    }
    #endregion
    #region Modifiers
    /// <summary>
    ///     Sets the basic components of a modifier
    /// </summary>
    /// <param name="modifier">Other modifier</param>
    public void SetModifier(AbilitySlotModifier modifier)
    {
        this.modifier = modifier;
        SetModifierIcon(modifier.sprite);
    }
    #endregion

    #region State Handling
    public void LockSlot()
    {
        locked = true;
        SetColor(s_LockColor);
    }

    public void EnableSlot(AbilityTrace trace)
    {
        // Return early to ensure no repeat calls
        if (enabled) return;

        // Set base information
        enabled = true;
        SetColor(Color.white);

        // Unbind events
        abilityCooldown.OnEnd -= trace.CheckFinish;
    }
    public void DisableSlot(AbilityTrace trace)
    {
        // Return early to ensure no repeat calls
        if (!enabled) return;

        // Set base information
        enabled = false;
        SetColor(Color.red);

        // Bind events
        abilityCooldown.OnEnd += trace.CheckFinish;
    }
    #endregion


    #region Debug
    private void OnDrawGizmos()
    {
        if (!s_ShowGizmos) return;

        // Draw default click region
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, s_ClickDistance);

        // Draw click comparison
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere((Vector2)transform.position + s_ClickOffset, s_ClickDistance);
        // Draw last click
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(_lastClick, 5);
    }
    private void OnDrawGizmosSelected()
    {
        if (!s_ShowGizmos) return;

        // Draw adjacent collection distance
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, s_AdjacentRadius);
        // Draw adjacent keys
        Gizmos.color = Color.green;
        foreach (AbilitySlot slot in adjacentSlots)
        {
            if (slot != null) Gizmos.DrawWireSphere(slot.transform.position, 10f);
        }
    }
    #endregion
}
