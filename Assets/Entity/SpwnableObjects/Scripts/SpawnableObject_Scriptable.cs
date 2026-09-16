using UnityEngine;

[CreateAssetMenu(fileName = "New Spawnable Object", menuName = "SlayOff/Content/Spawnable Object")]
public class SpawnableObject_Scriptable : ScriptableObject
{
    [Header("Graphical")]
    public Sprite sprite;
    public Color color = Color.white;

    [Header("Modifiers")]
    public string childObject = string.Empty;
    public SpawnableObject_Attribute[] attributes;
    public StatBlockModifier modifiers = new StatBlockModifier();

    [Header("Physics")]
    public LayerMask layerMask = 832;
    public float linearDamping = 0;

    [Header("Lifetime")]
    public SpawnableObject.LifetimeType lifetimeType = SpawnableObject.LifetimeType.Time;
    public float lifetime = 1;
}
