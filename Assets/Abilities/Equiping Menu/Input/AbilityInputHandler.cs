using HFHandyUtils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Utilities;

public class AbilityInputHandler : MonoBehaviour, IPointerClickHandler
{
    private static readonly string s_AbilityPrefix = "Ability_";
    private static readonly string s_ActionMapId = "Ability Actions";

    [SerializeField] private bool buildOnAwake = true;
    [SerializeField] private bool allowClickTrigger = false;
    private InputActionMap actionMap = new InputActionMap(s_ActionMapId);
    [Space]
    [SerializeField] private Transform as_Parent = null;
    [SerializeField] private GameObject as_Prefab = null;
    [Space]
    [SerializeField] public string[] abilityKeys = new string[0];
    [SerializeField] public AbilitySlot[] abilitySlots = new AbilitySlot[0];
    [SerializeField] public Vector3[] abilityKeyPositions = new Vector3[0];
    [SerializeField] public Transform[] abilityKeyParents = new Transform[0];
    [SerializeField] public Color[] abilityKeyColors = new Color[0];
    [SerializeField] public AbilitySlot.SlotType[] abilitySlotTypes = new AbilitySlot.SlotType[0];
    [Space]
    [SerializeField] public AbilitySlotData[] abilitySlotData = new AbilitySlotData[0];

    public struct AbilitySlotData
    {
        public string key;
        public AbilitySlot slot;
        public AbilitySlot.SlotType type;
        [Space]
        public Vector3 position;
        public Transform parent;

        public AbilitySlotData(string key, AbilitySlot slot, AbilitySlot.SlotType type, Vector3 position, Transform parent)
        {
            this.key = key;
            this.slot = slot;
            this.type = type;

            this.position = position;
            this.parent = parent;
        }
    }

    #region Unity Methods
    private void Awake()
    {
        // Check if we need to build
        if (!buildOnAwake)
        {
            BuildAbilityKeys();
        }

        BindAbilityKeys();
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
        abilityKeys = new string[keys.Count];
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

            // Build the key
            AbilitySlot cSlot = Instantiate(as_Prefab, as_Parent).GetComponent<AbilitySlot>();
            cSlot.SetName(displayName, GetAbilityName(index));
            // -> Check for a logged position
            if(index >= 0 && index < abilityKeyPositions.Length) cSlot.transform.position = abilityKeyPositions[index];
            if (index >= 0 && index < abilityKeyParents.Length) cSlot.transform.parent = abilityKeyParents[index];
            if (index >= 0 && index < abilityKeyColors.Length) cSlot.SetColor(abilityKeyColors[index]);

            // Add to serializable list
            abilityKeys[index] = key.path;
            abilitySlots[index] = cSlot;

            index++;
        }
        InputSystem.actions.AddActionMap(actionMap);
    }
    public void BuildAdjacentConnections()
    {
        foreach (AbilitySlot slot in abilitySlots)
        {
            slot.SetupAdjacents();
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
        foreach(string key in abilityKeys)
        {
            if (id.Equals(key) && abilitySlots[index].enabled)
            {
                abilitySlots[index].OnTriggerSlot(new AbilityTrace("Key Pressed"));
                AbilityInformationHandler.Instance.executionLine.ResetInformation();
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
        foreach (string key in abilityKeys)
        {
            if (id.Equals(key)) abilitySlots[index].LockSlot();
            index++;
        }
    }

    /// <summary>
    ///     Records the current position of keys
    /// </summary>
    public void RecordKeyData()
    {
        abilityKeyPositions = new Vector3[abilitySlots.Length];
        abilityKeyParents = new Transform[abilitySlots.Length];
        abilityKeyColors = new Color[abilitySlots.Length];

        abilitySlotData = new AbilitySlotData[abilitySlots.Length];

        for (int i = 0; i < abilitySlots.Length; i++)
        {
            abilityKeyPositions[i] = abilitySlots[i].transform.position;
            abilityKeyParents[i] = abilitySlots[i].transform.parent;
            abilityKeyColors[i] = abilitySlots[i].color;
            abilitySlots[i].SetColor(abilitySlots[i].color);

            abilitySlotData[i] = new AbilitySlotData();
        }
    }
    #endregion
    #region Click Methods
    public void OnPointerClick(PointerEventData eventData)
    {
        List<RaycastResult> results = new List<RaycastResult>();

        // -> Get all keys
        EventSystem.current.RaycastAll(eventData, results);
        if (allowClickTrigger)
            foreach (RaycastResult result in results) 
            {
                // Check if the result is an ability slot
                AbilitySlot slot = result.gameObject.GetComponent<AbilitySlot>();
                if (slot != null && slot.PointInClickRegion(eventData.position)) slot.OnTriggerSlot(new AbilityTrace("Mouse Clicked"));
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
        abilityKeys = new string[0];

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
