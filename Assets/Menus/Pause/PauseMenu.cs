using UnityEngine;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI run_indicator;
    [SerializeField] private TextMeshProUGUI floor_indicator;
    [SerializeField] private TextMeshProUGUI total_money_indicator;

    private void OnEnable()
    {
        run_indicator.text = $"Run #{GameManager.Instance.Save.employee_number + 1}";
        floor_indicator.text = $"Floor - {ArcadeModeManager.Instance.GetArcadeGenerator().GetCurrentFloor()}";
        total_money_indicator.text = $"${GameManager.Instance.Save.total_money.PrettyPrint()}";
    }
}
