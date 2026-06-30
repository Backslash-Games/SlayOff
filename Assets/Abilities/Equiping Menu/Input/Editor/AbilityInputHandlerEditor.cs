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
        GUILayout.TextField("Ability Slots: " + aih.abilitySlotData.Length);
        // Display buttons
        if (GUILayout.Button("Build"))
        {
            aih.ResetAll();
            aih.BuildAbilityKeys();
            aih.RecordKeyData();
            aih.BuildAdjacentConnections();
        }
        if (GUILayout.Button("Update Key Data"))
            aih.RecordKeyData();
        if (GUILayout.Button("Reset"))
            aih.ResetAll();

        // Run Base
        base.OnInspectorGUI();
    }
}
