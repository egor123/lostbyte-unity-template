using System;
using System.Reflection;
using Lostbyte.Toolkit.Common;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Lostbyte.Toolkit.CustomEditor
{
    public class EnumShowIfAttribute : CombinedAttribute
    {
        public string TargetName;
        public object[] TargetValues;
        public bool ExactMatch;
        public EnumShowIfAttribute(string targetName, params object[] targetValues)
                : this(targetName, false, targetValues) { }

        public EnumShowIfAttribute(string targetName, bool exactMatch, params object[] targetValues)
        {
            TargetName = targetName;
            ExactMatch = exactMatch;
            TargetValues = targetValues;
        }

#if UNITY_EDITOR
        private bool _isVisible = true;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _isVisible = ShouldShow(property);
        }

        public override bool DrawDefaultPropertyField() => _isVisible;

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
            var targetObject = property.GetTargetObject();
            if (targetObject == null) return false;
            var type = targetObject.GetType();
            if (string.IsNullOrEmpty(TargetName))
            {
                Print.Warn($"EnumShowIf: Target name is empty on {property.name}!");
                return false;
            }
            object value;
            if (type.GetField(TargetName, k_flags) is FieldInfo field)
                value = field.GetValue(targetObject);
            else if (type.GetProperty(TargetName, k_flags) is PropertyInfo prop)
                value = prop.GetValue(targetObject);
            else if (type.GetMethod(TargetName, k_flags) is MethodInfo method && method.GetParameters().Length == 0)
                value = method.Invoke(targetObject, null);
            else
            {
                Print.Warn($"EnumShowIf: Cannot find '{TargetName}' on {type.Name}");
                return false;
            }
            if (value is not Enum eVal)
            {
                Print.Warn($"EnumShowIf: '{TargetName}' on {type.Name} is not an Enum");
                return false;
            }
            return EvaluateEnumMatch(eVal);
        }

        private bool EvaluateEnumMatch(Enum currentValue)
        {
            if (TargetValues == null || TargetValues.Length == 0) return false;
            long currentLong = Convert.ToInt64(currentValue);
            bool isFlagsEnum = currentValue.GetType().IsDefined(typeof(FlagsAttribute), false);
            foreach (var target in TargetValues)
            {
                try
                {
                    long targetLong = Convert.ToInt64(target);
                    if (ExactMatch || !isFlagsEnum)
                    {
                        if (currentLong == targetLong)
                            return true;
                    }
                    else
                    {
                        if (targetLong == -1 && currentLong == -1) return true;
                        if (targetLong != 0 && (currentLong & targetLong) == targetLong) return true;
                        if (targetLong == 0 && currentLong == 0) return true;
                    }
                }
                catch (Exception e)
                {
                    Print.Warn($"EnumShowIf: Failed to convert target '{target}'. {e.Message}");
                }
            }

            return false;
        }
#endif
    }
}