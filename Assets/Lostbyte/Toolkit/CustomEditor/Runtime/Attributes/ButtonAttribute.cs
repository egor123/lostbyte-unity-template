using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Lostbyte.Toolkit.CustomEditor
{
    public class ButtonAttribute : CombinedAttribute
    {
        public string[] MethodNames;
        public ButtonAttribute(params string[] methodNames)
        {
            MethodNames = methodNames;
        }
#if UNITY_EDITOR
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.y += EditorGUI.GetPropertyHeight(property);
            position.height = EditorGUIUtility.singleLineHeight;
            foreach (var methodName in MethodNames)
            {
                if (GUI.Button(position, methodName))
                {
                    property.serializedObject.targetObject.GetType().GetMethod(methodName, EditorExtensions.FIELD_FLAGS).Invoke(property.serializedObject.targetObject, null);
                }
                position.y += EditorGUIUtility.singleLineHeight;
            }
        }
        public override float? GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property) + EditorGUIUtility.singleLineHeight * MethodNames.Length;
        }
#endif
    }
}