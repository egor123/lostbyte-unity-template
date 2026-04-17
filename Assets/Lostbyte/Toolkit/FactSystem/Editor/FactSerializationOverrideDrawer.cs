using UnityEditor;
using UnityEngine;

namespace Lostbyte.Toolkit.FactSystem.Editor
{
    [CustomPropertyDrawer(typeof(FactSerializationOverride))]
    public class FactSerializationOverrideDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var factProp = property.FindPropertyRelative($"<{nameof(FactSerializationOverride.Fact)}>k__BackingField");
            var serializableProp = property.FindPropertyRelative($"<{nameof(FactSerializationOverride.IsSerializable)}>k__BackingField");

            EditorGUI.BeginProperty(position, label, property);

            float w = position.width * 0.7f;
            var factRect = new Rect(position.x, position.y, w, position.height);
            var toggleRect = new Rect(position.x + w, position.y, position.width - w, position.height);

            EditorGUI.PropertyField(factRect, factProp, GUIContent.none);
            EditorGUI.PropertyField(toggleRect, serializableProp, GUIContent.none);

            EditorGUI.EndProperty();
        }
    }
}