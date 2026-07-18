using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lostbyte.Toolkit.Common;
using UnityEditor;
using UnityEngine;
using static Lostbyte.Toolkit.Localization.LocalizationDatabase;

namespace Lostbyte.Toolkit.Localization.Editor
{
    public static class LocalizationCodeGenerator
    {
        public const string FileTemplate =
    @"// AUTO-GENERATED FILE — DO NOT EDIT
using System.Runtime.CompilerServices;
using Lostbyte.Toolkit.FactSystem;
using Lostbyte.Toolkit.Localization;

namespace {NAMESPACE}
{
{TABLES}
}";

        public const string TableTemplate =
    @"    public static class {TABLE_NAME}
    {
        public const string Key = ""{KEY}"";
{METHODS}
    }";

        public const string RefMethodTemplate =
    @"        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static {TYPES} {METHOD_NAME}({ARGS}) => new(""{TABLE_ID}"", ""{KEY}""{ARG_VALUES});";
        public const string ValMethodTemplate =
    @"        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static {TYPE} {METHOD_NAME}({ARGS}) => LocalizationDatabase.GetValue<{TYPE}>(""{TABLE_ID}"", ""{KEY}""{ARG_VALUES});";

        private const string Namespace = "Localization";
        private const string ClassName = "Localization";

        // [MenuItem("Tools/Localization/Generate Code")]
        public static bool Generate()
        {
            LocalizationDatabase db = LocalizationSettings.Database;
            if (db == null)
            {
                Print.MError("No LocalizationDatabase provided for code generation.");
                return false;
            }
            var file = GenerateFile(Namespace, db.Schema);
            string assetPath = AssetDatabase.GetAssetPath(db);
            string folderPath = Path.GetDirectoryName(assetPath);
            string path = $"{folderPath}/{ClassName}.g.cs";
            File.WriteAllText(path, file);
            AssetDatabase.Refresh();

            Print.MLog("Localization code generation complete.");
            return true;
        }
        private static IEnumerable<(string type, string name)> GetArgs(string[] args)
        {
            return args.Select(a =>
            {
                Print.MLog(a);
                var split = a.Split(':');
                var name = ToCamelCase(split[0]);
                var type = split.Length > 1 ? split[1] : "object"; // TODO
                return (type, name);
            });
        }

        private static string GenerateFile(string namespaceName, LocalizationSchema schema)
        {
            return FileTemplate
                .Replace("{NAMESPACE}", namespaceName)
                .Replace("{TABLES}", string.Join("\n\n", schema.Tables.Select(GenerateTable)));
        }

        private static string GenerateTable(LocalizationTableSchema schema)
        {
            return TableTemplate
                .Replace("{TABLE_NAME}", $"{ToPascalCase(schema.Id)}Table")
                .Replace("{KEY}", schema.Id)
                .Replace("{METHODS}", string.Join("\n", schema.Keys.SelectMany(k => GenerateMethods(schema.Id, k))));
        }

        private static List<string> GenerateMethods(string tableId, LocalizationKey key)
        {
            List<string> methods = new();
            var name = $"Get{ToPascalCase(key.Id)}Ref";
            var argDecl = string.Join(", ", key.Args.Select(a => $"{GetLocArgName(a.Type)} {a.Name}"));
            var argValues = key.Args.Count() > 0 ? ", " + string.Join(", ", key.Args.Select(a => a.Name)) : "";
            methods.Add(RefMethodTemplate
                .Replace("{TYPES}", $"LocalizedReference<{string.Join(", ", key.Types.Select(t => GetLocType(t, key.IsArray)))}>")
                .Replace("{METHOD_NAME}", name)
                .Replace("{TABLE_ID}", tableId)
                .Replace("{KEY}", key.Id)
                .Replace("{ARGS}", argDecl)
                .Replace("{ARG_VALUES}", argValues));
            foreach (var type in key.Types)
            {
                name = $"Get{ToPascalCase(key.Id)}{ToPascalCase(type)}";
                argDecl = string.Join(", ", key.Args.Select(a => $"{a.Type} {a.Name}"));
                methods.Add(ValMethodTemplate
                    .Replace("{TYPE}", GetLocType(type, key.IsArray))
                    .Replace("{METHOD_NAME}", name)
                    .Replace("{TABLE_ID}", tableId)
                    .Replace("{KEY}", key.Id)
                    .Replace("{ARGS}", argDecl)
                    .Replace("{ARG_VALUES}", argValues));
            }
            return methods;
        }
        private static string GetLocType(string type, bool isArray)
        {
            return LocalizationKey.AllowedTypes[type] + (isArray ? "[]" : "");
        }
        private static string GetLocArgName(string type)
        {
            return type switch
            {
                "string" => nameof(LocStringArg),
                "float" => nameof(LocFloatArg),
                "int" => nameof(LocIntArg),
                "bool" => nameof(LocBoolArg),
                _ => nameof(LocArg)
            };
        }
        private static string ToPascalCase(string input)
        {
            if (string.IsNullOrEmpty(input)) return "Unnamed";
            return string.Concat(
                input.Replace("-", "").Replace(".", "")
                    .Split('_', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => char.ToUpper(s[0]) + s[1..]));
        }
        private static string ToCamelCase(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "unnamed";
            var parts = input.Replace("-", "").Split('_', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return "unnamed";
            var first = parts[0].ToLowerInvariant();
            var rest = parts
                .Skip(1)
                .Select(p => char.ToUpperInvariant(p[0]) + p[1..]);
            return first + string.Concat(rest);
        }
    }
}