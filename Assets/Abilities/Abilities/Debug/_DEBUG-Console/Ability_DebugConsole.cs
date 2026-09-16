using HFHandyUtils;

public class Ability_DebugConsole : Ability
{
    public string output = "Debug Ability Activated";

    public override void OnTriggerAbility(AbilityTrace trace)
    {
        base.OnTriggerAbility(trace);
        HFLogger.Log(output);
    }
}
