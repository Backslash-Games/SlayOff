using UnityEngine;

public class Elevator_Results : Elevator
{
    [SerializeField] private GameObject upgradesParent;

    /*private void Start()
    {
        GetArcadeModeManager().OnPlayerStaged += CheckTeleport;
    }
    private void OnDestroy()
    {
        GetArcadeModeManager().OnPlayerStaged -= CheckTeleport;
    }

    private void CheckTeleport(int index)
    {
        if (index != 0)
            return;
        
        ArcadeModeManager.Instance.GetArcadeGenerator().GenerateNew();
        GetPlayer().Heal("Elevator", 25);
        //SpawnUpgrades();
    }
    private void SpawnUpgrades()
    {
        upgradesParent.SetActive(true);
    }
    private void DespawnUpgrades()
    {
        upgradesParent.SetActive(false);
    }

    public void UpgradeFinished()
    {
        DespawnUpgrades();
        ArcadeModeManager.Instance.GetArcadeGenerator().GenerateNew();
    }*/
}
