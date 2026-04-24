using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Lostbyte.Toolkit.FactSystem.Editor
{
    public static class FieldFactory
    {
        public static VisualElement CreateFactValueField(Type type, string label = null, object value = null, Action<object> OnChange = null)
        {
            if (type == typeof(int))
            {
                var field = new IntegerField() { label = label, value = (int)value };
                if (OnChange != null) field.RegisterValueChangedCallback(evt => OnChange(evt.newValue));
                return field;
            }
            if (type == typeof(float))
            {
                var field = new FloatField() { label = label, value = (float)value };
                if (OnChange != null) field.RegisterValueChangedCallback(evt => OnChange(evt.newValue));
                return field;
            }
            if (type == typeof(string))
            {
                var field = new TextField() { label = label, value = (string)value };
                if (OnChange != null) field.RegisterValueChangedCallback(evt => OnChange(evt.newValue));
                return field;
            }
            if (type == typeof(bool))
            {
                var field = new Toggle() { label = label, value = (bool)value };
                if (OnChange != null) field.RegisterValueChangedCallback(evt => OnChange(evt.newValue));
                return field;
            }
            if (type.IsEnum)
            {
                var field = new EnumField((Enum)Enum.GetValues(type).GetValue(0)) { label = label, value = (Enum)value };
                if (OnChange != null) field.RegisterValueChangedCallback(evt => OnChange(evt.newValue));
                return field;
            }
            if (type == typeof(Vector2))
            {
                var field = new Vector2Field() { label = label, value = (Vector2)value };
                if (OnChange != null) field.RegisterValueChangedCallback(evt => OnChange(evt.newValue));
                return field;
            }
            if (type == typeof(Vector3))
            {
                var field = new Vector3Field() { label = label, value = (Vector3)value };
                if (OnChange != null) field.RegisterValueChangedCallback(evt => OnChange(evt.newValue));
                return field;
            }
            if (type == typeof(Vector4))
            {
                var field = new Vector4Field() { label = label, value = (Vector4)value };
                if (OnChange != null) field.RegisterValueChangedCallback(evt => OnChange(evt.newValue));
                return field;
            }
            if (type == typeof(Color))
            {
                var field = new ColorField() { label = label, value = (Color)value };
                if (OnChange != null) field.RegisterValueChangedCallback(evt => OnChange(evt.newValue));
                return field;
            }
            Debug.LogWarning($"Unsupported field type: {type}");
            return new TextField("Unsupported");
        }

        public static Type GetFactDefenitionTypeByArgType(Type argType)
        {
            if (argType == typeof(int))
                return typeof(IntFactDefinition);
            if (argType == typeof(float))
                return typeof(FloatFactDefinition);
            if (argType == typeof(bool))
                return typeof(BoolFactDefinition);
            if (argType == typeof(string))
                return typeof(StringFactDifenition);
            if (argType == typeof(Vector2))
                return typeof(Vector2FactDifenition);
            if (argType == typeof(Vector3))
                return typeof(Vector3FactDefinition);
            if (argType == typeof(Vector4))
                return typeof(Vector4FactDefinition);
            if (argType == typeof(Color))
                return typeof(ColorFactDefinition);
            if (argType == typeof(Enum))
                return typeof(EnumFactDefinition);
            Debug.LogWarning($"Unsupported arg type: {argType}");
            return null;
        }
        public static object FactValueField(string label, object value, Type type)
        {
            if (type == typeof(int))
                return EditorGUILayout.IntField(label, value is int i ? i : default);
            else if (type == typeof(float))
                return EditorGUILayout.FloatField(label, value is float i ? i : default);
            else if (type == typeof(bool))
                return EditorGUILayout.Toggle(label, value is bool i && i);
            else if (type == typeof(string))
                return EditorGUILayout.TextField(label, value is string i ? i : default);
            else if (type == typeof(Vector2))
                return EditorGUILayout.Vector2Field(label, value is Vector2 i ? i : default);
            else if (type == typeof(Vector3))
                return EditorGUILayout.Vector3Field(label, value is Vector3 i ? i : default);
            else if (type == typeof(Vector4))
                return EditorGUILayout.Vector4Field(label, value is Vector4 i ? i : default);
            else if (type == typeof(Color))
                return EditorGUILayout.ColorField(label, value is Color i ? i : default);
            else if (type == typeof(Enum))
            {
                EditorGUILayout.HelpBox($"Unsupported type: {type.Name}", MessageType.None);
            }
            // return Enum.GetValues(eFact.EnumType).GetValue(EditorGUILayout.Popup(label, (int)value, Enum.GetNames(eFact.EnumType)));
            else
                EditorGUILayout.HelpBox($"Unsupported type: {type.Name}", MessageType.None);
            return null;

        }
        public static object FactValueField(string label, object value, FactDefinition fact)
        {
            Type type = fact.GenericType;
            if (type == typeof(int))
                return EditorGUILayout.IntField(label, (int)value);
            else if (type == typeof(float))
                return EditorGUILayout.FloatField(label, (float)value);
            else if (type == typeof(bool))
                return EditorGUILayout.Toggle(label, (bool)value);
            else if (type == typeof(string))
                return EditorGUILayout.TextField(label, (string)value);
            else if (type == typeof(Vector2))
                return EditorGUILayout.Vector2Field(label, (Vector2)value);
            else if (type == typeof(Vector3))
                return EditorGUILayout.Vector3Field(label, (Vector3)value);
            else if (type == typeof(Vector4))
                return EditorGUILayout.Vector4Field(label, (Vector4)value);
            else if (type == typeof(Color))
                return EditorGUILayout.ColorField(label, (Color)value);
            else if (type == typeof(Enum) && fact is EnumFactDefinition eFact)
                return Enum.GetValues(eFact.EnumType).GetValue(EditorGUILayout.Popup(label, (int)value, Enum.GetNames(eFact.EnumType)));
            else
                EditorGUILayout.HelpBox($"Unsupported type: {type.Name}", MessageType.None);
            return null;
        }
        public static object FactValueField(Rect position, string label, object value, FactDefinition fact)
        {
            Type type = fact.GenericType;
            if (type == typeof(int))
                return EditorGUI.IntField(position, label, (int)value);
            else if (type == typeof(float))
                return EditorGUI.FloatField(position, label, (float)value);
            else if (type == typeof(bool))
                return EditorGUI.Toggle(position, label, (bool)value);
            else if (type == typeof(string))
                return EditorGUI.TextField(position, label, (string)value);
            else if (type == typeof(Vector2))
                return EditorGUI.Vector2Field(position, label, (Vector2)value);
            else if (type == typeof(Vector3))
                return EditorGUI.Vector3Field(position, label, (Vector3)value);
            else if (type == typeof(Vector4))
                return EditorGUI.Vector4Field(position, label, (Vector4)value);
            else if (type == typeof(Color))
                return EditorGUI.ColorField(position, label, (Color)value);
            else if (type == typeof(Enum) && fact is EnumFactDefinition eFact)
            {
                if (eFact.EnumType == null) return null;
                return Enum.GetValues(eFact.EnumType).GetValue(EditorGUI.Popup(position, label, (int)value, Enum.GetNames(eFact.EnumType)));
            }
            else
                EditorGUILayout.HelpBox($"Unsupported type: {type.Name}", MessageType.None);
            return null;
        }
    }
}