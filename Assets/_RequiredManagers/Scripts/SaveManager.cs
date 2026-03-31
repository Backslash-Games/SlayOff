using UnityEngine;

public class SaveManager : MonoSave
{
    public LimitlessNumeric total_money;
    public int employee_number;

    public float volume_master = 1f;
    public float volume_music = 1f;
    public float volume_sound_effects = 1f;

    public Vector2 camera_sensitivity = new Vector2(5.5f, 5.5f);

    private void Start()
    {
        UpdateOptions();
    }

    public void UpdateOptions()
    {
        AudioManager.Instance.SetMixerVolumes(volume_master, volume_music, volume_sound_effects);

        if(CameraController.Instance != null)
            CameraController.Instance.SetSensitivity(camera_sensitivity.x, camera_sensitivity.y);
    }
}
