using System;
using UnityEditor;
using UnityEngine;
using Lostbyte.Toolkit.Scenes;
using Lostbyte.Toolkit.FactSystem;

namespace Lostbyte.Toolkit.Editor
{
    [UnityEditor.CustomEditor(typeof(SceneLoader))]
    public class SceneLoaderEditor : UnityEditor.Editor
    {
        private SerializedProperty m_factProp;
        private SerializedProperty m_scenesProp;
        private SerializedProperty m_loadingScreen;

        private void OnEnable()
        {
            m_factProp = serializedObject.FindProperty("m_fact");
            m_scenesProp = serializedObject.FindProperty("m_scenes");
            m_loadingScreen = serializedObject.FindProperty("m_loadingScreen");

        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_factProp);
            EditorGUILayout.PropertyField(m_loadingScreen);

            SceneLoader sceneLoader = (SceneLoader)target;
            Type currentEnumType = ExtractEnumTypeFromFact(sceneLoader);


            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Scenes", EditorStyles.boldLabel);

            for (int i = 0; i < m_scenesProp.arraySize; i++)
            {
                SerializedProperty element = m_scenesProp.GetArrayElementAtIndex(i);
                SerializedProperty conditionProp = element.FindPropertyRelative("Condition");
                SerializedProperty sceneProp = element.FindPropertyRelative("Scene");

                EditorGUILayout.BeginVertical(GUI.skin.box);

                // --- Custom Enum Field ---
                if (currentEnumType != null && currentEnumType.IsEnum)
                {
                    Enum currentValue = GetEnumValue(conditionProp, currentEnumType);
                    Enum newValue = EditorGUILayout.EnumPopup("Condition", currentValue);

                    if (!Equals(currentValue, newValue))
                    {
                        conditionProp.managedReferenceValue = newValue;
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("Condition", "Invalid enum type");
                }

                // --- Default Scene field ---
                EditorGUILayout.PropertyField(sceneProp);

                // Remove button
                if (GUILayout.Button("Remove"))
                {
                    m_scenesProp.DeleteArrayElementAtIndex(i);
                    break;
                }

                EditorGUILayout.EndVertical();
            }

            if (GUILayout.Button("Add Scene"))
            {
                m_scenesProp.InsertArrayElementAtIndex(m_scenesProp.arraySize);
            }

            serializedObject.ApplyModifiedProperties();
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

        private Type ExtractEnumTypeFromFact(SceneLoader loader)
        {
            try
            {
                var m_factField = typeof(SceneLoader).GetField("m_fact", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (m_factField == null) return null;

                if (m_factField.GetValue(loader) is not FactWrapper<Enum> factWrapper) return null;
                if (factWrapper.Fact is not EnumFactDefinition enumFact) return null;
                return enumFact.EnumType;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}