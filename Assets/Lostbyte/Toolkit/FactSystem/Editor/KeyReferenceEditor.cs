using UnityEditor;
using UnityEngine;
using System;
using System.Linq;

namespace Lostbyte.Toolkit.FactSystem.Editor
{
    [CustomEditor(typeof(KeyReference))]
    public class KeyReferenceEditor : UnityEditor.Editor
    {
        private SerializedProperty _parentKeyProp;
        private SerializedProperty _keyProp;
        private bool _hasKey;
        private bool _showRuntimeValues = true;

        private void OnEnable()
        {
            _parentKeyProp = serializedObject.FindProperty($"<{nameof(KeyReference.ParentKey)}>k__BackingField");
            _keyProp = serializedObject.FindProperty("m_key");
        }

        public override void OnInspectorGUI()
        {
            var keyRef = (KeyReference)target;
            serializedObject.Update();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Parent", GUILayout.Width(50));


            if (keyRef.Key != null)
            {
                var parent = FactEditorUtils.GetAllKeys().Find(k => k.Children.Contains(keyRef.Key));
                if (parent != keyRef.ParentKey)
                {
                    _parentKeyProp.objectReferenceValue = parent;
                }
            }
            using (new EditorGUI.DisabledScope(keyRef.Key != null || Application.isPlaying))
            {
                EditorGUILayout.PropertyField(_parentKeyProp, GUIContent.none);
            }

            EditorGUILayout.LabelField("Key", GUILayout.Width(30));
            using (new EditorGUI.DisabledScope(Application.isPlaying))
            {
                EditorGUILayout.PropertyField(_keyProp, GUIContent.none);
            }
            EditorGUILayout.EndHorizontal();

            _hasKey = keyRef.Key != null;

            EditorGUILayout.BeginHorizontal();
            using (new EditorGUI.DisabledScope(_hasKey || Application.isPlaying))
            {
                if (GUILayout.Button("Create"))
                {
                    FactEditorUtils.ShowAddNewKeyPopup(keyRef.ParentKey, FactEditorUtils.GenerateValidIdentifier(keyRef.name), default, keyRef.ValueOverrides, key =>
                    {
                        _keyProp.objectReferenceValue = key;
                        serializedObject.ApplyModifiedProperties();
                    });
                }
            }

            using (new EditorGUI.DisabledScope(!_hasKey || Application.isPlaying))
            {
                if (GUILayout.Button("Copy"))
                {
                    FactEditorUtils.ShowDublicateKeyPopup(keyRef.Key, FactEditorUtils.GenerateValidIdentifier(keyRef.Key.name), default, (key) =>
                    {
                        _keyProp.objectReferenceValue = key;
                        serializedObject.ApplyModifiedProperties();
                    });
                }

                if (GUILayout.Button("Delete"))
                {
                    FactEditorUtils.ShowDeleteKeyModal(keyRef.Key, () =>
                    {
                        _keyProp.objectReferenceValue = null;
                        serializedObject.ApplyModifiedProperties();
                    });
                }
            }
            EditorGUILayout.EndHorizontal();

            DrawValueOverrides(keyRef);

            serializedObject.ApplyModifiedProperties();
        }
        private void DrawValueOverrides(KeyReference keyRef)
        {
            if (Application.isPlaying && keyRef.Key != null)
            {
                _showRuntimeValues = EditorGUILayout.Foldout(_showRuntimeValues, "Runtime Values", true);
                if (_showRuntimeValues)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUI.indentLevel++;

                    var key = keyRef.Key;
                    foreach (var fact in key.DefinedFacts)
                    {
                        var wrapper = key.GetWrapper(fact);
                        var value = wrapper.RawValue;
                        var type = fact.GenericType;

                        EditorGUI.BeginChangeCheck();
                        var newValue = FieldFactory.FactValueField(fact.name, value, fact);
                        if (EditorGUI.EndChangeCheck())
                            wrapper.RawValue = newValue;
                    }
                    foreach (var @event in key.DefinedEvents)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel(@event.name);
                        if (GUILayout.Button("Raise"))
                            key.Raise(@event);
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUI.indentLevel--;
                    EditorGUILayout.EndVertical();
                }
            }
            else if (!Application.isPlaying)
            {
                if (keyRef.Key != null)
                {
                    SerializedObject so = new(keyRef.Key);
                    var valueOverridesProp = so.FindProperty($"<{nameof(KeyContainer.ValueOverrides)}>k__BackingField");
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(valueOverridesProp);
                    if (valueOverridesProp.isArray)
                    {
                        var keyFacts = keyRef.Key.Facts.Select(f => f).ToList();

                        for (int i = 0; i < valueOverridesProp.arraySize; i++)
                        {
                            var element = valueOverridesProp.GetArrayElementAtIndex(i);
                            var factProp = element.FindPropertyRelative($"<{nameof(FactValueOverride.Fact)}>k__BackingField");
                            var fact = factProp.objectReferenceValue as FactDefinition;

                            if (fact != null && !keyFacts.Contains(fact))
                            {
                                keyRef.Key.Facts.Add(fact);
                                EditorUtility.SetDirty(keyRef.Key);
                            }
                        }
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                        so.ApplyModifiedProperties();
                        // so.Update();
                        EditorUtility.SetDirty(FactEditorUtils.Database);
                        // AssetDatabase.SaveAssets();
                    }
                }
                else
                {
                    var valueOverridesProp = serializedObject.FindProperty($"<{nameof(KeyReference.ValueOverrides)}>k__BackingField");
                    EditorGUILayout.PropertyField(valueOverridesProp);
                }
            }

        }
    }
}