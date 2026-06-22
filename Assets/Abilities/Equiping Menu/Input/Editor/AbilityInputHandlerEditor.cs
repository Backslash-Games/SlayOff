using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(AbilityInputHandler))]
public class AbilityInputHandlerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        AbilityInputHandler aih = (AbilityInputHandler)target;

        // Display information about 
        GUILayout.TextField("Ability Slots: " + aih.abilityKeys.Length);
        // Display buttons
        if (GUILayout.Button("Build"))
        {
            aih.ResetAll();
            aih.BuildAbilityKeys();
        }
        if (GUILayout.Button("Record Key Data"))
            aih.RecordKeyData();
        if (GUILayout.Button("Build Key Adjacents"))
            aih.BuildAdjacentConnections();
        if (GUILayout.Button("Reset"))
            aih.ResetAll();

        // Run Base
        base.OnInspectorGUI();
    }
}
