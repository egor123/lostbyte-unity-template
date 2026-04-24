using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Lostbyte.Toolkit.Localization;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Lostbyte.Toolkit.UI.Editor
{
    [UnityEditor.CustomEditor(typeof(LocalizedField))]
    public class LocalizedFieldEditor : UnityEditor.Editor
    {
        private SerializedProperty m_table;
        private SerializedProperty m_key;
        private SerializedProperty m_args;

        private void OnEnable()
        {
            m_table = serializedObject.FindProperty("m_table");
            m_key = serializedObject.FindProperty("m_key");
            m_args = serializedObject.FindProperty("m_args");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            string currentTable = m_table.stringValue;
            string currentKey = m_key.stringValue;

            List<string> availableTables = new();
            List<string> availableKeys = new();

            bool isValidKey = false;
            string[] requiredArgs = null;

            // 1. Fetch data from LocalizationDatabase via Reflection
            var db = LocalizationSettings.Database;
            if (db != null)
            {
                FieldInfo sourceItemsField = db.GetType().GetField("m_sourceItems", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (sourceItemsField != null && sourceItemsField.GetValue(db) is IList sourceFiles)
                {
                    foreach (var fileObj in sourceFiles)
                    {
                        if (fileObj is not LocalizationDatabase.SourceFile sourceFile) continue;

                        availableTables.Add(sourceFile.Name);

                        // If this is the currently selected table, extract its keys
                        if (sourceFile.Name == currentTable)
                        {
                            FieldInfo keysField = fileObj.GetType().GetField("keys", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                            if (keysField?.GetValue(fileObj) is IList keys)
                            {
                                foreach (var itemObj in keys)
                                {
                                    FieldInfo idField = itemObj.GetType().GetField("id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                                    string id = idField?.GetValue(itemObj) as string;

                                    if (!string.IsNullOrEmpty(id))
                                    {
                                        availableKeys.Add(id);
                                    }

                                    // Check if this specific key is the currently selected one to grab its args
                                    if (id == currentKey)
                                    {
                                        isValidKey = true;
                                        FieldInfo argsField = itemObj.GetType().GetField("args", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                                        requiredArgs = argsField?.GetValue(itemObj) as string[];
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // 2. Draw Table Dropdown (or fallback to text field if no DB data is found)
            if (availableTables.Count > 0)
            {
                int tableIndex = Mathf.Max(0, availableTables.IndexOf(currentTable));
                tableIndex = EditorGUILayout.Popup("Table", tableIndex, availableTables.ToArray());
                m_table.stringValue = availableTables[tableIndex];
                currentTable = m_table.stringValue;
            }
            else
            {
                EditorGUILayout.PropertyField(m_table);
            }

            // 3. Draw Key Dropdown (or fallback to text field)
            if (availableKeys.Count > 0)
            {
                int keyIndex = Mathf.Max(0, availableKeys.IndexOf(currentKey));
                keyIndex = EditorGUILayout.Popup("Key", keyIndex, availableKeys.ToArray());
                m_key.stringValue = availableKeys[keyIndex];
                currentKey = m_key.stringValue;
            }
            else
            {
                EditorGUILayout.PropertyField(m_key);
            }

            // 4. Validate and Draw Arguments
            if (!string.IsNullOrEmpty(currentTable) && !string.IsNullOrEmpty(currentKey))
            {
                if (!isValidKey && availableKeys.Count > 0)
                {
                    EditorGUILayout.HelpBox($"Key '{currentKey}' does not exist in Table '{currentTable}'.", MessageType.Error);
                    m_args.arraySize = 0;
                }
                else
                {
                    int argCount = requiredArgs != null ? requiredArgs.Length : 0;
                    m_args.arraySize = argCount;

                    if (argCount > 0)
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("Arguments", EditorStyles.boldLabel);

                        for (int i = 0; i < argCount; i++)
                        {
                            SerializedProperty argElement = m_args.GetArrayElementAtIndex(i);
                            string argName = requiredArgs[i];

                            SerializedProperty item1 = argElement.FindPropertyRelative("<Item1>k__BackingField");
                            SerializedProperty item2 = argElement.FindPropertyRelative("<Item2>k__BackingField");

                            EditorGUILayout.BeginVertical("box");
                            EditorGUILayout.LabelField($"Argument: {argName}", EditorStyles.miniBoldLabel);

                            if (item1 != null) EditorGUILayout.PropertyField(item1, new GUIContent("Key Container"));
                            if (item2 != null) EditorGUILayout.PropertyField(item2, new GUIContent("Fact Definition"));

                            EditorGUILayout.EndVertical();
                        }
                    }
                    else if (isValidKey)
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("No arguments required for this key.", EditorStyles.helpBox);
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Please select a Table and Key.", MessageType.Info);
                m_args.arraySize = 0;
            }

            serializedObject.ApplyModifiedProperties();

            // 5. Live update the TMP_Text component in the Editor
            if (!Application.isPlaying)
            {
                LocalizedField targetField = (LocalizedField)target;
                TMP_Text tmpText = targetField.GetComponent<TMP_Text>();

                if (tmpText != null)
                {
                    string expectedText = $"{currentTable}/{currentKey}";
                    if (tmpText.text != expectedText)
                    {
                        Undo.RecordObject(tmpText, "Update Localized Field Text");
                        tmpText.text = expectedText;
                        EditorUtility.SetDirty(tmpText);
                    }
                }
            }
        }
    }
}