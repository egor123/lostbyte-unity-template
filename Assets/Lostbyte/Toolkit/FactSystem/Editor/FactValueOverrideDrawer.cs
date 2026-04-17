using UnityEditor;
using UnityEngine;
using System;

namespace Lostbyte.Toolkit.FactSystem.Editor
{
    [CustomPropertyDrawer(typeof(FactValueOverride))]
    public class FactValueOverrideDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var factProp = property.FindPropertyRelative($"<{nameof(FactValueOverride.Fact)}>k__BackingField");
            var wrapperProp = property.FindPropertyRelative($"<{nameof(FactValueOverride.Wrapper)}>k__BackingField");

            var fact = factProp.objectReferenceValue as FactDefinition;

            EditorGUI.BeginProperty(position, label, property);

            if (fact != null)
            {
                var factType = fact.GenericType;
                var expectedWrapperType = GetExpectedWrapperType(factType);

                if (wrapperProp.managedReferenceValue is not IValueHolder wrapper || wrapper.GetType() != expectedWrapperType)
                {
                    var newWrapper = Activator.CreateInstance(expectedWrapperType) as IValueHolder;
                    newWrapper.RawValue = fact.DefaultValueRaw;
                    wrapperProp.managedReferenceValue = newWrapper;
                }

                var gap = 10;
                var factRect = new Rect(position.x, position.y, position.width * 0.5f, EditorGUIUtility.singleLineHeight);
                var valueRect = new Rect(position.x + factRect.width + gap, position.y, position.width - factRect.width - gap, EditorGUIUtility.singleLineHeight);

                EditorGUI.ObjectField(factRect, factProp, GUIContent.none);
                var valueProp = wrapperProp.FindPropertyRelative($"<{nameof(ValueHolder<object>.Value)}>k__BackingField");
                EditorGUI.PropertyField(valueRect, valueProp, GUIContent.none);
            }
            else
            {
                var factRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(factRect, factProp, GUIContent.none);
            }

            EditorGUI.EndProperty();
        }

        private Type GetExpectedWrapperType(Type type)
        {
            if (type == typeof(int)) return typeof(IntValueHolder);
            if (type == typeof(float)) return typeof(FloatValueHolder);
            if (type == typeof(bool)) return typeof(BoolValueHolder);
            if (type == typeof(string)) return typeof(StringValueHolder);
            if (type == typeof(Vector2)) return typeof(Vector2ValueHolder);
            if (type == typeof(Vector3)) return typeof(Vector3ValueHolder);
            if (type == typeof(Vector4)) return typeof(Vector4ValueHolder);
            if (type == typeof(Color)) return typeof(ColorValueHolder);
            if (type == typeof(Enum)) return typeof(EnumValueHolder);

            return typeof(StringValueHolder); // fallback
        }
    }
}