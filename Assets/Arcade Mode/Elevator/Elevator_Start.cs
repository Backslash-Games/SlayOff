using UnityEngine;

public class Elevator_Start : Elevator
{
    [SerializeField] private AudioClip open_audio;
    
    private void Start()
    {
        GetArcadeModeManager().OnPlayerStaged += TriggerAnimation;
        GetArcadeModeManager().OnPlayerStaged += PlayAudio;
    }
    private void OnDestroy()
    {
        GetArcadeModeManager().OnPlayerStaged -= TriggerAnimation;
        GetArcadeModeManager().OnPlayerStaged -= PlayAudio;
    }

    private void PlayAudio(int elevator_index)
    {
        if (open_audio == null)
            return;
        AudioManager.Instance.PlayAudio(open_audio, Vector3.zero, false, true);
    }
}
