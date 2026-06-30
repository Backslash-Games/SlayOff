using HFHandyUtils;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Utilities;

public class AbilityInputHandler : MonoBehaviour, IPointerClickHandler
{
    #region Singleton
    // Singleton
    private static AbilityInputHandler _instance;
    public static AbilityInputHandler Instance { get { return _instance; } }
    private void CreateSingleton()
    {
        // -> Pulled from Out on the Red Sea
        // Checks if the instance of object is first of its type
        // If object is not unique, destroy current instance
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        // Declares this script as current
        else
        {
            _instance = this;
        }
    }
    #endregion

    private static readonly string s_AbilityPrefix = "Ability_";
    private static readonly string s_ActionMapId = "Ability Actions";
    private static readonly string s_MouseActionMapId = "UI";

    [SerializeField] private bool buildOnAwake = true;
    [SerializeField] private bool allowClickTrigger = false;
    private InputActionMap actionMap = new InputActionMap(s_ActionMapId);
    [Space]
    [SerializeField] private Transform as_Parent = null;
    [SerializeField] private GameObject as_Prefab = null;
    [Space]
    [SerializeField] public AbilitySlot[] abilitySlots = new AbilitySlot[0];
    [SerializeField] public AbilitySlotData[] abilitySlotData = new AbilitySlotData[0];
    [Space]
    [SerializeField] private Vector2 mousePosition = Vector2.zero;
    public PointerEventData pointerEventData = null;
    [Space]
    public AbilitySlotPickupDummy pickupDummy;

    [System.Serializable]
    public struct AbilitySlotData
    {
        public string key;
        public AbilitySlot.SlotType type;
        [Space]
        public Vector3 position;
        public Transform parent;

        public AbilitySlotData(string key, AbilitySlot.SlotType type, Vector3 position, Transform parent)
        {
            this.key = key;
            this.type = type;

            this.position = position;
            this.parent = parent;
        }
    }

    #region Unity Methods
    private void Awake()
    {
        CreateSingleton();
        // Check if we need to build
        if (!buildOnAwake)
        {
            BuildAbilityKeys();
        }

        BindAbilityKeys();
        BindInput_Mouse();
    }
    #endregion
    #region Mouse Binding
    public delegate void OnMouseInput(Vector2 position);
    public event OnMouseInput OnPointerMoved;
    public event OnMouseInput OnRightClick;
    public event OnMouseInput OnLeftClick;

    private void BindInput_Mouse()
    {
        // Method variables
        pointerEventData = new PointerEventData(EventSystem.current);
        InputActionMap mouseMap = InputSystem.actions.FindActionMap(s_MouseActionMapId);

        // -> Movement
        InputAction point = mouseMap.FindAction("Point");
        point.performed += context => { mousePosition = context.ReadValue<Vector2>(); pointerEventData.position = mousePosition; };
        point.performed += _ => OnPointerMoved?.Invoke(mousePosition);

        // -> Right click
        InputAction rightClick = mouseMap.FindAction("RightClick");
        rightClick.performed += _ => OnRightClick?.Invoke(mousePosition);

        // -> Right click
        InputAction leftClick = mouseMap.FindAction("LeftClick");
        leftClick.performed += _ => OnLeftClick?.Invoke(mousePosition);
        leftClick.canceled += _ => { pickupDummy.ForceDrop(pointerEventData); };
    }
    #endregion

    #region Key Methods
    /// <summary>
    ///     Builds individual abilities for the keyboard
    /// </summary>
    public void BuildAbilityKeys()
    {
        // Track and set up abilities
        int index = 0;
        ReadOnlyArray<KeyControl> keys = Keyboard.current.allKeys;

        // Set up arrays
        abilitySlots = new AbilitySlot[keys.Count];

        // Check if our action map already exists
        RemoveActionMap();


        // Build based on keys
        actionMap.Disable();
        InputSystem.actions.Disable();
        foreach (KeyControl key in keys)
        {
            // Build the action
            string displayName = key.displayName;
            InputAction action = actionMap.AddAction(GetAbilityName(index));
            action.AddBinding(key);

            // Build the key'
            #if UNITY_EDITOR
            AbilitySlot cSlot = ((GameObject)PrefabUtility.InstantiatePrefab(as_Prefab, as_Parent)).GetComponent<AbilitySlot>();
            #else
            AbilitySlot cSlot = Instantiate(as_Prefab, as_Parent).GetComponent<AbilitySlot>();
            #endif
            cSlot.SetName(displayName, GetAbilityName(index));
            // -> Check for a logged position
            if(index >= 0 && index < abilitySlotData.Length) cSlot.transform.position = abilitySlotData[index].position;
            if (index >= 0 && index < abilitySlotData.Length) cSlot.transform.parent = abilitySlotData[index].parent;

            // Add to serializable list
            abilitySlots[index] = cSlot;
            abilitySlots[index].path = key.path;

            index++;
        }
        InputSystem.actions.AddActionMap(actionMap);
    }
    public void BuildAdjacentConnections()
    {
        foreach (AbilitySlot slot in abilitySlots)
        {
            slot.SetupAdjacents();
#if UNITY_EDITOR
            PrefabUtility.RecordPrefabInstancePropertyModifications(slot);
#endif
        }
    }

    /// <summary>
    ///     Binds abilities to keyboard
    /// </summary>
    public void BindAbilityKeys()
    {
        // Track and set up abilities
        int index = 0;
        ReadOnlyArray<KeyControl> keys = Keyboard.current.allKeys;
        actionMap = InputSystem.actions.FindActionMap(s_ActionMapId);

        // Get locked keys
        List<string> boundKeys = new List<string>();
        ReadOnlyArray<InputActionMap> boundMaps = InputSystem.actions.actionMaps;
        foreach(InputActionMap map in boundMaps)
        {
            // Check for our ability map
            if (map.name.Equals(s_ActionMapId)) continue;
            // Pull all keys
            ReadOnlyArray<InputBinding> bindings = map.bindings;
            foreach(InputBinding binding in bindings)
            {
                if (!binding.isComposite) 
                {
                    string path = binding.effectivePath;
                    if (path.Contains("<Keyboard>"))
                    {
                        string finalPath = path.Replace("<Keyboard>", "/Keyboard");
                        boundKeys.Add(finalPath);
                        HFLogger.Log(finalPath);
                    }
                }
            }
        }

        // Build based on keys
        actionMap.Disable();
        foreach (KeyControl key in keys)
        {
            // Check if the slot is bound
            //HFLogger.Log(boundKeys.Contains(key.path) + " - " + key.path);
            if (boundKeys.Contains(key.path))
            {
                LockAbilityKey(key.path);
                index++;
                continue;
            }

            // Build the action
            string displayName = key.displayName;
            InputAction action = actionMap.FindAction(GetAbilityName(index));
            action.started += _ => ActivateAbilityKey(key.path);

            index++;
        }
        InputSystem.actions.Enable();
        actionMap.Enable();
    }
    /// <summary>
    ///     Activates an ability
    /// </summary>
    /// <param name="identifier">Ability identifier</param>
    private void ActivateAbilityKey(string id)
    {
        // Find ability
        int index = 0;
        foreach(AbilitySlotData keyData in abilitySlotData)
        {
            if (id.Equals(keyData.key) && abilitySlots[index].enabled)
            {
                AbilityTrace trace = new AbilityTrace("Key Pressed", abilitySlots[index]);
                abilitySlots[index].OnTriggerSlot(trace);

                AbilityInformationHandler.Instance.executionLine.ResetInformation();
                AbilityInformationHandler.Instance.executionTree.Build(trace);
            }
            index++;
        }
    }
    /// <summary>
    ///     Locks an ability
    /// </summary>
    /// <param name="identifier">Ability identifier</param>
    private void LockAbilityKey(string id)
    {
        HFLogger.Log("Attempting to lock slot with id " + id);

        // Find ability
        int index = 0;
        foreach (AbilitySlotData keyData in abilitySlotData)
        {
            if (id.Equals(keyData.key)) abilitySlots[index].LockSlot();
            index++;
        }
    }

    /// <summary>
    ///     Records the current position of keys
    /// </summary>
    public void RecordKeyData()
    {
        abilitySlotData = new AbilitySlotData[abilitySlots.Length];

        for (int i = 0; i < abilitySlotData.Length; i++)
        {
            AbilitySlot slot = abilitySlots[i];
            abilitySlotData[i] = new AbilitySlotData()
            {
                key = slot.path,
                parent = slot.transform.parent,
                position = slot.transform.position
            };
        }
    }
#endregion
    #region Click Methods
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!allowClickTrigger) return;
        List<RaycastResult> results = new List<RaycastResult>();

        // -> Get all keys
        EventSystem.current.RaycastAll(eventData, results);
        foreach (RaycastResult result in results)
        {
            // Check if the result is an ability slot
            AbilitySlot slot = result.gameObject.GetComponent<AbilitySlot>();
            if (slot != null && slot.PointInClickRegion(eventData.position))
            {
                AbilityTrace trace = new AbilityTrace("Mouse Clicked", slot);
                slot.OnTriggerSlot(trace);
            }
        }
    }
    #endregion

    #region Get Methods
    private string GetAbilityName(int index) { return s_AbilityPrefix + index; }
    #endregion
    #region Reset
    /// <summary>
    ///     Resets the input handler
    /// </summary>
    public void ResetAll()
    {
        BreakAbilitySlots();
        RemoveActionMap();
    }
    /// <summary>
    ///     Breaks ability slots
    /// </summary>
    private void BreakAbilitySlots()
    {
        foreach(AbilitySlot slot in abilitySlots)
        {
#if UNITY_EDITOR
            DestroyImmediate(slot.gameObject);
#else
            Destroy(slot.gameObject);
#endif
        }
        abilitySlots = new AbilitySlot[0];
        actionMap = new InputActionMap(s_ActionMapId);
    }
    /// <summary>
    ///     Removes action map from global input
    /// </summary>
    private void RemoveActionMap()
    {
        // Remove action map
        InputActionMap map = InputSystem.actions.FindActionMap(s_ActionMapId);
        if (map != null)
        {
            map.Disable();
            InputSystem.actions.Disable();
            InputSystem.actions.RemoveActionMap(s_ActionMapId);
            InputSystem.actions.Enable();
        }
    }
    #endregion
}
