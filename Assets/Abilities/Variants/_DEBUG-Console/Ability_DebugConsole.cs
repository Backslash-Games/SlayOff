using HFHandyUtils;

public class Ability_DebugConsole : Ability
{
    public string output = "Debug Ability Activated";

    public override void OnTrigger()
    {
        HFLogger.Log(output);
    }
}
