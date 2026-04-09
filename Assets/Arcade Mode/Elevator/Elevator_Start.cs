using UnityEngine;

public class Elevator_Start : Elevator
{
    private enum EffectStates { Open, Moving };
    [SerializeField] private EffectLibrary<EffectStates, AudioClip, EffectComponent_Audio.AudioParameters> audioLibrary;
    
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
        if (elevator_index != 1)
            return;
        EffectManager.Instance.Play(audioLibrary, EffectStates.Open);
    }
}
