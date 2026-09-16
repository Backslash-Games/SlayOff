using HFHandyUtils.Data.Spawning;
using HFHandyUtils.Graphical;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;
using VInspector.Libs;

public class SpawnableObject : EntityData
{
    [Header("Spawnable Object")]
    [SerializeField] private SpawnableObject_Scriptable _data;


    [Header("Attributes")]
    public string childObject = string.Empty;
    [SerializeField] private List<SpawnableObject_Attribute> _attributes;
    [Space]
    [SerializeField] private Dictionary<string, object> _attributeVariables = new Dictionary<string, object>();

    public Vector3 spawnPosition = Vector3.zero;
    public GameObject attributeTarget = null;
    public int spawnDepth = 0;
    private static readonly int s_MaxSpawnDepth = 5;


    [Header("Graphical")]
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private Billboard _billboard;


    private bool _customPhysicsEnabled = false;
    [Header("Physics")]
    public float size = 1;
    private static readonly Vector2 s_SizeRange = new Vector2(0.125f, 100f);
    public LayerMask layerMask = 0;
    /// <summary>
    ///     Last frame's linear velocity
    /// </summary>
    private Vector3 l_linearVelocity = Vector3.zero;

    private static readonly Vector2 s_RadiusScale = new Vector2(1.125f, 5f);
    private static readonly float s_RadiusScaleMaxSpeed = 100;

    [Header("Time")]
    public float aliveTime = 0;
    private static readonly float s_MaximumDistance = 1000;
    public enum LifetimeType { Time, Impacts, DirectImpact };
    private Cooldown _lifetimeTimer;
    private int _lifetimeImpacts = 0;

    #region Spawn Information - Static
    /// <summary>
    ///     Spawns a new SpawnableObject
    /// </summary>
    /// <param name="scriptable">Object data</param>
    /// <param name="target">Target spawn information</param>
    /// <param name="offset">Spawn offset</param>
    /// <param name="parentPhysics">Physics of parent object</param>
    /// <param name="initialVelocity">Initial spawn velocity</param>
    /// <param name="depth">Spawn depth, used for recursive spawns</param>
    public static SpawnableObject Spawn(SpawnableObject_Scriptable scriptable, Transform target, Vector3 offset, Rigidbody parentPhysics, Vector3 initialVelocity, int depth = 0)
    {
        if (target == null) return null;
        if (depth >= s_MaxSpawnDepth) return null;
        Vector3 position = target.transform.position;

        SpawnableObject spawned = ObjectPooler.Instance.Spawn("Spawnable Object", Vector3.zero, Vector3.zero, true).GetComponent<SpawnableObject>();
        Rigidbody rb = spawned.GetRigidbody();
        // -> Set Transform
        rb.position = spawned.transform.position = position + (target.transform.rotation * offset);
        rb.rotation = spawned.transform.rotation = target.transform.rotation;
        if(parentPhysics != null) rb.linearVelocity = parentPhysics.linearVelocity * Mathf.Clamp01(Vector3.Dot(parentPhysics.linearVelocity, target.forward)); // Needs some more work here
        // -> Set Data
        spawned.SetData(scriptable);
        spawned.spawnDepth = depth + 1;

        spawned.GetRigidbody().AddForce(target.transform.rotation * initialVelocity, ForceMode.Impulse);
        return spawned;
    }

    /// <summary>
    ///     Default folder that spawnable objects are stored in Assets/Resources
    /// </summary>
    private static string s_DefaultResourcePath = "Spawnable Objects";
    /// <summary>
    ///     Returns the file name with the default resource path
    /// </summary>
    /// <param name="file">File name</param>
    /// <returns>Resource folder path</returns>
    public static string GetResourcePath(string file) { return s_DefaultResourcePath + "/" + file; }
    #endregion

    #region Unity Methods
    protected override void OnEnable()
    {
        ResetAll();

        base.OnEnable();
        if (_data != null) SetData(_data);
    }
    protected virtual void Update()
    {
        // Update time alive
        UpdateAliveTime();
        // Update impact idle correction
        Impact_IdleCorrectionUpdate();
    }
    protected virtual void FixedUpdate()
    {
        // Run custom physics
        TickCustomPhysics();
        // Run attribute physics
        RunAttributes_FixedUpdate();
    }
    private void LateUpdate()
    {
        // Update the linear velocity
        l_linearVelocity = GetRigidbody().linearVelocity;
    }
    private void OnCollisionEnter(Collision collision)
    {
        OnImpact();
    }
    #endregion
    #region Data Initialization
    /// <summary>
    ///     Sets the spawnable object data and initializes
    /// </summary>
    /// <param name="data">New data</param>
    public void SetData(SpawnableObject_Scriptable data)
    {
        // Set the object data
        _data = data;

        // Initialize information
        Init_Graphical();
        Init_Stats();
        Init_Physics();
        Init_Lifetime();
        Init_Size();

        // Set attributes
        childObject = _data.childObject;
        AddAttributes(data.attributes);
    }
    /// <summary>
    ///     Initializes sprite renderer information
    /// </summary>
    private void Init_Graphical()
    {
        // Error check
        if (_renderer == null) return;
        // Set values
        _renderer.sprite = _data.sprite;
        _renderer.color = _data.color;
    }
    /// <summary>
    ///     Initializes stat information
    /// </summary>
    private void Init_Stats()
    {
        GetStatblock().ApplyModifer(_data.modifiers);
    }
    /// <summary>
    ///     Initializes physics information
    /// </summary>
    private void Init_Physics()
    {
        // Pull body
        Rigidbody body = GetRigidbody();
        Collider collider = GetCollision();
        // Error check
        if (body == null || collider == null) return;
        // Set position
        spawnPosition = body.position;
        // Set values
        body.useGravity = false;
        body.linearDamping = _data.linearDamping;

        // Allow gravity
        _customPhysicsEnabled = true;
    }
    /// <summary>
    ///     Initializes lifetime
    /// </summary>
    private void Init_Lifetime()
    {
        _lifetimeTimer = new Cooldown(this, 0, 0);
        _lifetimeTimer.OnCooldownEnded += Break;


        // Initializes timer lifetime
        if (_data.lifetimeType.Equals(LifetimeType.Time))
            // Set rates
            _lifetimeTimer.SetRates(_data.lifetime, 1);

        else if (isLifetimeImpact())
            // Set rates
            _lifetimeTimer.SetRates(s_ImpactMaxIdleTime, 1);


        _lifetimeTimer.Start();
    }
    /// <summary>
    ///     Initializes size
    /// </summary>
    private void Init_Size()
    {
        // Set layer mask
        layerMask = _data.layerMask;
        SetSize(statblock.GetSize());
    }
    #endregion
    #region Clean Up
    /// <summary>
    ///     Breaks the spawnable object
    /// </summary>
    public virtual void Break()
    {
        RunAttributes_Break();

        ResetAll();
        gameObject.SetActive(false);
    }
    /// <summary>
    ///     Resets information associated with the spawnable object
    /// </summary>
    private void ResetAll()
    {
        // General data
        _data = null;
        // Attribute information
        childObject = string.Empty;
        _attributes = new List<SpawnableObject_Attribute>();
        attributeTarget = null;
        spawnDepth = 0;
        // Renderer information
        _renderer.sprite = null;
        _renderer.color = Color.white;
        // Phsyics information
        _customPhysicsEnabled = false;
        // Lifetime information
        _lifetimeTimer?.ResetTimer();
        _lifetimeImpacts = 0;
    }
    #endregion

    #region Attributes
    #region Addition
    /// <summary>
    ///     Adds an attribute to the list and runs initialization
    /// </summary>
    /// <param name="attribute">New attribute</param>
    public void AddAttribute(SpawnableObject_Attribute attribute)
    {
        // Add to total list
        _attributes.Add(attribute);
        // Initialize object
        SpawnableObject_Attribute cAttribute = _attributes[_attributes.Count - 1];
        cAttribute.OnInitialization(this);
    }
    /// <summary>
    ///     Adds an attribute to the list and runs initialization
    /// </summary>
    /// <param name="attribute">New attribute</param>
    public void AddAttribute(SpawnableObject_Attribute.Tag tag)
    {
        // Add to total list
        _attributes.Add(new SpawnableObject_Attribute(new List<SpawnableObject_Attribute.Tag>() { tag }));
        // Initialize object
        SpawnableObject_Attribute cAttribute = _attributes[_attributes.Count - 1];
        cAttribute.OnInitialization(this);
    }


    /// <summary>
    ///     Adds multiple attributes to the list and runs initialization
    /// </summary>
    /// <param name="attribute">New attribute</param>
    public void AddAttributes(List<SpawnableObject_Attribute> attributes)
    {
        foreach (var item in attributes) AddAttribute(item);
    }
    /// <summary>
    ///     Adds multiple attributes to the list and runs initialization
    /// </summary>
    /// <param name="attribute">New attribute</param>
    public void AddAttributes(SpawnableObject_Attribute[] attributes)
    {
        foreach (var item in attributes) AddAttribute(item);
    }
    #endregion
    #region Events
    /// <summary>
    ///     Runs attribute on fixed update
    /// </summary>
    private void RunAttributes_FixedUpdate()
    {
        foreach (var item in _attributes)
            item.OnFixedUpdate(this);
    }
    /// <summary>
    ///     Runs attribute on break
    /// </summary>
    private void RunAttributes_Impact()
    {
        foreach (var item in _attributes)
            item.OnImpact(this);
    }
    /// <summary>
    ///     Runs attribute on break
    /// </summary>
    private void RunAttributes_ImpactEntity(EntityData entity)
    {
        foreach (var item in _attributes)
            item.OnImpactEntity(this, entity);
    }
    /// <summary>
    ///     Runs attribute on break
    /// </summary>
    private void RunAttributes_Break()
    {
        foreach (var item in _attributes)
            item.OnBreak(this);
    }
    #endregion
    #region Variable Handling
    /// <summary>
    ///     Properly adds an attribute to the dictionary
    /// </summary>
    /// <param name="key">Key</param>
    /// <param name="value">Value</param>
    public void AddAttributeVariable(string key, object value)
    {
        if (_attributeVariables.ContainsKey(key)) return;
        _attributeVariables.Add(key, value);
    }

    /// <summary>
    ///     Properly reads a varaible from the attribute variable list
    /// </summary>
    /// <param name="key">Key</param>
    /// <returns>Value</returns>
    public object ReadAttributeVariable(string key)
    {
        if (!_attributeVariables.ContainsKey(key)) return null;
        return _attributeVariables[key];
    }

    /// <summary>
    ///     Properly writes a varaible from the attribute variable list
    /// </summary>
    /// <param name="key">Key</param>
    /// <param name="value">Value</param>
    public void WriteAttributeVariable(string key, object value)
    {
        if (!_attributeVariables.ContainsKey(key))
        {
            AddAttributeVariable(key, value);
            return;
        }
        _attributeVariables[key] = value;
    }
    #endregion
    #endregion

    #region Physics
    /// <summary>
    ///     Updates custom physics
    /// </summary>
    private void TickCustomPhysics()
    {
        if (!_customPhysicsEnabled) return;
        // Update order
        ScaledGravity();
    }
    /// <summary>
    ///     Runs scaled gravity
    /// </summary>
    private void ScaledGravity()
    {
        Rigidbody body = GetRigidbody();
        Vector3 gravity = GetGravity();
        body.AddForce(gravity, ForceMode.Acceleration);
    }
    /// <summary>
    ///     Gets custom gravity from spawnable object
    /// </summary>
    /// <returns>Custom gravity</returns>
    public Vector3 GetGravity()
    {
        return Physics.gravity * statblock.GetWeight();
    }
    #endregion
    #region Time
    /// <summary>
    ///     Updates the total time alive
    /// </summary>
    public void UpdateAliveTime() { aliveTime += Time.deltaTime; }
    /// <summary>
    ///     Updates the alive distance
    /// </summary>
    public void UpdateAliveDistance() 
    {
        if (Vector3.Distance(transform.position, Camera.main.transform.position) >= s_MaximumDistance) Break();
    }
    #endregion
    #region Size
    /// <summary>
    ///     Sets the scale of the object
    /// </summary>
    /// <param name="scale">New scale</param>
    public void SetSize(float size)
    {
        // Store size
        this.size = Mathf.Clamp(size, s_SizeRange.x, s_SizeRange.y);
        // Set scale
        transform.localScale = Vector3.one * this.size;
    }
    public float GetHitboxScale() { return Mathf.Lerp(s_RadiusScale.x, s_RadiusScale.y, GetLinearVelocity().magnitude / s_RadiusScaleMaxSpeed); }
    #endregion
    #region Impact
    float _lastImpactResetHeight = 0;
    private static readonly float s_ImpactResetDistance = 0.25f;
    private static readonly float s_ImpactMaxIdleTime = 5;
    /// <summary>
    ///     Updates impact function
    /// </summary>
    private void Impact_IdleCorrectionUpdate()
    {
        if (_data == null || !isLifetimeImpact()) return;

        // Check if we need to reset the timer
        // -> Reset timer condition
        if(Mathf.Abs(_lastImpactResetHeight - transform.position.y) >= s_ImpactResetDistance)
        {
            _lifetimeTimer?.ResetTimer();
            _lastImpactResetHeight = transform.position.y;
        }
    }
    /// <summary>
    ///     Runs logic when the object impacts a surface
    /// </summary>
    private void OnImpact()
    {
        // Check impact
        _lifetimeImpacts++;
        _lifetimeTimer?.ResetTimer();
        // Run attributes
        RunAttributes_Impact();

        // Check for an entitiy impact
        bool entityHit = false;
        Collider[] colliding = Physics.OverlapSphere(transform.position, size + 0.05f, layerMask);
        foreach (var item in colliding)
        {
            EntityData entity = item.GetComponent<EntityData>();
            if (entity != null) 
            {
                entityHit = true;
                RunAttributes_ImpactEntity(entity);
            }
        }

        // -> Check for a break
        if (isLifetimeImpact() && _lifetimeImpacts >= _data.lifetime) Break();
        else if (entityHit && _data.lifetimeType.Equals(LifetimeType.DirectImpact)) Break();
    }
    private bool isLifetimeImpact() { return _data.lifetimeType.Equals(LifetimeType.Impacts) || _data.lifetimeType.Equals(LifetimeType.DirectImpact); }


    /// <summary>
    ///     Gets a specific impact point using the current velocity
    /// </summary>
    /// <param name="spawnable">Spawnable object</param>
    /// <param name="hit">Raycast hit</param>
    public bool GetImpactPoint_Velocity(out RaycastHit hit, float scale = 1)
    {
        Vector3 direction = l_linearVelocity;
        Ray ray = new Ray(transform.position, direction);
        return Physics.Raycast(ray, out hit, direction.magnitude * scale, layerMask);
    }
    #endregion
}
