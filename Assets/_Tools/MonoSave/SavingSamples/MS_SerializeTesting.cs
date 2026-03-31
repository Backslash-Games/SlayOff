using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MS_SerializeTesting : MonoSave
{
    [SerializeField]
    public List<int[]> intmapTest = new List<int[]>();
    [SerializeField]
    public Dictionary<int, Dictionary<int, string>> dictionaryTest;

    private void Start()
    {
        PopulateIntMap();
        Save();
    }

    // Loads the int map with information
    void PopulateIntMap()
    {
        for(int i = 0; i < 10; i++)
            intmapTest.Add(new int[10]);
    }
}
