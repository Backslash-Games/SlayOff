using HFHandyUtils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(AbilityInformationHandler.InformationFormat))]
public class InformationFormatEditor : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        // Create the root
        var root = new VisualElement();
        // -> Set up foldout and title
        var target_serialized = property.FindPropertyRelative("target");
        var foldout = new Foldout() { text = target_serialized.stringValue, value = false };
        // -> Pull serialized
        var target = property.boxedValue as AbilityInformationHandler.InformationFormat;



        // Show preview of text
        var preview = new Label(target.Preview());



        // Display basic information
        // -> Target
        #region Target String Property
        var target_string = new PropertyField(target_serialized);
        root.TrackPropertyValue(target_serialized, value =>
        {
            target.target = value.stringValue;
            foldout.text = target.target;
            preview.text = target.Preview();
        });
        #endregion
        // -> Color
        #region Color Toggle Property
        var color = GetToggleProperty(property, "color", "c_color", preview, target);
        color.TrackPropertyValue(property.FindPropertyRelative("color"), value =>
        {
            target.color = value.colorValue;
            preview.text = target.Preview();
        });
        color.TrackPropertyValue(property.FindPropertyRelative("c_color"), value =>
        {
            target.c_color = value.boolValue;
            preview.text = target.Preview();
        });
        #endregion
        // -> Size
        #region Size Toggle Property
        var size = GetToggleProperty(property, "size", "c_size", preview, target);
        size.TrackPropertyValue(property.FindPropertyRelative("size"), value =>
        {
            target.size = value.intValue;
            preview.text = target.Preview();
        });
        size.TrackPropertyValue(property.FindPropertyRelative("c_size"), value =>
        {
            target.c_size = value.boolValue;
            preview.text = target.Preview();
        });
        #endregion


        // Add up content
        // -> Foldout
        foldout.Add(preview);
        AddSpace(foldout);

        foldout.Add(target_string);
        AddSpace(foldout);

        foldout.Add(color);
        foldout.Add(size);
        // -> Root
        root.Add(foldout);



        // Finalize
        return root;
    }
    private VisualElement GetToggleProperty(SerializedProperty property, string b_property, string c_property, Label preview, AbilityInformationHandler.InformationFormat boxed)
    {
        // Establish a root
        var root = new VisualElement();
        root.style.flexDirection = FlexDirection.Row;

        // Get serialized properties
        var serializedProperty_field = property.FindPropertyRelative(b_property);
        var serializedProperty_toggle = property.FindPropertyRelative(c_property);

        // Set up base property
        var field = new PropertyField(serializedProperty_field);
        field.style.flexGrow = 1;
        // Set up field toggle
        var field_toggle = new PropertyField(serializedProperty_toggle) { label = "" };

        // Add all
        root.Add(field_toggle);
        root.Add(field);

        // Return root
        return root;
    }
    private void AddSpace(Foldout foldout)
    {
        foldout.Add(new Label(" "));
    }
}
