using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public enum Type { Pause, Option, GameOver_Results, LayOff }
    [SerializeField] private MenuContainer[] menus = new MenuContainer[0];
    
    #region Singleton
    // Singleton
    private static MenuManager _instance;
    public static MenuManager Instance { get { return _instance; } }
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
    #region Unity Events
    private void Awake()
    {
        CreateSingleton();
        
        foreach (MenuContainer menu in menus)
            menu.Initialize(this);
    }
    private void OnDestroy()
    {
        foreach (MenuContainer menu in menus)
            menu.Disable();
    }
    #endregion

    #region Sequencing
    public void OpenMenu(Type menuType) { TriggerMenu(menuType, true); }
    public void CloseMenu(Type menuType) { TriggerMenu(menuType, false); }

    private void TriggerMenu(Type menuType, bool state)
    {
        foreach (MenuContainer menu in menus)
            if (menu.GetMenuType().Equals(menuType))
                menu.TriggerMenu(state);
    }
    public void ToggleMenu(Type menuType)
    {
        foreach (MenuContainer menu in menus)
            if (menu.GetMenuType().Equals(menuType))
                menu.TriggerMenu(!menu.GetMenuState());
    }

    public void CloseAllMenus()
    {
        Debug.Log("Close all");
        int length = System.Enum.GetValues(typeof(Type)).Length;
        for (int i = 0; i < length; i++)
            TriggerMenu((Type)i, false); 
    }
    #endregion

    #region Button Serialization
    public void OpenPauseMenu() { OpenMenu(Type.Pause); }
    public void ClosePauseMenu() { CloseMenu(Type.Pause); }

    public void OpenOptionsMenu() { OpenMenu(Type.Option); }
    public void CloseOptionsMenu() { CloseMenu(Type.Option); }
    #endregion

    #region Get Methods
    public bool isAnyMenuActive() 
    {
        foreach (MenuContainer menu in menus)
            if (menu.GetMenuState())
                return true;
        return false;
    }
    #endregion
}
