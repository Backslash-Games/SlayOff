using HFHandyUtils;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityInformationPopup : MonoBehaviour
{
    public AbilitySlot slot;
    public Ability ability;
    public AbilitySlotModifier modifier;
    [Space]
    public AbilityInputHandler inputHandler;
    [SerializeField] private RectTransform _parent;
    [SerializeField] private CanvasGroup _canvasGroup;
    [Space]
    [Header("Graphical Information")]
    [SerializeField] private RectTransform _eyes;
    [SerializeField] private RectTransform _shadow;
    [SerializeField] private float _shadowNudgeScale = 2;
    private Vector2 _regionSpawnLocation = Vector2.zero;
    private Vector2 _targetPosition = Vector2.zero;
    [Space]
    [Header("Screen Boundary Positions")]
    [SerializeField] private float _flipPercentage = 0.8f;
    [SerializeField] private int _rightPosition = 450;
    [SerializeField] private int _leftPosition = -60;
    [SerializeField] private int _bottomPosition = 10;
    [SerializeField] private int _defaultHeight = 50;
    [Space]
    [Header("Mouse Interactions")]
    [SerializeField] private float _moveSpeed = 21.5f;
    [SerializeField] private float _tiltSpeed = 21.5f;
    [SerializeField] private float _fadeSpeed = 21.5f;
    [SerializeField] private float _mouseAliveRadius = 100;
    [SerializeField] private float _mouseNudgeStrength = 0.01f;
    [SerializeField] private float _mouseTiltStrength = 10;
    [SerializeField] private float _teleportRange = 500;
    private Vector2 _mouseDirection = Vector2.zero;
    private Vector2 _mouseNudge = Vector2.zero;
    [Space]
    [Header("Key")]
    [SerializeField] private GameObject _key;
    [SerializeField] private TextMeshProUGUI _keyCode;
    [SerializeField] private TextMeshProUGUI _keyTitle;
    [SerializeField] private TextMeshProUGUI _keyDescription;
    [SerializeField] private string _keyTitleSuffix = string.Empty;
    [TextArea, SerializeField, Tooltip("Use <keycode> to display the current key")] private string _keyDefaultDescription = string.Empty;
    [TextArea, SerializeField, Tooltip("Use <keycode> to display the current key")] private string _keyDefaultLockDescription = string.Empty;
    [Space]
    [Header("Modifier")]
    [SerializeField] private GameObject _modifier;
    [SerializeField] private Image _modifierIcon;
    [SerializeField] private TextMeshProUGUI _modifierTitle;
    [SerializeField] private TextMeshProUGUI _modifierDescription;
    [Space]
    [Header("Ability")]
    [SerializeField] private GameObject _ability;
    [SerializeField] private Image _abilityIcon;
    [SerializeField] private TextMeshProUGUI _abilityTitle;
    [SerializeField] private TextMeshProUGUI _abilityDescription;

    #region Unity Methods
    private void OnEnable()
    {
        _canvasGroup.alpha = 0;
        _parent.gameObject.SetActive(false);
        inputHandler.OnPointerMoved += TickTracking;
    }
    private void LateUpdate()
    {
        SetHeight();

        transform.position = Vector2.Lerp(transform.position, _targetPosition, Time.deltaTime * _moveSpeed);
        _shadow.localPosition = Vector2.Lerp(_shadow.localPosition, _parent.localPosition - (Vector3)_mouseNudge * _shadowNudgeScale, Time.deltaTime * _moveSpeed);
        _shadow.sizeDelta = _parent.sizeDelta;

        _eyes.LookAt(_eyes.position + (Vector3)_mouseDirection + Vector3.forward * (1 / _mouseTiltStrength));
        transform.rotation = Quaternion.Lerp(transform.rotation, _eyes.rotation, Time.deltaTime * _tiltSpeed);
    }
    #endregion
    #region Main
    /// <summary>
    ///     Abridged set information call
    /// </summary>
    /// <param name="slot">Input slot</param>
    /// <param name="position">Interacted position</param>
    public void SetInformation(AbilitySlot slot, Vector3 position)
    {
        SetInformation(slot, slot.boundAbility, slot.modifier, position);
    }
    /// <summary>
    ///     Sets the information in the popup to a slot
    /// </summary>
    /// <param name="slot">Input slot</param>
    public void SetInformation(AbilitySlot slot, Ability ability, AbilitySlotModifier modifier, Vector3 position)
    {
        // Set variables
        this.slot = slot;
        this.ability = ability;
        this.modifier = modifier;

        // Control parent elements
        SetActive(this.slot != null || this.ability != null || this.modifier != null);
        SetPosition(position);

        // Control layout elements
        SetKey();
        SetModifier();
        SetAbility();

        // Set flip logic
        SetFlip();

        // Set graphical
        SetShadow();
    }
    #endregion

    #region Logic
    #region Parent
    /// <summary>
    ///     Sets the popup as active or inactive
    /// </summary>
    public void SetActive(bool state)
    {
        if (_routineActive != null) StopCoroutine(_routineActive);
        _routineActive = StartCoroutine(IEnum_SetActive(state));
    }
    Coroutine _routineActive = null;
    /// <summary>
    ///     Coroutine to handle fading
    /// </summary>
    /// <param name="state">New state</param>
    /// <returns>State</returns>
    private IEnumerator IEnum_SetActive(bool state)
    {
        float target = state ? 1f : 0f;
        if (state) _parent.gameObject.SetActive(true);
        while(_canvasGroup.alpha != target)
        {
            _canvasGroup.alpha = Mathf.Lerp(_canvasGroup.alpha, target, Time.deltaTime * _fadeSpeed);
            yield return new WaitForEndOfFrame();
        }
        if (!state) _parent.gameObject.SetActive(false);
    }
    /// <summary>
    ///     Sets position
    /// </summary>
    private void SetPosition(Vector3 position)
    {
        if (Vector3.Distance(transform.position, position) >= _teleportRange) transform.position = position;
        _regionSpawnLocation = _targetPosition = position;
    }
    #endregion
    #region Layout Elements
    /// <summary>
    ///     Sets key information
    /// </summary>
    private void SetKey()
    {
        // Set active
        if (slot == null)
        {
            _key.SetActive(false);
            return;
        }
        _key.SetActive(true);
        
        // Establish information
        string keyTitle = slot.bindingName + " " + _keyTitleSuffix;

        // Key binding
        _keyCode.text = slot.bindingName;

        // Formatted text
        _keyTitle.text = Format(keyTitle);
        _keyDescription.text = Format(_keyDefaultDescription).Replace("<keycode>", keyTitle);
        if (slot.locked) _keyDescription.text += "\n" + Format(_keyDefaultLockDescription);
    }
    /// <summary>
    ///     Sets modifier information
    /// </summary>
    private void SetModifier()
    {
        if (modifier == null)
        {
            _modifier.SetActive(false);
            return;
        }

        _modifier.SetActive(true);
        _modifierIcon.sprite = modifier.icon;

        // Formatted Text
        _modifierTitle.text = modifier.GetName();
        _modifierDescription.text = modifier.GetDescription();
    }
    /// <summary>
    ///     Sets ability information
    /// </summary>
    private void SetAbility()
    {
        if (ability == null)
        {
            _ability.SetActive(false);
            return;
        }

        _ability.SetActive(true);
        _abilityIcon.sprite = ability.icon;

        // Formatted Text
        _abilityTitle.text = ability.GetName();
        _abilityDescription.text = ability.GetDescription();
    }
    #endregion
    #region Screen Boundaries
    private void SetFlip()
    {
        //HFLogger.Log($"{_regionSpawnLocation} | {Screen.width * _flipPercentage} | {_leftPosition} | {_rightPosition} | {_regionSpawnLocation.x > Screen.width * _flipPercentage}");
        if (_regionSpawnLocation.x > Screen.width * _flipPercentage)
        {
            _parent.transform.localPosition = new Vector3(_leftPosition, _parent.transform.localPosition.y, _parent.transform.localPosition.z);
            return;
        }
        _parent.transform.localPosition = new Vector3(_rightPosition, _parent.transform.localPosition.y, _parent.transform.localPosition.z);
    }
    /// <summary>
    ///     Sets the height of the parent
    /// </summary>
    private void SetHeight()
    {
        // Grab the expected minimum height
        float expectedMinimum = _regionSpawnLocation.y + _defaultHeight - _parent.sizeDelta.y - _bottomPosition;
        HFLogger.Log($"{_regionSpawnLocation.y} + {_defaultHeight} - {_parent.sizeDelta.y} = {expectedMinimum}");
        float height = _defaultHeight;
        if (expectedMinimum < 0) height -= expectedMinimum;

        _parent.localPosition = new Vector3(_parent.localPosition.x, height, _parent.localPosition.z);
    }
    #endregion
    #region Graphics
    private void SetShadow()
    {
        _shadow.transform.localPosition = _parent.transform.localPosition;
    }
    #endregion
    #endregion
    #region Formatting
    private AbilityInformationHandler _informationHandler = null;
    private AbilityInformationHandler GetInformationHandler() 
    {
        if (_informationHandler == null)
            _informationHandler = AbilityInformationHandler.Instance;
        return _informationHandler;
    }
    private string Format(string text) { return GetInformationHandler().Format(text); }
    #endregion
    #region Tracking
    private void TickTracking(Vector2 position)
    {
        // Check if the mouse is still in position
        if (Vector2.Distance(position, _regionSpawnLocation) >= _mouseAliveRadius) SetActive(false);
        _mouseDirection = (position - _regionSpawnLocation).normalized;
        // -> Set targets
        _mouseNudge = _mouseDirection * _mouseNudgeStrength;
        _targetPosition = _regionSpawnLocation + _mouseNudge;
    }
    #endregion

    #region Debug
    private void OnDrawGizmos()
    {
        if (!_parent.gameObject.activeSelf) return;

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(_regionSpawnLocation, _mouseAliveRadius);

        Gizmos.color = Color.yellow;
        float screenLineX = Screen.width * _flipPercentage;
        Gizmos.DrawLine(new Vector2(screenLineX, 0), new Vector2(screenLineX, Screen.height));
    }
    #endregion
}
