using UnityEngine;

[System.Serializable]
public class EffectLibraryEntry<TKey, TValue>
    where TKey : System.Enum
    where TValue : class
{
    [SerializeField] private TKey state;
    [SerializeField] private TValue content;
    [Space]
    [SerializeField] private string eventKey;

    #region Constructor
    public EffectLibraryEntry(TKey key, TValue value, string eventKey)
    {
        state = key;
        content = value;
        this.eventKey = eventKey;
    }
    public EffectLibraryEntry(TKey key, TValue value) : this(key, value, "") { }
    #endregion

    #region Get Methods
    public TKey GetState() { return state; }
    public TValue GetContent() { return content; }
    public string GetEventKey() { return eventKey; }
    #endregion
    #region String Handling
    public override string ToString()
    {
        return $"{state} : {content} -> {eventKey}";
    }
    #endregion
}
