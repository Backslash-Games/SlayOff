using UnityEngine;

public class ArcadeResultsHandler : MonoBehaviour
{
    private PlayerController player;

    public void DisplayResults()
    {
        EndResults();
    }

    public void EndResults()
    {
        ArcadeModeManager.Instance.GetArcadeGenerator().GenerateNew();
        GetPlayer().Heal("Elevator", 25);
    }

    private PlayerController GetPlayer()
    {
        if (player == null)
            player = FindAnyObjectByType<PlayerController>();
        return player;
    }
}
