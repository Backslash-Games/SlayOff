using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameOver_Controller : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI employee_number_textField = null;
    [SerializeField] private TextMeshProUGUI effective_textField = null;
    [SerializeField] private TextMeshProUGUI reasons_textField = null;
    [Space]
    [SerializeField] private Image bg_image;
    [SerializeField] private float dim_rate = 0.1f;

    private void OnEnable()
    {
        SetEmployeeNumeber();
        SetEffectiveDate();
        SetReasons();
    }
    private void LateUpdate()
    {
        DarkenImage();
    }

    private void SetEmployeeNumeber()
    {
        if (employee_number_textField == null)
            return;

        employee_number_textField.text = GameManager.Instance.Save.employee_number.ToString();
    }

    private void SetEffectiveDate()
    {
        if (effective_textField == null)
            return;

        effective_textField.text = System.DateTime.Now.ToString("MM-dd-yyyy hh:mm tt");
    }

    private void DarkenImage()
    {
        if (bg_image == null)
            return;

        bg_image.color = Color.Lerp(bg_image.color, new Color(bg_image.color.r, bg_image.color.g, bg_image.color.b, 1), dim_rate);
    }

    private void SetReasons()
    {
        if (reasons_textField == null)
            return;

        reasons_textField.text = GameManager.Instance.GetResults();
    }
}
