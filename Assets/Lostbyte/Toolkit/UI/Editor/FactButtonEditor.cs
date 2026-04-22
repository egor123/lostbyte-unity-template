using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Lostbyte.Toolkit.UI;
using Lostbyte.Toolkit.FactSystem;
using Lostbyte.Toolkit.CustomEditor;

namespace Lostbyte.Toolkit.UI.Editor
{
    [UnityEditor.CustomEditor(typeof(FactButton))]
    public class FactButtonEditor : UnityEditor.Editor
    {
        private SerializedProperty m_buttonProp;
        private SerializedProperty m_keyProp;
        private SerializedProperty m_factProp;
        private SerializedProperty m_valueProp;

        private void OnEnable()
        {
            m_buttonProp = serializedObject.FindProperty("m_button");
            m_keyProp = serializedObject.FindProperty("m_key");
            m_factProp = serializedObject.FindProperty("m_fact");
            m_valueProp = serializedObject.FindProperty("m_value");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_buttonProp);
            EditorGUILayout.PropertyField(m_keyProp);
            EditorGUILayout.PropertyField(m_factProp);

            FactButton button = (FactButton)target;
            Type expectedType = ExtractFactValueType(button);

            DrawValueField(button, expectedType);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawValueField(FactButton button, Type expectedType)
        {
            if (expectedType == null)
            {
                EditorGUILayout.HelpBox("Fact type not resolved.", MessageType.Warning);
                EditorGUILayout.PropertyField(m_valueProp);
                return;
            }
            var currentObject = m_valueProp.managedReferenceValue;

            Type validType = UniqeReferenceAttribute.GetSubClasses(typeof(IValueHolder))
                .FirstOrDefault(t => MatchesType(t, expectedType));


            if (validType == null)
            {
                EditorGUILayout.HelpBox($"No ValueHolder found for {expectedType}", MessageType.Error);
                return;
            }

            if (currentObject == null || currentObject.GetType() != validType)
            {
                m_valueProp.managedReferenceValue = Activator.CreateInstance(validType);
            }
            var valueProp = m_valueProp.FindPropertyRelative("<Value>k__BackingField");
            if (validType.Equals(typeof(EnumValueHolder)))
            {
                var enumType = ExtractEnumTypeFromFact(button);
                Enum currentValue = GetEnumValue(valueProp, enumType);
                Enum newValue = EditorGUILayout.EnumPopup("Value", currentValue);
                if (!Equals(currentValue, newValue))
                {
                    valueProp.managedReferenceValue = newValue;
                }
            }
            else
            {
                EditorGUILayout.PropertyField(valueProp);
            }
        }

        private Type ExtractFactValueType(FactButton button)
        {
            if (button == null) return null;
            if (button.GetType().GetField("m_fact",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(button) is not FactDefinition fact)
                return null;
            return fact.GenericType;
        }

        private bool MatchesType(Type holderType, Type expectedType)
        {
            return holderType.BaseType.GetGenericArguments()[0] == expectedType;
        }

        private Enum GetEnumValue(SerializedProperty prop, Type enumType)
        {
            try
            {
                return (Enum)Enum.ToObject(enumType, prop.managedReferenceValue);
            }
            catch
            {
                return (Enum)Enum.GetValues(enumType).GetValue(0);
            }
        }

        private Type ExtractEnumTypeFromFact(FactButton loader)
        {
            try
            {
                var m_factField = typeof(FactButton).GetField("m_fact", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (m_factField == null) return null;

                if (m_factField.GetValue(loader) is not EnumFactDefinition enumFact) return null;
                return enumFact.EnumType;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
