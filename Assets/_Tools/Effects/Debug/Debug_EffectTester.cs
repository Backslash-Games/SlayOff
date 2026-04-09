using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.VFX;

public class Debug_EffectTester : MonoBehaviour
{
    public enum EffectTesterStates { state_1, state_2, state_3 };

    public EffectLibrary<EffectTesterStates, AudioClip, EffectComponent_Audio.AudioParameters> audioLibrary;
    public EffectLibrary<EffectTesterStates, VisualEffectAsset, EffectComponent_Visual.VisualParameters> particleLibrary;

    public void PlayState(int value)
    {
        // Provide library and state
        EffectManager.Instance.Play(audioLibrary, (EffectTesterStates)value);
        EffectManager.Instance.Play(particleLibrary, (EffectTesterStates)value);
    }

    public void PlayStateContinuous(int value)
    {
        // Provide library and state
        EffectManager.Instance.Play(audioLibrary, (EffectTesterStates)value, EffectManager.PlayMode.Continuous);
        EffectManager.Instance.Play(particleLibrary, (EffectTesterStates)value, EffectManager.PlayMode.Continuous);
    }

    public void StopStateContinuous(int value)
    {
        // Provide library and state
        EffectManager.Instance.Stop(audioLibrary, (EffectTesterStates)value);
        EffectManager.Instance.Stop(particleLibrary, (EffectTesterStates)value);
    }
}
