using HFHandyUtils.Physics;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.Rendering.Universal;
using NUnit.Framework;

public class SpawnableEffect : EntityData
{
    private bool _initialized = false;
    public enum TriggerType { Manual, Enable, LateUpdate };

    [Header("Spawnable Effect")]
    public TriggerType trigger = TriggerType.Enable;
    public Hitbox hitbox = null;
    public float size = 1;

    [SerializeField] private VisualEffect _visualEffect = null;
    [SerializeField] private string _visualEffectEventName = "OnTrigger";
    [Space]
    [SerializeField] private DecalProjector _decalProjector = null;
    [SerializeField] private Texture _decal = null;
    [Space]
    [SerializeField] private float _effectLength = 1.4f;

    private Cooldown _cooldown;

    private SpawnableObject source = null;
    private LayerMask layerMask = 0;

    private static readonly float s_Depth = 0.5f;

    [Header("Spawnable Effect - Entity Tracking")]
    [SerializeField] private List<EntityData> _trackedEntities = new List<EntityData>();

    protected delegate void OnEntityTracked(EntityData entitiy);
    protected event OnEntityTracked OnEntityTracking_Added;
    protected event OnEntityTracked OnEntityTracking_Removed;

    #region Unity Events
    protected override void OnEnable()
    {
        base.OnEnable();
        if (trigger.Equals(TriggerType.Enable)) OnTrigger(source, layerMask, size);
    }

    protected virtual void LateUpdate()
    {
        if (trigger.Equals(TriggerType.LateUpdate)) OnTrigger(source, layerMask, size);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        ResetAll();
    }
    #endregion
    #region Spawnable Effect
    /// <summary>
    ///     Initialize the spawnable effect
    /// </summary>
    public void Initialize(SpawnableObject source, LayerMask layerMask, float size)
    {
        // Handle flag
        _initialized = true;

        this.source = source;
        this.layerMask = layerMask;

        // Establish break cooldown
        _cooldown = new Cooldown(this, _effectLength, 1);
        _cooldown.OnCooldownEnded += () => gameObject.SetActive(false);

        // Handle decal projector
        if (_decalProjector == null) _decalProjector = GetComponentInChildren<DecalProjector>();
        if (_decalProjector != null)
        {
            _decalProjector.size = new Vector3(1, 1, 0) * size * 2;
            _decalProjector.size += Vector3.forward * s_Depth;
            if (_decal != null) _decalProjector.material.SetTexture("Base_Map", _decal);
        }
        // Set size
        SetSize(size);

        // Run when initialized
        OnInitialize();
    }
    /// <summary>
    ///     Method run when initialized
    /// </summary>
    protected virtual void OnInitialize() { }
    private void ResetAll()
    {
        _initialized = false;
    }

    /// <summary>
    ///     Handles spawnable effect trigger function
    /// </summary>
    public virtual void OnTrigger(SpawnableObject source, LayerMask layerMask, float size)
    {
        // Check if we have initialized
        if (!_initialized) Initialize(source, layerMask, size);

        // Ensure the effect is initialized
        _cooldown.Start();
        hitbox.SetLayerMask(layerMask);

        // Hit entities inside the explosion hitbox
        List<EntityData> entities = new List<EntityData>();
        hitbox.GetColliding(out Collider[] colliders);
        foreach (var item in colliders)
        {
            EntityData entity = item.GetComponent<EntityData>();
            if (entity != null && entity != source)
            {
                entities.Add(entity);
                HitEntity(entity, source.GetStatblock());
            }
        }

        // Update tracking
        UpdateEntityTracking(entities);
    }
    public virtual void OnSetHitbox(float size)
    {
        hitbox = new Hitbox_Sphere(transform) { radius = size };
    }
    /// <summary>
    ///     Applies damage to an entitiy
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="stats"></param>
    public virtual void HitEntity(EntityData entity, Statblock stats) { }
    #endregion

    #region Get Methods
    public DecalProjector GetDecalProjector() { return _decalProjector; }
    #endregion
    #region Set Methods
    public void SetSize(float size)
    {
        this.size = size;
        // Handle explosion effect values
        if (_visualEffect == null) _visualEffect = GetComponentInChildren<VisualEffect>();
        if (_visualEffect != null)
        {
            if (_visualEffect.HasFloat("radius")) _visualEffect.SetFloat("radius", size);
            _visualEffect.SendEvent(_visualEffectEventName);
        }

        // Set up the hitbox
        OnSetHitbox(size);
    }
    #endregion

    #region Entity Tracking
    private void AddEntityTracking(EntityData entity)
    {
        // Check if we are adding
        if(_trackedEntities.Contains(entity)) return;

        _trackedEntities.Add(entity);
        OnEntityTracking_Added?.Invoke(entity);
    }
    private void RemovesEntityTracking(EntityData entity)
    {
        // Check if we are removing
        if (!_trackedEntities.Contains(entity)) return;

        _trackedEntities.Remove(entity);
        OnEntityTracking_Removed?.Invoke(entity);
    }


    /// <summary>
    ///     Goes through a list of entities and updates the list based on entries
    /// </summary>
    /// <param name="entities">Compared entities</param>
    private void UpdateEntityTracking(List<EntityData> entities)
    {
        // Roll through each entity and check if they need to be added or removed from tracking
        // -> Check remove
        for (int i = 0; i < _trackedEntities.Count; i++)
        {
            EntityData entity = _trackedEntities[i];
            if (!entities.Contains(entity)) RemovesEntityTracking(entity);
        }
        // -> Check add
        for (int i = 0; i < entities.Count; i++)
        {
            EntityData entity = entities[i];
            if (!_trackedEntities.Contains(entity)) AddEntityTracking(entity);
        }
    }
    /// <summary>
    ///     Removes all entities while triggering events
    /// </summary>
    private void ResetEntityTracking()
    {
        while (_trackedEntities.Count > 0)
            RemovesEntityTracking(_trackedEntities[0]);
    }
    #endregion

    #region Debug
    protected virtual void OnDrawGizmos()
    {
        hitbox?.DrawGizmos();
    }
    #endregion
}
