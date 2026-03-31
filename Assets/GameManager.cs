using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public SaveManager Save;
    private TimeControl time_control;

    #region Singleton
    // Singleton
    private static GameManager _instance;
    public static GameManager Instance { get { return _instance; } }
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
    #region Unity Methods
    private void Awake()
    {
        time_control = new TimeControl(this);
        CreateSingleton();
    }
    #endregion

    private void LoadLevel(string id)
    {
        time_control.ResetScale();
        SceneManager.LoadScene(id);
    }

    public void StartArcadeMode()
    {
        LoadLevel("1_Factory");
    }
    public void GameOver()
    {
        InventoryHandler.Instance.CollapseCollectableFeed();

        int floor = ArcadeModeManager.Instance.GetArcadeGenerator().GetCurrentFloor();
        Save.total_money.Add(InventoryHandler.Instance.GetTotalScore(floor));
        Save.employee_number += 1;

        Save.Save();

        MenuManager.Instance.OpenMenu(MenuManager.Type.GameOver_Results);
    }
    public void QuitGame()
    {
        Application.Quit();
    }

    public string GetResults()
    {
        string output = "";

        int floor = ArcadeModeManager.Instance.GetArcadeGenerator().GetCurrentFloor();

        output += $"Items Got - {InventoryHandler.Instance.GetCollectableLength()}\n";
        output += $"Highest Value Item: {InventoryHandler.Instance.GetCollectable_MostValuablePrice()}\n" +
            $"- {InventoryHandler.Instance.GetCollectable_MostValuableName()}\n\n";

        output += $"Run Cash - {InventoryHandler.Instance.GetTotalScoreString(floor)}\n";
        output += $"- Cash.....{InventoryHandler.Instance.GetFloorScoreString()}\n";
        output += $"- Floor Mult.....x{ArcadeModeManager.Instance.GetArcadeGenerator().GetCurrentFloor()}\n\n";

        output += $"Total Cash - ${Save.total_money.PrettyPrint()}";

        return output;
    }
}
