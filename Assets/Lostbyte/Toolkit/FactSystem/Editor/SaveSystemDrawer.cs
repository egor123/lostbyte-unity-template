using Lostbyte.Toolkit.FactSystem.Persistance;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SaveSystem))]
public class SaveSystemDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var enabledProp = property.FindPropertyRelative("<Enabled>k__BackingField");

        // Layout rects
        var lineHeight = EditorGUIUtility.singleLineHeight;
        var spacing = 2f;

        var headerRect = new Rect(position.x, position.y, position.width, lineHeight);

        // Toggle + label (header)
        enabledProp.boolValue = EditorGUI.ToggleLeft(
            headerRect,
            label,
            enabledProp.boolValue
        );

        if (!enabledProp.boolValue)
        {
            EditorGUI.EndProperty();
            return;
        }

        // Draw children only when enabled
        EditorGUI.indentLevel++;
        EditorGUI.indentLevel++;

        float y = position.y + lineHeight + spacing;

        DrawProp(property, "m_formatter", ref y, position.width);
        DrawProp(property, "m_storage", ref y, position.width);
        DrawProp(property, "<AutoLoad>k__BackingField", ref y, position.width);
        DrawProp(property, "<SaveOnChange>k__BackingField", ref y, position.width);

        EditorGUI.indentLevel--;
        EditorGUI.indentLevel--;

        EditorGUI.EndProperty();
    }

    private void DrawProp(SerializedProperty root, string name, ref float y, float width)
    {
        var prop = root.FindPropertyRelative(name);
        if (prop == null) return;

        var rect = new Rect(
            0,
            y,
            width,
            EditorGUI.GetPropertyHeight(prop, true)
        );

        EditorGUI.PropertyField(rect, prop, true);
        y += rect.height + 2f;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var enabledProp = property.FindPropertyRelative("<Enabled>k__BackingField");

        float height = EditorGUIUtility.singleLineHeight;

        if (!enabledProp.boolValue)
            return height;

        height += 2f;

        height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("<AutoLoad>k__BackingField"), true);
        height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("<SaveOnChange>k__BackingField"), true);
        height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("m_formatter"), true);
        height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("m_storage"), true);

        height += EditorGUIUtility.singleLineHeight;

        return height;
    }
}
