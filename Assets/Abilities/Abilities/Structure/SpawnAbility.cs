using HFHandyUtils.Data.Spawning;
using UnityEngine;

public class SpawnAbility : Ability
{
    [Header("Spawnable Object")]
    [SerializeField] private GameObject _prefab;
    [SerializeField] private string _ability = "";
    private string _lastAbility = "";
    private SpawnableObject_Scriptable _cachedAbility = null;
    [Space]
    [SerializeField] private Vector3 _spawnOffset = Vector3.up * 1.5f;
    [Space]
    [SerializeField] private Vector3 _initialVelocityDirection;
    [SerializeField] private float _initialVelocityStrength;

    public override void OnTriggerAbility(AbilityTrace trace)
    {
        base.OnTriggerAbility(trace);
        // Spawn object at point
        Spawn_SpawnableObject(Camera.main.transform, GetPlayer().GetRigidbody());
    }

    public virtual void Spawn_SpawnableObject(Transform target, Rigidbody parentPhysics)
    {
        // -> Load resource
        if (_lastAbility != _ability || _cachedAbility == null) _cachedAbility = Resources.Load<SpawnableObject_Scriptable>(SpawnableObject.GetResourcePath(_ability));
        _lastAbility = _ability;
        if (_cachedAbility == null) return;

        // -> Spawn object
        SpawnableObject spawned = SpawnableObject.Spawn(_cachedAbility, target, _spawnOffset, parentPhysics, _initialVelocityDirection * _initialVelocityStrength);
        // -> Set object team
        spawned.ChangeTeam(GetPlayer().team);
        // -> Pass through stats
        Statblock p_Statblock = GetPlayer().statblock;
        StatBlockModifier playerModifer = new StatBlockModifier()
        {
            entries = new System.Collections.Generic.List<StatBlockModifier.Entry>
            {
                new StatBlockModifier.Entry(Stat.Tag.attackLower, StatBlockModifier.Attribute.basic, p_Statblock.GetValue(Stat.Tag.attackLower)),
                new StatBlockModifier.Entry(Stat.Tag.attackHigher, StatBlockModifier.Attribute.basic, p_Statblock.GetValue(Stat.Tag.attackHigher)),
                new StatBlockModifier.Entry(Stat.Tag.luck, StatBlockModifier.Attribute.basic, p_Statblock.GetValue(Stat.Tag.luck)),
                new StatBlockModifier.Entry(Stat.Tag.entropy, StatBlockModifier.Attribute.basic, p_Statblock.GetValue(Stat.Tag.entropy))
            }
        };
        spawned.statblock.ApplyModifer(playerModifer);
    }
}
