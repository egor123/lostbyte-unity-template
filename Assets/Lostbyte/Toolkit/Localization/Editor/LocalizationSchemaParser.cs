using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lostbyte.Toolkit.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Lostbyte.Toolkit.Localization.Editor
{
    public static class LocalizationSchemaParser
    {
        private static readonly HashSet<string> AllowedRootFields = new() { "schema_version", "table_id", "meta", "keys" };
        private static readonly HashSet<string> AllowedKeyFields = new() { "id", "meta", "types", "args", "is_array" };

        [MenuItem("Tools/Localization/Update Schema", priority = 10)]
        public static void UpdateScema()
        {
            if (LocalizationSettings.Instance == null || TryParse(LocalizationSettings.Database, out var schema) == false)
            {
                Print.MError("Failed to update localization schema!");
                return;
            }
            var dbSO = new SerializedObject(LocalizationSettings.Database);
            var schemaProp = dbSO.FindProperty($"<{nameof(LocalizationDatabase.Schema)}>k__BackingField");
            schemaProp.boxedValue = schema;
            dbSO.ApplyModifiedPropertiesWithoutUndo();
        }

        public static bool TryParse(LocalizationDatabase db, out LocalizationSchema result)
        {
            if (db == null)
            {
                Print.MError("Localization Database does not exist!");
                result = default;
                return false;
            }
            var assetPath = AssetDatabase.GetAssetPath(db);
            var rootFolder = Path.GetDirectoryName(assetPath);
            var schemaPath = VerefyFolderPath(rootFolder, "Schema");
            var jsonTexts = Directory.GetFiles(schemaPath, "*.*", SearchOption.AllDirectories)
                .Where(f => f.EndsWith(".json") && !f.StartsWith("conf"))
                .Select(File.ReadAllText);

            result = default;
            bool hasErrors = false;
            List<LocalizationTableSchema> tables = new();
            foreach (var jsonText in jsonTexts)
            {
                if (TryParseTable(jsonText, out LocalizationTableSchema table))
                {
                    tables.Add(table);
                }
                else
                {
                    hasErrors = true;
                    break;
                }
            }
            if (!hasErrors)
            {
                result = new(tables);
                return true;
            }
            return false;
        }
        private static string VerefyFolderPath(string rootFolder, string name)
        {
            var path = Path.Combine(rootFolder, name);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                Print.MLog($"Created {name} folder at: {path}");
                AssetDatabase.Refresh();
            }
            return path;
        }

        public static bool TryParseTable(string jsonText, out LocalizationTableSchema result)
        {
            result = default;
            bool hasErrors = false;

            try
            {
                JObject root = JObject.Parse(jsonText);
                hasErrors |= ValidateAllowedFields(root, AllowedRootFields, "Root");
                string schemaVersion = root.Value<string>("schema_version");
                string tableId = root.Value<string>("table_id");
                if (string.IsNullOrWhiteSpace(tableId))
                {
                    Print.MError("Missing required field: 'table_id' at root.");
                    hasErrors = true;
                }
                string rootMeta = root.Value<string>("meta");
                var parsedKeys = new List<LocalizationKey>();
                JArray keysArray = root.Value<JArray>("keys");
                if (keysArray != null)
                {
                    for (int i = 0; i < keysArray.Count; i++)
                    {
                        if (keysArray[i] is JObject keyObj)
                        {
                            hasErrors |= ParseKey(keyObj, i, out LocalizationKey parsedKey);
                            parsedKeys.Add(parsedKey);
                        }
                    }
                }
                if (!hasErrors)
                {
                    result = new(schemaVersion, tableId, rootMeta, parsedKeys);
                    return true;
                }
                return false;
            }
            catch (JsonException ex)
            {
                Print.MError($"Malformed JSON: {ex.Message}");
                return false;
            }
        }

        private static bool ParseKey(JObject keyObj, int index, out LocalizationKey result)
        {
            bool hasErrors = false;
            result = default;

            string id = keyObj.Value<string>("id");
            string contextName = string.IsNullOrEmpty(id) ? $"Key at index {index}" : $"Key '{id}'";
            hasErrors |= ValidateAllowedFields(keyObj, AllowedKeyFields, contextName);

            if (string.IsNullOrWhiteSpace(id))
            {
                Print.MError($"Missing required field 'id' in {contextName}.");
                hasErrors = true;
            }
            string meta = keyObj.Value<string>("meta");
            var typesList = new List<string>();
            if (keyObj.TryGetValue("types", out JToken typesToken))
            {
                if (typesToken.Type == JTokenType.String)
                {
                    typesList.Add(typesToken.ToString());
                }
                else if (typesToken.Type == JTokenType.Array)
                {
                    foreach (var typeItem in typesToken)
                    {
                        typesList.Add(typeItem.ToString());
                    }
                }
                else
                {
                    Print.MError($"Field 'types' must be a string or an array of strings in {contextName}.");
                    hasErrors = true;
                }
            }
            else
            {
                typesList.Add("string");
            }
            var argsList = new List<ArgumentDefinition>();
            if (keyObj.TryGetValue("args", out JToken argsToken) && argsToken.Type == JTokenType.Array)
            {
                foreach (var argItem in argsToken)
                {
                    string rawArg = argItem.ToString();
                    string[] parts = rawArg.Split(':');
                    string argName = parts[0].Trim();
                    string argType = (parts.Length > 1) ? parts[1].Trim() : "object";
                    argsList.Add(new ArgumentDefinition(argName, argType));
                }
            }
            bool isArray = false;
            if (keyObj.TryGetValue("is_array", out JToken isArrayToken))
            {
                if (isArrayToken.Type == JTokenType.Boolean)
                {
                    isArray = isArrayToken.Value<bool>();
                }
                else
                {
                    Print.MError($"Field 'is_array' must be a boolean in {contextName}.");
                    hasErrors = true;
                }
            }
            result = new LocalizationKey(id, meta, typesList, argsList, isArray);
            return hasErrors;
        }

        private static bool ValidateAllowedFields(JObject obj, HashSet<string> allowedFields, string context)
        {
            bool foundExtra = false;
            foreach (var property in obj.Properties())
            {
                if (!allowedFields.Contains(property.Name))
                {
                    Print.MError($"Unrecognized field '{property.Name}' found in {context}.");
                    foundExtra = true;
                }
            }
            return foundExtra;
        }
    }
}
