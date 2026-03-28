using UnityEngine;

public class ArcadeResultsHandler : MonoBehaviour
{
    public void DisplayResults()
    {
        EndResults();
    }

    public void EndResults()
    {
        ArcadeModeManager.Instance.GetArcadeGenerator().GenerateNew();
    }
}
