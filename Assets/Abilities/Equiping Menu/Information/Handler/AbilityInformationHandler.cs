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

    #region Unity Methods
    private void Awake()
    {
        CreateSingleton();
    }
    #endregion
}
