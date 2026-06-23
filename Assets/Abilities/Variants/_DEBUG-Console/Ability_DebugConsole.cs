using HFHandyUtils;

public class Ability_DebugConsole : Ability
{
    public string output = "Debug Ability Activated";

    public override bool OnTriggerAbility(AbilityTrace trace)
    {
        if(!base.OnTriggerAbility(trace)) return false;

        HFLogger.Log(output);
        return true;
    }
}
