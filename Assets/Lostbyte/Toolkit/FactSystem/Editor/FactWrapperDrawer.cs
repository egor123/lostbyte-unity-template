using UnityEditor;
using UnityEngine;
using System;

namespace Lostbyte.Toolkit.FactSystem.Editor
{
    [CustomPropertyDrawer(typeof(FactWrapper<string>))]
    public class StringFactWrapperDrawer : FactWrapperDrawer<string> { }
    [CustomPropertyDrawer(typeof(FactWrapper<bool>))]
    public class BoolFactWrapperDrawer : FactWrapperDrawer<bool> { }
    [CustomPropertyDrawer(typeof(FactWrapper<int>))]
    public class IntFactWrapperDrawer : FactWrapperDrawer<int> { }
    [CustomPropertyDrawer(typeof(FactWrapper<float>))]
    public class FloatFactWrapperDrawer : FactWrapperDrawer<float> { }
    [CustomPropertyDrawer(typeof(FactWrapper<Enum>))]
    public class EnumFactWrapperDrawer : FactWrapperDrawer<Enum> { }
    [CustomPropertyDrawer(typeof(FactWrapper<Color>))]
    public class ColorFactWrapperDrawer : FactWrapperDrawer<Color> { }
    [CustomPropertyDrawer(typeof(FactWrapper<Vector2>))]
    public class Vector2FactWrapperDrawer : FactWrapperDrawer<Vector2> { }
    [CustomPropertyDrawer(typeof(FactWrapper<Vector3>))]
    public class Vector3FactWrapperDrawer : FactWrapperDrawer<Vector3> { }
    [CustomPropertyDrawer(typeof(FactWrapper<Vector4>))]
    public class Vector4FactWrapperDrawer : FactWrapperDrawer<Vector4> { }



    [CustomPropertyDrawer(typeof(SelfFactWrapper<string>))]
    public class StringSlefFactWrapperDrawer : SelfFactWrapperDrawer<string> { }
    [CustomPropertyDrawer(typeof(SelfFactWrapper<bool>))]
    public class BoolSlefFactWrapperDrawer : SelfFactWrapperDrawer<bool> { }
    [CustomPropertyDrawer(typeof(SelfFactWrapper<int>))]
    public class IntSlefFactWrapperDrawer : SelfFactWrapperDrawer<int> { }
    [CustomPropertyDrawer(typeof(SelfFactWrapper<float>))]
    public class FloatSlefFactWrapperDrawer : SelfFactWrapperDrawer<float> { }
    [CustomPropertyDrawer(typeof(SelfFactWrapper<Enum>))]
    public class EnumSlefFactWrapperDrawer : SelfFactWrapperDrawer<Enum> { }
    [CustomPropertyDrawer(typeof(SelfFactWrapper<Color>))]
    public class ColorSlefFactWrapperDrawer : SelfFactWrapperDrawer<Color> { }
    [CustomPropertyDrawer(typeof(SelfFactWrapper<Vector2>))]
    public class Vector2SlefFactWrapperDrawer : SelfFactWrapperDrawer<Vector2> { }
    [CustomPropertyDrawer(typeof(SelfFactWrapper<Vector3>))]
    public class Vector3SlefFactWrapperDrawer : SelfFactWrapperDrawer<Vector3> { }
    [CustomPropertyDrawer(typeof(SelfFactWrapper<Vector4>))]
    public class Vector4SlefFactWrapperDrawer : SelfFactWrapperDrawer<Vector4> { }


    public class FactWrapperDrawer<T> : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            Type genericType = typeof(T);
            var keyProp = property.FindPropertyRelative($"<{nameof(FactWrapper<object>.Key)}>k__BackingField");
            var factProp = property.FindPropertyRelative($"<{nameof(FactWrapper<object>.Fact)}>k__BackingField");
            var key = keyProp.objectReferenceValue as KeyContainer;
            var fact = factProp.objectReferenceValue as FactDefinition;

            var labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height);
            EditorGUI.LabelField(labelRect, label);
            var w = (position.width - labelRect.width) / 3;
            var fieldPos1 = new Rect(labelRect.x + labelRect.width, labelRect.y, w, position.height);
            var fieldPos2 = new Rect(fieldPos1.x + w, labelRect.y, w, position.height);
            var fieldPos3 = new Rect(fieldPos2.x + w, labelRect.y, w, position.height);
            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            EditorGUI.ObjectField(fieldPos1, keyProp, new GUIContent());
            EditorGUI.ObjectField(fieldPos2, factProp, FieldFactory.GetFactDefenitionTypeByArgType(genericType), new GUIContent());
            EditorGUI.EndDisabledGroup();

            if (fact == null || key == null)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.TextField(fieldPos3, "Unassigned");
                EditorGUI.EndDisabledGroup();
            }
            else if (Application.isPlaying)
            {
                var wrapper = key.GetWrapper(fact);
                EditorGUI.BeginChangeCheck();
                var value = FieldFactory.FactValueField(fieldPos3, "", wrapper.RawValue, fact);
                if (EditorGUI.EndChangeCheck())
                    wrapper.RawValue = value;
            }
            else
            {
                EditorGUI.BeginDisabledGroup(true);
                FieldFactory.FactValueField(fieldPos3, "", fact.DefaultValueRaw, fact);
                EditorGUI.EndDisabledGroup();
            }
            EditorGUI.EndProperty();
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }

    public class SelfFactWrapperDrawer<T> : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            Type genericType = typeof(T);
            var keyProp = property.FindPropertyRelative($"m_key");
            var factProp = property.FindPropertyRelative($"<{nameof(FactWrapper<object>.Fact)}>k__BackingField");

            var keyRef = keyProp.objectReferenceValue as KeyReference;
            if (!Application.isPlaying)
            {
                if (property.serializedObject.targetObject is Component component)
                {
                    var k = component.GetComponentInParent<KeyReference>();
                    if (keyRef != k)
                    {
                        keyProp.objectReferenceValue = keyRef = k;
                    }
                }
            }
            var key = keyRef != null ? keyRef.Key : null;
            var fact = factProp.objectReferenceValue as FactDefinition;

            var labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height);
            EditorGUI.LabelField(labelRect, label);
            var w = (position.width - labelRect.width) / 3;
            var fieldPos1 = new Rect(labelRect.x + labelRect.width, labelRect.y, w, position.height);
            var fieldPos2 = new Rect(fieldPos1.x + w, labelRect.y, w, position.height);
            var fieldPos3 = new Rect(fieldPos2.x + w, labelRect.y, w, position.height);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.TextField(fieldPos1, keyRef != null ? keyRef.name : "null");
            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            EditorGUI.ObjectField(fieldPos2, factProp, FieldFactory.GetFactDefenitionTypeByArgType(genericType), new GUIContent());
            EditorGUI.EndDisabledGroup();

            if (fact == null || key == null)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.TextField(fieldPos3, "Unassigned");
                EditorGUI.EndDisabledGroup();
            }
            else if (Application.isPlaying)
            {
                var wrapper = key.GetWrapper(fact);
                EditorGUI.BeginChangeCheck();
                var value = FieldFactory.FactValueField(fieldPos3, "", wrapper.RawValue, fact);
                if (EditorGUI.EndChangeCheck() && wrapper.RawValue != value)
                    wrapper.RawValue = value;
            }
            else
            {
                EditorGUI.BeginDisabledGroup(true);
                FieldFactory.FactValueField(fieldPos3, "", fact.DefaultValueRaw, fact);
                EditorGUI.EndDisabledGroup();
            }
            EditorGUI.EndProperty();
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}