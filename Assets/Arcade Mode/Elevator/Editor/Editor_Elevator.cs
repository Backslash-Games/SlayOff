using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Elevator))]
public class Editor_Elevator : Editor
{
    public override void OnInspectorGUI()
    {
        // Make sure default is set up
        base.OnInspectorGUI();

        // Store reference
        Elevator source = (Elevator)target;

        // Toggle target elevator element
        
    }
}
