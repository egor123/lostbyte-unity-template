using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static Lostbyte.Toolkit.Localization.LocalizationDatabase;

namespace Lostbyte.Toolkit.Localization.Editor
{
    public static class LocalizationCodeGenerator
    {
        public const string FileTemplate =
    @"// AUTO-GENERATED FILE — DO NOT EDIT
using Lostbyte.Toolkit.FactSystem;
using Lostbyte.Toolkit.Localization;

namespace {NAMESPACE}
{
{TABLES}
}";

        public const string TableTemplate =
    @"    public static class {TABLE_NAME}
    {
{METHODS}
    }";

        public const string MethodTemplate =
    @"        public static LocalizedString {METHOD_NAME}({ARGS}) => new(""{TABLE_ID}"", ""{KEY}""{ARG_VALUES});";

        private const string Namespace = "Localization";
        private const string ClassName = "Localization";

        public static void Generate(LocalizationDatabase db)
        {
            if (db == null)
            {
                Debug.LogError("No LocalizationDatabase provided for code generation.");
                return;
            }
            var source = db.GetSourceData();
            var file = GenerateFile(Namespace, source.Select(t => GenerateTable(t.Name, t.keys.Select(k => GenerateMethod(t.Name, k.id, GetArgs(k.args))))));
            string assetPath = AssetDatabase.GetAssetPath(LocalizationSettings.Instance);
            string folderPath = Path.GetDirectoryName(assetPath);
            string path = $"{folderPath}/{ClassName}.g.cs";
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, file);
            AssetDatabase.Refresh();

            Debug.Log("Localization code generation complete.");
        }
        private static IEnumerable<(string type, string name)> GetArgs(string[] args)
        {
            return args.Select(a =>
            {
                Debug.Log(a);
                var split = a.Split(':');
                var name = ToCamelCase(split[0]);
                var type = split.Length > 1 ? split[1] : "object"; // TODO
                return (type, name);
            });
        }
        private static string GenerateFile(string namespaceName, IEnumerable<string> tables)
        {
            return FileTemplate
                .Replace("{NAMESPACE}", namespaceName)
                .Replace("{TABLES}", string.Join("\n\n", tables));
        }

        private static string GenerateTable(string tableName, IEnumerable<string> methods)
        {
            var name = $"{ToPascalCase(tableName)}Table";
            return TableTemplate
                .Replace("{TABLE_NAME}", name)
                .Replace("{METHODS}", string.Join("\n", methods));
        }

        private static string GenerateMethod(string tableId, string key, IEnumerable<(string type, string name)> args)
        {
            var name = $"Get{ToPascalCase(key)}String";
            var argDecl = string.Join(", ", args.Select(a => $"LocArg<{a.type}> {a.name}"));
            var argValues = args.Count() > 0 ? ", " + string.Join(", ", args.Select(a => $"{a.name}.Value")) : "";
            return MethodTemplate
                .Replace("{METHOD_NAME}", name)
                .Replace("{TABLE_ID}", tableId)
                .Replace("{KEY}", key)
                .Replace("{ARGS}", argDecl)
                .Replace("{ARG_VALUES}", argValues);
        }
        private static string ToPascalCase(string input)
        {
            if (string.IsNullOrEmpty(input)) return "Unnamed";
            return string.Concat(
                input.Split('_', StringSplitOptions.RemoveEmptyEntries)
                     .Select(s => char.ToUpper(s[0]) + s[1..]));
        }
        private static string ToCamelCase(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "unnamed";
            var parts = input.Split('_', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return "unnamed";
            var first = parts[0].ToLowerInvariant();
            var rest = parts
                .Skip(1)
                .Select(p => char.ToUpperInvariant(p[0]) + p[1..]);
            return first + string.Concat(rest);
        }
    }
}