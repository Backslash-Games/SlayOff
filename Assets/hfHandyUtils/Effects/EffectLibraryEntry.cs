using UnityEngine;


namespace HFHandyUtils.Effects
{
    /// <summary>
    ///     Entry contained within the EffectLibrary
    ///     <br></br>
    ///     <br>Luke Wittbrodt :: lwittbrodt87@gmail.com :: halfhand870</br>
    /// </summary>
    /// <typeparam name="TKey">State Key</typeparam>
    /// <typeparam name="TValue">Value</typeparam>
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
}