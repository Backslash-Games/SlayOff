using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
/*using UnityEngine.UI;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(SpawnableObject_Attribute))]
public class SO_AttributeEditor : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        return base.CreatePropertyGUI(property);
        // Pull target
        var target = property.boxedValue as SpawnableObject_Attribute;
        // Create a new visual element
        VisualElement root = new VisualElement();
        Foldout foldout = new Foldout();
        foldout.text = target.tag.ToString();

        // Add enum selection
        EnumField enumField = new EnumField("Tag", (SpawnableObject_Attribute.Tag)0)
        {
            bindingPath = "tag"
        };
        root.TrackPropertyValue(property.FindPropertyRelative("tag"), value => 
        {
            target.tag = (SpawnableObject_Attribute.Tag)value.intValue;
            foldout.text = target.tag.ToString(); 
        });
        // Add value input
        PropertyField valueField = new PropertyField(property.FindPropertyRelative("value"));

        // Add elements to root
        foldout.Add(enumField);
        foldout.Add(valueField);
        root.Add(foldout);

        return root;
    }
}
*/