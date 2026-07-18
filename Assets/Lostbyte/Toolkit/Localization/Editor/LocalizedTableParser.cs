using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Lostbyte.Toolkit.Common;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;

namespace Lostbyte.Toolkit.Localization.Editor
{
    public class LocalizedTableParser : MonoBehaviour
    {
        // [MenuItem("Tools/Localization/Update Tables", priority = 20)]
        public static bool UpdateTables()
        {
            if (LocalizationSettings.Database == null)
            {
                Print.MError("Failed to update localization tables: Database is null!");
                return false;
            }

            var db = LocalizationSettings.Database;
            var schemaList = db.Schema.Tables;

            string assetPath = AssetDatabase.GetAssetPath(db);
            if (string.IsNullOrEmpty(assetPath))
            {
                Print.MError("Localization Database is not an asset on disk.");
                return false;
            }

            string rootFolder = Path.GetDirectoryName(assetPath);
            string localesFolder = Path.Combine(rootFolder, "Locales");
            string tablesFolder = Path.Combine(rootFolder, "Tables");

            if (!Directory.Exists(localesFolder)) Directory.CreateDirectory(localesFolder);
            if (Directory.Exists(tablesFolder)) Directory.Delete(tablesFolder, true);
            Directory.CreateDirectory(tablesFolder);
            var configs = LoadAndValidateConfigs(localesFolder);

            foreach (var tableSchema in schemaList)
            {
                var filePaths = new Dictionary<string, string>();
                var allLocalesData = PreloadTableStringData(localesFolder, tableSchema.Id, configs.Keys, filePaths);

                var modifiedLocales = new HashSet<string>();

                foreach (var localeName in configs.Keys)
                {
                    LocalizedTable tableAsset = BuildTableAsset(localesFolder, localeName, tableSchema, configs, allLocalesData, modifiedLocales);

                    if (tableAsset != null)
                    {
                        string savePath = Path.Combine(tablesFolder, $"{localeName}_{tableSchema.Id}.asset");
                        AssetDatabase.CreateAsset(tableAsset, savePath);
                        MakeTableAddressable(savePath, tableAsset, localeName, tableSchema.Id);
                    }
                }
                foreach (var localeName in modifiedLocales)
                {
                    if (filePaths.TryGetValue(localeName, out string path) && allLocalesData.TryGetValue(localeName, out JObject jobj))
                    {
                        try
                        {
                            string updatedJson = jobj.ToString(Formatting.Indented);
                            File.WriteAllText(path, updatedJson);
                            Print.MLog($"[Locale: {localeName}] Updated {tableSchema.Id}.json with missing keys.");
                        }
                        catch (Exception e)
                        {
                            Print.MError($"[Locale: {localeName}] Failed to save missing keys back to disk: {e.Message}");
                        }
                    }
                }
            }
            var settingsSo = new SerializedObject(LocalizationSettings.Instance);
            var localesProp = settingsSo.FindProperty("m_locales");
            var locales = configs.Keys.ToArray();
            localesProp.arraySize = locales.Length;
            for (int i = 0; i < locales.Length; i++)
                localesProp.GetArrayElementAtIndex(i).stringValue = locales[i];
            configs.Keys.ForEach(k => Print.MLog(k));
            settingsSo.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Print.MLog("Localization Tables updated successfully.");
            return true;
        }


        private static void MakeLocalizedAssetAddressable(string path, UnityEngine.Object asset, string locale, string tableId)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            string assetGUID = AssetDatabase.AssetPathToGUID(path);
            string groupName = "LocalizedAssets";
            AddressableAssetGroup targetGroup = settings.FindGroup(groupName) ?? settings.CreateGroup(groupName, false, false, true, settings.DefaultGroup.Schemas);
            AddressableAssetEntry entry = settings.CreateOrMoveEntry(assetGUID, targetGroup);
            if (entry != null)
            {
                entry.address = $"Localization/{locale}/{tableId}/{asset.name}";
                string labelName = $"LOCALE_{locale}";
                settings.AddLabel(labelName);
                entry.SetLabel(labelName, true);
                settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
            }
        }

        private static void MakeTableAddressable(string path, UnityEngine.Object asset, string locale, string tableId)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            string assetGUID = AssetDatabase.AssetPathToGUID(path);
            string groupName = LocalizationSettings.k_addressableTableGroupName;

            AddressableAssetGroup targetGroup = settings.FindGroup(groupName);
            if (targetGroup == null)
            {
                Type[] types = { typeof(LocalizedTable) };
                targetGroup = settings.CreateGroup(groupName, false, false, true, settings.DefaultGroup.Schemas, types);
            }

            AddressableAssetEntry entry = settings.CreateOrMoveEntry(assetGUID, targetGroup);
            if (entry != null)
            {
                entry.address = $"Tables/{locale}/{tableId}";
                string labelName = $"LOCALE_{locale}";
                settings.AddLabel(labelName);
                entry.SetLabel(labelName, true);
                settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
            }
        }

        private static Dictionary<string, LocaleConfig> LoadAndValidateConfigs(string localesFolder)
        {
            var configs = new Dictionary<string, LocaleConfig>();

            foreach (var dir in Directory.GetDirectories(localesFolder))
            {
                string locale = new DirectoryInfo(dir).Name;
                string confPath = Path.Combine(dir, "conf.json");

                if (File.Exists(confPath))
                {
                    try
                    {
                        string json = File.ReadAllText(confPath);
                        var config = JsonConvert.DeserializeObject<LocaleConfig>(json);
                        configs[locale] = config;
                    }
                    catch (Exception e)
                    {
                        Print.MError($"Failed to parse conf.json in {dir}: {e.Message}");
                    }
                }
                else
                {
                    Print.MWarn($"{locale} is missing conf.json! Using blank config.");
                    configs[locale] = new LocaleConfig("1.0", null, locale);
                }
            }

            foreach (var locale in configs.Keys.ToList())
            {
                HashSet<string> visited = new();
                string current = locale;

                while (!string.IsNullOrEmpty(current) && configs.TryGetValue(current, out var conf))
                {
                    if (!visited.Add(current))
                    {
                        Print.MError($"Circular dependency detected in fallbacks: {string.Join(" -> ", visited)} -> {current}. Breaking cycle at {current}!");
                        configs[current] = new LocaleConfig(conf.SchemaVersion, null, conf.DisplayName);
                        break;
                    }
                    current = conf.Fallback;
                }
            }

            return configs;
        }

        private static Dictionary<string, JObject> PreloadTableStringData(string localesFolder, string tableId, IEnumerable<string> validLocales, Dictionary<string, string> outPaths)
        {
            var data = new Dictionary<string, JObject>();
            string targetFileName = $"{tableId}.json";

            foreach (var locale in validLocales)
            {
                string localeDir = Path.Combine(localesFolder, locale);
                string dataPath = Directory.EnumerateFiles(localeDir, targetFileName, SearchOption.AllDirectories).FirstOrDefault();

                if (dataPath != null)
                {
                    try
                    {
                        outPaths[locale] = dataPath;
                        string jsonContent = File.ReadAllText(dataPath);
                        data[locale] = JObject.Parse(jsonContent);
                    }
                    catch (Exception e)
                    {
                        Print.MError($"[Locale: {locale}] Malformed JSON in {targetFileName}: {e.Message}");
                    }
                }
                else
                {
                    string defaultDir = Path.Combine(localeDir, tableId);
                    if (!Directory.Exists(defaultDir)) Directory.CreateDirectory(defaultDir);

                    outPaths[locale] = Path.Combine(defaultDir, targetFileName);
                    data[locale] = new JObject();
                }
            }

            return data;
        }

        private static LocalizedTable BuildTableAsset(
            string localesFolder,
            string localeName,
            LocalizationTableSchema schema,
            Dictionary<string, LocaleConfig> configs,
            Dictionary<string, JObject> stringData,
            HashSet<string> modifiedLocales)
        {
            LocalizedTable table = ScriptableObject.CreateInstance<LocalizedTable>();

            var stringEntries = new List<SerializedKeyValuePair<string, string>>();
            var stringArrayEntries = new List<SerializedKeyValuePair<string, string[]>>();
            var addrEntries = new List<LocalizedTable.LocalizedAssetEntry>();
            var addrArrayEntries = new List<LocalizedTable.LocalizedAssetEntries>();

            foreach (var keySchema in schema.Keys)
            {
                bool isSimpleString = keySchema.Types.Count == 1;

                foreach (var requiredType in keySchema.Types)
                {
                    if (keySchema.IsArray)
                    {
                        string[] resolvedArr = ResolveArrayValue(localesFolder, localeName, schema.Id, keySchema.Id, requiredType, isSimpleString, configs, stringData, modifiedLocales);

                        if (resolvedArr != null)
                        {
                            if (requiredType == "string")
                            {
                                var sArr = resolvedArr.Select(s => Formatter.PreFormat(s, keySchema.Args)).ToArray();
                                stringArrayEntries.Add(new SerializedKeyValuePair<string, string[]>(keySchema.Id, sArr));
                            }
                            else
                            {
                                AssetReference[] arr = new AssetReference[resolvedArr.Length];
                                for (int i = 0; i < resolvedArr.Length; i++)
                                    arr[i] = string.IsNullOrEmpty(resolvedArr[i]) ? new AssetReference() : new AssetReference(resolvedArr[i]);

                                addrArrayEntries.Add(new LocalizedTable.LocalizedAssetEntries() { Key = keySchema.Id, AssetType = requiredType, References = arr });
                            }
                        }
                    }
                    else
                    {
                        string resolvedStr = ResolveSingleValue(localesFolder, localeName, schema.Id, keySchema.Id, requiredType, isSimpleString, configs, stringData, modifiedLocales);

                        if (resolvedStr != null)
                        {
                            if (requiredType == "string")
                            {
                                stringEntries.Add(new SerializedKeyValuePair<string, string>(keySchema.Id, Formatter.PreFormat(resolvedStr, keySchema.Args)));
                            }
                            else
                            {
                                addrEntries.Add(new LocalizedTable.LocalizedAssetEntry() { Key = keySchema.Id, AssetType = requiredType, Reference = new AssetReference(resolvedStr) });
                            }
                        }
                    }
                }
            }
            InjectPrivateData(table, localeName, schema.Id, stringEntries, stringArrayEntries, addrEntries, addrArrayEntries);
            return table;
        }

        private static string ResolveSingleValue(string localesFolder, string startLocale, string tableId, string keyId, string reqType, bool isSingleType, Dictionary<string, LocaleConfig> configs, Dictionary<string, JObject> stringData, HashSet<string> modifiedLocales)
        {
            string currentLocale = startLocale;
            while (!string.IsNullOrEmpty(currentLocale))
            {
                if (reqType == "string")
                {
                    if (stringData.TryGetValue(currentLocale, out JObject root))
                    {
                        if (root.TryGetValue(keyId, out JToken keyToken))
                        {
                            string val = ExtractValue(keyToken, reqType, isSingleType);
                            if (!string.IsNullOrEmpty(val)) return val;
                        }
                        else if (currentLocale == startLocale)
                        {
                            root[keyId] = null;
                            modifiedLocales.Add(startLocale);
                        }
                    }
                }
                else
                {
                    string guid = FindAssetGuid(localesFolder, currentLocale, tableId, keyId, reqType);
                    if (!string.IsNullOrEmpty(guid)) return guid;
                }
                if (configs.TryGetValue(currentLocale, out var conf) && !string.IsNullOrEmpty(conf.Fallback))
                {
                    Print.MWarn($"[Locale: {currentLocale}] Missing key/type '{keyId}:{reqType}'. Falling back to {conf.Fallback}.");
                    currentLocale = conf.Fallback;
                }
                else break;
            }

            Print.MError($"[Locale: {startLocale}] Completely missing key/type '{keyId}:{reqType}'. It will be omitted from the table.");
            return null;
        }

        private static string[] ResolveArrayValue(string localesFolder, string startLocale, string tableId, string keyId, string reqType, bool isSingleType, Dictionary<string, LocaleConfig> configs, Dictionary<string, JObject> data, HashSet<string> modifiedLocales)
        {
            string currentLocale = startLocale;

            while (!string.IsNullOrEmpty(currentLocale))
            {
                if (reqType == "string")
                {
                    if (data.TryGetValue(currentLocale, out JObject root))
                    {
                        if (root.TryGetValue(keyId, out JToken keyToken))
                        {
                            if (keyToken is JArray arr)
                            {
                                string[] result = new string[arr.Count];
                                bool hasAnyValidData = false;

                                for (int i = 0; i < arr.Count; i++)
                                {
                                    string val = ExtractValue(arr[i], reqType, isSingleType);
                                    if (!string.IsNullOrEmpty(val))
                                    {
                                        result[i] = val;
                                        hasAnyValidData = true;
                                    }
                                }
                                if (hasAnyValidData) return result;
                            }
                            else
                            {
                                Print.MWarn($"[Locale: {currentLocale}] Key '{keyId}' should be an array but isn't.");
                            }
                        }
                        else if (currentLocale == startLocale)
                        {
                            root[keyId] = null;
                            modifiedLocales.Add(startLocale);
                        }
                    }
                }
                else
                {
                    List<string> assetGuids = new();
                    int index = 0;
                    while (true)
                    {
                        string targetFileName = $"{keyId}_{index}";
                        string guid = FindAssetGuid(localesFolder, currentLocale, tableId, targetFileName, reqType);
                        if (!string.IsNullOrEmpty(guid))
                        {
                            assetGuids.Add(guid);
                            index++;
                        }
                        else break;
                    }
                    if (assetGuids.Count > 0) return assetGuids.ToArray();
                }

                if (configs.TryGetValue(currentLocale, out var conf) && !string.IsNullOrEmpty(conf.Fallback))
                {
                    Print.MWarn($"[Locale: {currentLocale}] Missing array key/files '{keyId}:{reqType}'. Falling back to {conf.Fallback}.");
                    currentLocale = conf.Fallback;
                }
                else break;
            }
            Print.MError($"[Locale: {startLocale}] Completely missing array key/files '{keyId}:{reqType}'. It will be omitted from the table.");
            return null;
        }

        private static string FindAssetGuid(string localesFolder, string locale, string tableId, string fileNameWithoutExtension, string type)
        {
            string localeDir = Path.Combine(localesFolder, locale);
            if (!Directory.Exists(localeDir)) return null;

            var tableDirs = Directory.GetDirectories(localeDir, tableId, SearchOption.AllDirectories);
            foreach (var tableDir in tableDirs)
            {
                var files = Directory.GetFiles(tableDir, "*.*", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    if (Path.GetFileNameWithoutExtension(file).Equals(fileNameWithoutExtension, StringComparison.OrdinalIgnoreCase))
                    {
                        string projectRoot = Directory.GetCurrentDirectory().Replace('\\', '/');
                        string cleanFile = file.Replace('\\', '/');

                        string relativePath = cleanFile.StartsWith(projectRoot)
                            ? cleanFile[projectRoot.Length..].TrimStart('/')
                            : cleanFile;

                        if (!relativePath.StartsWith("Assets/"))
                            relativePath = "Assets/" + relativePath;

                        string guid = AssetDatabase.AssetPathToGUID(relativePath);
                        UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath(relativePath, LocalizationKey.AllowedTypes[type]);
                        if (asset != null && !string.IsNullOrEmpty(guid))
                        {
                            MakeLocalizedAssetAddressable(relativePath, asset, locale, tableId);
                            return guid;
                        }
                    }
                }
            }
            return null;
        }

        private static string ExtractValue(JToken token, string requestedType, bool isSingleType)
        {
            if (token == null || token.Type == JTokenType.Null) return null;

            if (isSingleType)
            {
                return token.Type == JTokenType.String ? token.ToString() : null;
            }
            else
            {
                if (token is JObject obj && obj.TryGetValue(requestedType, out JToken propToken) && propToken.Type == JTokenType.String)
                {
                    return propToken.ToString();
                }
                return null;
            }
        }

        private static void InjectPrivateData(
            LocalizedTable table,
            string locale,
            string tableId,
            List<SerializedKeyValuePair<string, string>> strEntries,
            List<SerializedKeyValuePair<string, string[]>> strArrEntries,
            List<LocalizedTable.LocalizedAssetEntry> addrEntries,
            List<LocalizedTable.LocalizedAssetEntries> addrArrEntries)
        {
            Type t = typeof(LocalizedTable);

            t.GetField("<Locale>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(table, locale);
            t.GetField("<TableId>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(table, tableId);

            t.GetField("m_stringEntries", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(table, strEntries);
            t.GetField("m_stringArrayEntries", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(table, strArrEntries);
            t.GetField("m_addressableEnries", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(table, addrEntries);
            t.GetField("m_addressableArrayEnries", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(table, addrArrEntries);
        }
    }
}