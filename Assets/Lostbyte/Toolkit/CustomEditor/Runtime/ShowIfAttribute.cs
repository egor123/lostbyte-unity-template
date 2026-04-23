using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Lostbyte.Toolkit.CustomEditor
{
    public class ShowIfAttribute : CombinedAttribute
    {
        public string TargetName;

        public ShowIfAttribute(string targetName)
        {
            TargetName = targetName;
        }
#if UNITY_EDITOR

        private bool _isVisible = true;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _isVisible = ShouldShow(property);
        }
        public override bool DrawDefaultPropertyField()
        {
            return _isVisible;
        }
        public override float? GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return ShouldShow(property) ? base.GetPropertyHeight(property, label) : 0f;
        }

        private bool ShouldShow(SerializedProperty property)
        {
            var targetObject = property.serializedObject.targetObject;
            var type = targetObject.GetType();


            var field = type.GetField(TargetName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null)
            {
                var value = field.GetValue(targetObject);

                if (value is bool b)
                    return b;

                return value != null;
            }

            var prop = type.GetProperty(TargetName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop != null)
            {
                var value = prop.GetValue(targetObject);
                if (value is bool b)
                    return b;

                return value != null;
            }

            Debug.LogWarning($"ShowIf: Cannot find '{TargetName}' on {type.Name}");
            return true;
        }

#endif

    }
}