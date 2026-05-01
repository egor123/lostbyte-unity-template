using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Lostbyte.Toolkit.Common;
using UnityEditor;
using UnityEngine;

namespace Lostbyte.Toolkit.CustomEditor
{
    public class ShowIfAttribute : CombinedAttribute
    {
        public string[] TargetNames;
        public Type[] TargetTypes;
        public bool IncludeChildrenTypes;

        public ShowIfAttribute(params string[] targetNames)
        {
            TargetNames = targetNames;
            TargetTypes = null;
            IncludeChildrenTypes = false;
        }
        public ShowIfAttribute(params Type[] types)
        {
            TargetNames = null;
            TargetTypes = types;
            IncludeChildrenTypes = false;
        }
        public ShowIfAttribute(bool includeChildren, params Type[] types)
        {
            TargetNames = null;
            TargetTypes = types;
            IncludeChildrenTypes = includeChildren;
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

        private const BindingFlags k_flags = BindingFlags.Instance |
            BindingFlags.Static |
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.FlattenHierarchy;

        private bool ShouldShow(SerializedProperty property)
        {
            var targetObject = property.serializedObject.targetObject;
            var type = targetObject.GetType();

            if (TargetNames != null && TargetNames.Length > 0)
            {
                foreach (var target in TargetNames)
                {
                    object value = null;
                    if (type.GetField(target, k_flags) is FieldInfo field)
                        value = field.GetValue(targetObject);
                    else if (type.GetProperty(target, k_flags) is PropertyInfo prop)
                        value = prop.GetValue(targetObject);
                    else if (type.GetMethod(target, k_flags) is MethodInfo method && method.GetParameters().Length == 0)
                        value = method.Invoke(targetObject, null);
                    else
                    {
                        DebugLogger.LogWarning($"ShowIf: Cannot find '{target}' on {type.Name}", targetObject);
                        continue;
                    }
                    if (value switch
                    {
                        bool b => b,
                        float f => f > 0,
                        int i => i > 0,
                        string s => !string.IsNullOrWhiteSpace(s),
                        UnityEngine.Object obj => obj != null,
                        null => false,
                        _ => false
                    }) return true;
                }
                return false;
            }
            else if (TargetTypes != null && TargetTypes.Length > 0)
            {

                if (targetObject is not Component c)
                {
                    DebugLogger.LogWarning($"ShowIf: Target must be Component for type check!", targetObject);
                    return false;
                }
                return TargetTypes.Any(t => IncludeChildrenTypes ? c.GetComponentInChildren(t) : c.GetComponent(t));
            }
            DebugLogger.LogWarning($"ShowIf: No valid condition", targetObject);
            return false;
        }

#endif

    }
}