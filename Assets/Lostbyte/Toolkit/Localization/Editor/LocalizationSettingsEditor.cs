using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.IO;
using System.Linq;
using System;
using Newtonsoft.Json;
using UnityEditor.UIElements;

namespace Lostbyte.Toolkit.Localization.Editor
{
    [CustomEditor(typeof(LocalizationSettings))]
    public class LocalizationSettingsEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            var localesProp = serializedObject.FindProperty("m_locales");
            var localeProp = serializedObject.FindProperty("m_locale");
            var popup = new PopupField<string> { label = "Locale" };
            void RefreshChoices()
            {
                serializedObject.Update();

                var choices = new List<string>();

                if (localesProp != null && localesProp.isArray)
                {
                    for (int i = 0; i < localesProp.arraySize; i++)
                    {
                        choices.Add(localesProp
                            .GetArrayElementAtIndex(i)
                            .stringValue);
                    }
                }

                popup.choices = choices;

                // Sync selected value with m_locale
                if (choices.Contains(localeProp.stringValue))
                    popup.value = localeProp.stringValue;
                else if (choices.Count > 0)
                    popup.value = choices[0];
            }

            // When user changes dropdown → update m_locale
            popup.RegisterValueChangedCallback(evt =>
            {
                serializedObject.Update();
                localeProp.stringValue = evt.newValue;
                serializedObject.ApplyModifiedProperties();
            });

            RefreshChoices();

            root.Add(popup);

            // Track changes to locales array
            root.TrackPropertyValue(localesProp, _ =>
            {
                RefreshChoices();
            });

            // Track changes to selected locale
            root.TrackPropertyValue(localeProp, _ =>
            {
                RefreshChoices();
            });


            // root.Add(new Label("Locales"));
            var listView = new ListView
            {
                reorderable = false,
                selectionType = SelectionType.None,
                showAddRemoveFooter = false,
                showBoundCollectionSize = false,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                bindingPath = localesProp.propertyPath,
                makeItem = () =>
                    {
                        var field = new TextField();
                        field.SetEnabled(false);
                        field.style.flexGrow = 1;
                        return field;
                    },

                bindItem = (element, index) =>
                    {
                        var itemProp = localesProp.GetArrayElementAtIndex(index);
                        ((TextField)element).value = itemProp.stringValue;
                    }
            };
            root.Add(listView);

            root.Add(new Button(UpdateLocalization)
            {
                text = "Update Localization"
            });
            root.Bind(serializedObject);
            return root;
        }


        [Serializable]
        public struct SourceFile
        {
            public SourceItem[] keys;
            [Serializable]
            public struct SourceItem
            {
                public string id;
                public string meta;
                public string[] args;
            }
        }
        [Serializable]
        public struct ConfFile
        {
            public string fallback;
        }
        private LocalizationDatabase GetClearedDB()
        {
            var dbProp = serializedObject.FindProperty("m_database");
            if (dbProp.objectReferenceValue == null)
            {
                var targetObject = (LocalizationSettings)serializedObject.targetObject;
                var newDB = CreateInstance<LocalizationDatabase>();
                newDB.name = "Database";
                AssetDatabase.AddObjectToAsset(newDB, targetObject);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                dbProp.FindPropertyRelative("m_tables").arraySize = 0;
                dbProp.objectReferenceValue = newDB;
                serializedObject.ApplyModifiedProperties();
            }
            var db = dbProp.objectReferenceValue as LocalizationDatabase;
            var subAssets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(db));
            foreach (var asset in subAssets)
            {
                if (asset == db || asset == serializedObject.targetObject) continue;
                AssetDatabase.RemoveObjectFromAsset(asset);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            serializedObject.ApplyModifiedProperties();
            return db;
        }
        private T LoadFile<T>(string file)
        {
            using StreamReader r = new(file);
            string json = r.ReadToEnd();
            return JsonConvert.DeserializeObject<T>(json);
        }
        private void SaveFile<T>(string file, T content)
        {
            using StreamWriter writer = new(file);
            string json = JsonConvert.SerializeObject(content, Formatting.Indented);
            writer.Write(json);
        }

        private string VerefyFolderPath(string rootFolder, string name)
        {
            var path = Path.Combine(rootFolder, name);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                Debug.Log($"Created {name} folder at: {path}");
                AssetDatabase.Refresh();
            }
            return path;
        }
        private string[] GetLocales(string localesFolder)
        {
            return Directory.GetDirectories(localesFolder).Select(d => Path.GetFileName(d)).ToArray();
        }
        private Dictionary<string, SourceFile> GetSourceFiles(string sourceFolder)
        {
            return Directory.GetFiles(sourceFolder, "*.*", SearchOption.AllDirectories)
                .Where(f => f.EndsWith(".json") && !f.StartsWith("conf"))
                .ToDictionary(
                    f => Path.Combine(Path.GetDirectoryName(f[(sourceFolder.Length + 1)..]) ?? "", Path.GetFileName(f)),
                    f => LoadFile<SourceFile>(f)
                );
        }
        private void UpdateLocalization()
        {
            serializedObject.Update();
            var settings = (LocalizationSettings)serializedObject.targetObject;
            var assetPath = AssetDatabase.GetAssetPath(settings);
            var localesProp = serializedObject.FindProperty("m_locales");

            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.LogWarning("Asset path is null or empty.");
                return;
            }

            var rootFolder = Path.GetDirectoryName(assetPath);
            if (string.IsNullOrEmpty(rootFolder) || !Directory.Exists(rootFolder))
            {
                Debug.LogWarning("Folder does not exist: " + rootFolder);
                return;
            }

            var sourceFolder = VerefyFolderPath(rootFolder, "Source");
            var localesFolder = VerefyFolderPath(rootFolder, "Locales");
            var sourceFiles = GetSourceFiles(sourceFolder);
            var locales = GetLocales(localesFolder);
            var db = GetClearedDB();
            var dbSO = new SerializedObject(db);
            var tablesProp = dbSO.FindProperty("m_tables");
            tablesProp.arraySize = 0;
            localesProp.arraySize = locales.Length;
            for (int i = 0; i < locales.Length; i++)
                localesProp.GetArrayElementAtIndex(i).stringValue = locales[i];
            Debug.Log("Detected locales: " + string.Join(',', GetLocales(localesFolder)));
            Dictionary<string, LocalizedTable> tables = new();
            foreach (var locale in locales)
            {
                var localeFolder = VerefyFolderPath(localesFolder, locale);
                var table = CreateInstance<LocalizedTable>();
                table.name = locale;
                AssetDatabase.AddObjectToAsset(table, db);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                tablesProp.arraySize++;
                tablesProp.GetArrayElementAtIndex(tablesProp.arraySize - 1).objectReferenceValue = table;
                tables[locale] = table;
                var tableSO = new SerializedObject(table);
                var entriesProp = tableSO.FindProperty("m_entries");
                entriesProp.arraySize = 0;
                foreach ((var sourcePath, var content) in sourceFiles)
                {
                    var targetFilePath = Path.Combine(
                        localeFolder,
                        sourcePath
                    );
                    var targetDir = Path.GetDirectoryName(targetFilePath);
                    if (!Directory.Exists(targetDir))
                    {
                        Directory.CreateDirectory(targetDir);
                        AssetDatabase.Refresh();
                    }
                    if (!File.Exists(targetFilePath))
                    {
                        File.WriteAllText(targetFilePath, "{\n\n}");
                        Debug.Log("Created: " + targetFilePath);
                    }

                    var data = LoadFile<Dictionary<string, string>>(targetFilePath);
                    foreach (var key in content.keys)
                        if (!data.ContainsKey(key.id))
                            data[key.id] = null;
                    foreach ((var key, var val) in data)
                    {
                        if (val != null)
                        {
                            var value = val;
                            var args = content.keys.FirstOrDefault(d => d.id == key).args;
                            if (args != null)
                            {
                                for (int i = 0; i < args.Length; i++)
                                {
                                    value = value.Replace('{' + args[i].Split(':')[0], "{" + i); //FIXME proper check for double {{
                                }
                            }
                            //TODO check for invalid args
                            entriesProp.arraySize++;
                            var elementProp = entriesProp.GetArrayElementAtIndex(entriesProp.arraySize - 1);
                            elementProp.managedReferenceValue = new LocalizedTable.StringEntry() { Key = key, Value = value };
                        }
                    }

                    SaveFile(targetFilePath, data);
                    tableSO.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(table);
                }
            }
            foreach (var locale in locales)
            {
                var table = tables[locale];
                var tableSO = new SerializedObject(table);
                var fallbackProp = tableSO.FindProperty("m_fallback");
                var confPath = Path.Combine(localesFolder, locale, "conf.json");
                if (!File.Exists(confPath))
                {
                    File.WriteAllText(confPath, JsonConvert.SerializeObject(new ConfFile() { }, Formatting.Indented));
                    Debug.Log("Created: " + confPath);
                }
                var conf = LoadFile<ConfFile>(confPath);
                if (!string.IsNullOrEmpty(conf.fallback))
                {
                    if (tables.ContainsKey(conf.fallback))
                    {
                        fallbackProp.objectReferenceValue = tables[conf.fallback];
                        tableSO.ApplyModifiedPropertiesWithoutUndo();
                        EditorUtility.SetDirty(table);
                    }
                    else Debug.LogWarning($"Unknown fallback locale '{conf.fallback}' in {confPath}");
                }
            }
            AssetDatabase.Refresh();
            dbSO.ApplyModifiedPropertiesWithoutUndo();
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
            EditorUtility.SetDirty(db);
        }
    }
}
