using UnityEngine;

[CreateAssetMenu(fileName = "New Tile", menuName = "SlayOff/Arcade/New Tile")]
public class Arcade_Tile : ScriptableObject
{
    [SerializeField] private string identification = "";
    [SerializeField] private GameObject prefab;

    #region Get Methods
    public string GetIdentification() { return identification; }
    public GameObject GetPrefab() { return prefab; }
    #endregion
}
