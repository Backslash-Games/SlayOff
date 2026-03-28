using UnityEngine;

public class Elevator_Start : Elevator
{
    private void Start()
    {
        GetArcadeModeManager().OnPlayerStaged += TriggerAnimation;
    }
    private void OnDestroy()
    {
        GetArcadeModeManager().OnPlayerStaged -= TriggerAnimation;
    }
}
