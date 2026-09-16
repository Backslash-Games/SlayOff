using HFHandyUtils;
using System.Text.RegularExpressions;
using UnityEngine;

public class AbilityInformationHandler : MonoBehaviour
{
    #region Singleton
    // Singleton
    private static AbilityInformationHandler _instance;
    public static AbilityInformationHandler Instance { get { return _instance; } }
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

    /// <summary>
    ///     Ability information handler - Execution line reference
    /// </summary>
    public AIH_ExecutionLine executionLine;
    /// <summary>
    ///     Ability information handler - Execution tree reference
    /// </summary>
    public AIH_ExecutionTree executionTree;
    /// <summary>
    ///     Ability information popup
    /// </summary>
    [Space] public AbilityInformationPopup informationPopup;
    /// <summary>
    ///     A list of information formats, set in inspector
    /// </summary>
    [Space, Header("Formatting"), Tooltip("Use <var> to define the variable"), SerializeField] private InformationFormat[] _informationFormats;

    #region Information Format - Struct
    [System.Serializable]
    public class InformationFormat
    {
        public string target;

        public bool c_color = false; // Custom color
        public bool c_size = false; // Custom size

        public Color color = Color.white;
        public int size = -1;

        private string _defaultFormat = "$&";

        /// <summary>
        ///     Pulls the pattern associated with the target
        /// </summary>
        /// <returns>Pattern</returns>
        public string GetPattern()
        {
            return @"\(?\[?\{?<?\b(" + target + @"?)(?:|s|es|ed|ies)+('s)?\b>?\}?\]?\)?";
        }
        /// <summary>
        ///     Gets the format that replaces the target string
        /// </summary>
        /// <returns>Format</returns>
        public string GetFormat()
        {
            return GetFormat(color, size);
        }
        /// <summary>
        ///     Gets a format with defined parameters
        /// </summary>
        /// <param name="color">Color</param>
        /// <param name="size">Size</param>
        /// <returns>Formatted string</returns>
        public string GetFormat(Color color, int size)
        {
            string finalFormat = _defaultFormat;

            // Add information to formatting
            if (c_color) finalFormat = $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{finalFormat}</color>";
            if (c_size) finalFormat = $"<size={Mathf.Clamp(size, 0.01f, 1000)}>{finalFormat}</size>";

            return finalFormat;
        }

        /// <summary>
        ///     Previews the formatted text
        /// </summary>
        /// <returns></returns>
        public string Preview()
        {
            return GetFormat(color, size).Replace(_defaultFormat, target);
        }
    }
    #endregion

    #region Unity Methods
    private void Awake()
    {
        CreateSingleton();
    }
    #endregion
    
    #region Information Popup
    /// <summary>
    ///     Sets the popup
    /// </summary>
    /// <param name="slot">New slot</param>
    public void SetPopup(AbilitySlot slot, Vector3 position)
    {
        if (informationPopup == null) return;
        informationPopup.SetInformation(slot, position);
    }
    /// <summary>
    ///     Sets the popup
    /// </summary>
    /// <param name="slot">New slot</param>
    public void SetPopup(AbilitySlot slot, Ability ability, AbilitySlotModifier modifier, Vector3 position)
    {
        if (informationPopup == null) return;
        informationPopup.SetInformation(slot, ability, modifier, position);
    }

    /// <summary>
    ///     Closes the popup
    /// </summary>
    public void ClosePopup()
    {
        if (informationPopup == null) return;
        informationPopup.SetActive(false);
    }
    #endregion
    #region Formatting
    /// <summary>
    ///     Properly formats an input string based on information formats
    /// </summary>
    /// <param name="input">Input string</param>
    /// <returns>Formatted string</returns>
    public string Format(string input)
    {
        string formatted = input;
        // For each instance of formats find and replace
        for (int i = 0; i < _informationFormats.Length; i++)
        {
            if(Regex.IsMatch(formatted, _informationFormats[i].GetPattern(), RegexOptions.IgnoreCase))
                formatted = Regex.Replace(formatted, _informationFormats[i].GetPattern(), _informationFormats[i].GetFormat(), RegexOptions.IgnoreCase);
        }
        return formatted;
    }
    #endregion
}
