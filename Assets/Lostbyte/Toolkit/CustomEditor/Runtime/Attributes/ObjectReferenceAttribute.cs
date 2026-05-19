using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Lostbyte.Toolkit.CustomEditor
{
    public class OfTypeAttribute : CombinedAttribute
    {
        public readonly Type Type;
        public bool AllowSceneObjects;
        public OfTypeAttribute(Type type, bool allowSceneObjects = true) => (Type, AllowSceneObjects) = (type, allowSceneObjects);
#if UNITY_EDITOR
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                EditorGUI.LabelField(position, label.text, "Use OfType with ObjectReference properties only.");
                return;
            }
            UnityEngine.Object objectValue = EditorGUI.ObjectField(position, label, property.objectReferenceValue, Type, true);
            if (objectValue != null && !Type.IsAssignableFrom(objectValue.GetType()))
            {
                property.objectReferenceValue = null;
            }
            else
            {
                property.objectReferenceValue = objectValue;
            }
        }
        public override float? GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
        public override bool DrawDefaultPropertyField() => false;
#endif
    }
}
