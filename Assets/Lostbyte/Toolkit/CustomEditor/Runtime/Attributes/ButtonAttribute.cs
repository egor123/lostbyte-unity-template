using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Lostbyte.Toolkit.CustomEditor
{
    public class ButtonAttribute : CombinedAttribute
    {
        public string MethodName;
        public ButtonAttribute(string methodName)
        {
            MethodName = methodName;
        }
#if UNITY_EDITOR
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.y += EditorGUI.GetPropertyHeight(property);
            position.height = EditorGUIUtility.singleLineHeight;
            if (GUI.Button(position, MethodName))
                property.serializedObject.targetObject.GetType().GetMethod(MethodName, EditorExtensions.FIELD_FLAGS).Invoke(property.serializedObject.targetObject, null);
        }
        public override float? GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property) + EditorGUIUtility.singleLineHeight;
        }
#endif
    }
}