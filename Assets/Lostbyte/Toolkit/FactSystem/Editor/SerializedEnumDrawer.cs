using UnityEditor;
using UnityEngine;
using System;

namespace Lostbyte.Toolkit.FactSystem.Editor
{
    [CustomPropertyDrawer(typeof(SerializedEnumAttribute))]
    public class SerializedEnumDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            SerializationUtility.ClearAllManagedReferencesWithMissingTypes(property.serializedObject.targetObject);
            Rect labelRect = new(position.x, position.y, string.IsNullOrEmpty(label.text) ? 0 : EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
            Rect fieldRect = new(position.x + labelRect.width, position.y, position.width - labelRect.width, EditorGUIUtility.singleLineHeight);
            Enum enumValue = property.managedReferenceValue as Enum;
            Type currentType = enumValue?.GetType();

            EditorGUI.LabelField(labelRect, label.text);
            if (currentType != null)
            {
                string[] enumNames = Enum.GetNames(currentType);
                int currentIndex = Array.IndexOf(enumNames, enumValue.ToString());
                if (currentIndex == -1 || currentIndex > enumNames.Length)
                {
                    currentIndex = 0;
                    Enum defaultValue = (Enum)Enum.Parse(currentType, enumNames[0]);
                    property.managedReferenceValue = defaultValue;
                }
                int newIndex = EditorGUI.Popup(fieldRect, currentIndex, enumNames);
                if (newIndex != currentIndex && newIndex >= 0)
                {
                    Enum newValue = (Enum)Enum.Parse(currentType, enumNames[newIndex]);
                    property.managedReferenceValue = newValue;
                }
            }
            else
            {
                EditorGUI.LabelField(fieldRect, "Unassigned");

            }
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}