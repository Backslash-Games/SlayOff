using UnityEngine;

namespace HFHandyUtils.Effects
{
    /// <summary>
    ///     Collection of items that can be played as an effect
    ///     <br></br>
    ///     <br>Luke Wittbrodt :: lwittbrodt87@gmail.com :: halfhand870</br>
    /// </summary>
    /// <typeparam name="TState">Enum State</typeparam>
    /// <typeparam name="TContent">Class Content</typeparam>
    /// <typeparam name="TParams">Parameters</typeparam>
    [System.Serializable]
    public class EffectLibrary<TState, TContent, TParams>
        where TState : System.Enum
        where TContent : class
        where TParams : EffectComponent.Parameters
    {
        [SerializeField] private TParams parameters;
        [SerializeField] private EffectLibraryEntry<TState, TContent>[] entries;

        #region Get Methods - Entries
        /// <summary>
        ///     Compares the current content type with other type
        /// </summary>
        /// <param name="other">Other Type</param>
        /// <returns>True if the types are equal</returns>
        public bool CompareContentType(System.Type other) { return other.Equals(typeof(TContent)); }

        /// <summary>
        ///     Pull entry based on state
        /// </summary>
        /// <param name="state">Enum state</param>
        /// <returns>Content based on state</returns>
        public EffectLibraryEntry<TState, TContent> GetEntryFromState(TState state)
        {
            // Get audio clip of type
            foreach (EffectLibraryEntry<TState, TContent> value in entries)
                if (value.GetState().Equals(state))
                    return value;

            // If state cannot be found
            return null;
        }
        /// <summary>
        ///     Pull content from entires based on state
        /// </summary>
        /// <param name="state">Enum state</param>
        /// <returns>Content based on state</returns>
        public TContent GetContentFromState(TState state)
        {
            // Get audio clip of type
            foreach (EffectLibraryEntry<TState, TContent> value in entries)
                if (value.GetState().Equals(state))
                    return value.GetContent();

            // If state cannot be found
            return default(TContent);
        }
        #endregion
        #region Get Methods - Parameters
        public TParams GetParams() { return parameters; }
        #endregion
    }
}