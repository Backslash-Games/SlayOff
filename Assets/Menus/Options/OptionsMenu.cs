using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionsMenu : MonoBehaviour
{
    [Header("Raw Values")]
    [SerializeField] private float volume_master;
    [SerializeField] private float volume_music;
    [SerializeField] private float volume_sound_effects;
    [Space]
    [SerializeField] private float horizontal_sensitivity;
    [SerializeField] private float vertical_sensitivity;

    [Header("Sliders")]
    [SerializeField] private Slider v_master_slider;
    [SerializeField] private Slider v_music_slider;
    [SerializeField] private Slider v_soundEffect_slider;
    [Space]
    [SerializeField] private Slider h_sensitivity_slider;
    [SerializeField] private Slider v_sensitivity_slider;

    [Header("Readouts")]
    [SerializeField] private TextMeshProUGUI v_master_text;
    [SerializeField] private TextMeshProUGUI v_music_text;
    [SerializeField] private TextMeshProUGUI v_soundEffect_text;
    [Space]
    [SerializeField] private TextMeshProUGUI h_sensitivity_text;
    [SerializeField] private TextMeshProUGUI v_sensitivity_text;

    private static readonly Vector3 default_volume = Vector3.one;
    private static readonly Vector2 default_camera_sensitivity = new Vector2(5.5f, 5.5f);

    private void OnEnable()
    {
        LoadAllValues();
        SetSliderValues();
        UpdateReadouts();
    }

    #region Readouts
    public void UpdateReadouts()
    {
        SetReadout(v_master_text, v_master_slider.value);
        SetReadout(v_music_text, v_music_slider.value);
        SetReadout(v_soundEffect_text, v_soundEffect_slider.value);

        SetReadout(h_sensitivity_text, h_sensitivity_slider.value);
        SetReadout(v_sensitivity_text, v_sensitivity_slider.value);
    }
    private void SetReadout(TextMeshProUGUI textField, float value)
    {
        float print = Mathf.FloorToInt(value * 100);
        textField.text = print.ToString();
    }
    #endregion
    #region Updates
    public void UpdateAllValues()
    {
        volume_master = v_master_slider.value;
        volume_music = v_music_slider.value;
        volume_sound_effects = v_soundEffect_slider.value;

        horizontal_sensitivity = h_sensitivity_slider.value;
        vertical_sensitivity = v_sensitivity_slider.value;


        GameManager.Instance.Save.volume_master = volume_master;
        GameManager.Instance.Save.volume_music = volume_music;
        GameManager.Instance.Save.volume_sound_effects = volume_sound_effects;

        GameManager.Instance.Save.camera_sensitivity = new Vector2(horizontal_sensitivity, vertical_sensitivity);

        GameManager.Instance.Save.UpdateOptions();
    }
    public void SaveAllValues()
    {
        UpdateAllValues();
        GameManager.Instance.Save.Save();
    }
    public void LoadAllValues()
    {
        volume_master = GameManager.Instance.Save.volume_master;
        volume_music = GameManager.Instance.Save.volume_music;
        volume_sound_effects = GameManager.Instance.Save.volume_sound_effects;

        horizontal_sensitivity = GameManager.Instance.Save.camera_sensitivity.x;
        vertical_sensitivity = GameManager.Instance.Save.camera_sensitivity.y;
    }
    #endregion

    #region Reset
    public void ResetToDefault()
    {
        volume_master = default_volume.x;
        volume_music = default_volume.y;
        volume_sound_effects = default_volume.z;

        horizontal_sensitivity = default_camera_sensitivity.x;
        vertical_sensitivity = default_camera_sensitivity.y;

        SetSliderValues();
        UpdateReadouts();

        UpdateAllValues();
    }
    #endregion
    #region Set Methods
    private void SetSliderValues()
    {
        v_master_slider.SetValueWithoutNotify(volume_master);
        v_music_slider.SetValueWithoutNotify(volume_music);
        v_soundEffect_slider.SetValueWithoutNotify(volume_sound_effects);

        h_sensitivity_slider.SetValueWithoutNotify(horizontal_sensitivity);
        v_sensitivity_slider.SetValueWithoutNotify(vertical_sensitivity);
    }
    #endregion
}
