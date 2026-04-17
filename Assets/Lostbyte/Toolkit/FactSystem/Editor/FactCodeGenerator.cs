using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace Lostbyte.Toolkit.FactSystem.Editor
{
    public static class FactCodeGenerator
    {
        // private const string OutputPath = "Assets/Scripts/Generated/GameFacts.g.cs";
        private const string ClassName = "GameFacts";
        private const string Namespace = "GameFacts";
        private const string KeysClassName = "Keys";
        private const string FactsClassName = "Facts";
        private const string EventsClassName = "Events";
        private const string EnumsClassName = "Enums";

        public static void Generate(FactDatabase db)
        {
            if (db == null)
            {
                Debug.LogError("No FactDatabase provided for code generation.");
                return;
            }
            Dictionary<string, List<string>> enums = new();

            StringBuilder sb = new();
            sb.AppendLine("// AUTO-GENERATED FILE — DO NOT EDIT");
            sb.AppendLine("using System;");
            sb.AppendLine("using Lostbyte.Toolkit.FactSystem;");
            sb.AppendLine($"namespace {Namespace}");
            sb.AppendLine("{");
            // -------------------------------------------------------
            sb.AppendLine($"    //------------- Key Refs -------------");
            sb.AppendLine($"    public static class {KeysClassName}");
            sb.AppendLine("    {");
            foreach (var key in db.RootKeys)
            {
                GenerateKeyRef(sb, key, "        ");
            }
            sb.AppendLine("    }");
            // -------------------------------------------------------
            sb.AppendLine($"    //------------- Fact Refs -------------");
            sb.AppendLine($"    public static class {FactsClassName}");
            sb.AppendLine("    {");
            foreach (var fact in db.FactStorage)
            {
                GenerateFactRef(sb, fact, enums, "        ");
            }
            sb.AppendLine("    }");
            // -------------------------------------------------------
            sb.AppendLine($"    //------------- Events -------------");
            sb.AppendLine($"    public static class {EventsClassName}");
            sb.AppendLine("    {");
            foreach (var @event in db.EventStorage)
            {
                GenerateEventRef(sb, @event, "        ");
            }
            sb.AppendLine("    }");
            // -------------------------------------------------------
            sb.AppendLine($"    //------------- Enums -------------");
            sb.AppendLine($"    public static class {EnumsClassName}");
            sb.AppendLine("    {");
            foreach (var pair in enums)
            {
                GenerateEnum(sb, pair.Key, pair.Value, "        ");
            }
            sb.AppendLine("    }");
            // -------------------------------------------------------
            sb.AppendLine("}");

            string assetPath = AssetDatabase.GetAssetPath(FactEditorUtils.Database);
            string folderPath = Path.GetDirectoryName(assetPath);
            string path = $"{folderPath}/{ClassName}.g.cs";
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, sb.ToString());
            AssetDatabase.Refresh();
            Debug.Log("Fact code generation complete.");
        }

        private static void GenerateKeyRef(StringBuilder sb, KeyContainer key, string indent)
        {
            string publicName = FactUtils.MakeSafeIdentifier(key.name);
            string privateName = '_' + publicName;
            string id = key.Guid;
            sb.AppendLine($"{indent}private static {nameof(KeyContainer)} {privateName} = null;");
            sb.AppendLine($"{indent}public static {nameof(KeyContainer)} {publicName} = {privateName} != null ? {privateName} : {privateName} = {nameof(FactDatabase)}.{nameof(FactDatabase.Instance)}.{nameof(FactDatabase.Instance.GetKey)}(\"{id}\");");
            foreach (var child in key.Children) GenerateKeyRef(sb, child, indent);
        }

        private static void GenerateFactRef(StringBuilder sb, FactDefinition fact, Dictionary<string, List<string>> enums, string indent)
        {
            if (fact is EnumFactDefinition eFact) enums[FactUtils.MakeSafeIdentifier(eFact.name)] = eFact.Values;
            string publicName = FactUtils.MakeSafeIdentifier(fact.name);
            string privateName = '_' + publicName;
            string type = $"{nameof(FactDefinition)}<{fact.GenericType}>";
            string id = fact.Guid;
            sb.AppendLine($"{indent}private static {type} {privateName} = null;");
            sb.AppendLine($"{indent}public static {type} {publicName} = {privateName} != null ? {privateName} : {privateName} = ({type}) {nameof(FactDatabase)}.{nameof(FactDatabase.Instance)}.{nameof(FactDatabase.Instance.GetFact)}(\"{id}\");");
        }
        private static void GenerateEventRef(StringBuilder sb, EventDefinition @event, string indent)
        {
            string publicName = FactUtils.MakeSafeIdentifier(@event.name);
            string privateName = '_' + publicName;
            string type = nameof(EventDefinition);
            string id = @event.Guid;
            sb.AppendLine($"{indent}private static {type} {privateName} = null;");
            sb.AppendLine($"{indent}public static {type} {publicName} = {privateName} != null ? {privateName} : {privateName} = ({type}) {nameof(FactDatabase)}.{nameof(FactDatabase.Instance)}.{nameof(FactDatabase.Instance.GetEvent)}(\"{id}\");");
        }

        private static void GenerateEnum(StringBuilder sb, string name, List<string> values, string indent)
        {
            string type = FactUtils.MakeSafeIdentifier(name);
            if (string.IsNullOrEmpty(type)) Debug.LogWarning("Enum type must have a name!");
            else if (values.Count == 0) Debug.LogWarning("Enum type must at least one value!");
            else sb.AppendLine($"{indent}public enum {type} {{ {string.Join(", ", values.Select(FactUtils.MakeSafeIdentifier))} }}");
        }
    }
}