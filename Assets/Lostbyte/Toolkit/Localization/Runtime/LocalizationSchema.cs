using System;
using System.Collections;
using System.Collections.Generic;
using Lostbyte.Toolkit.CustomEditor;
using UnityEngine;

namespace Lostbyte.Toolkit.Localization
{
    [Serializable]
    public struct LocaleConfig
    {
        [field: SerializeField] public string SchemaVersion { get; private set; }
        [field: SerializeField] public string Fallback { get; private set; }
        [field: SerializeField] public string DisplayName { get; private set; }

        public LocaleConfig(string schemaVersion, string fallback, string displayName)
        {
            SchemaVersion = schemaVersion;
            Fallback = fallback;
            DisplayName = displayName;
        }
    }
    [Serializable]
    public struct LocalizationSchema
    {
        [SerializeField] private List<LocalizationTableSchema> m_tables;
        public readonly IReadOnlyList<LocalizationTableSchema> Tables => m_tables.AsReadOnly();

        public LocalizationSchema(List<LocalizationTableSchema> tables)
        {
            m_tables = tables ?? new();
        }
    }
    [Serializable]
    public struct LocalizationTableSchema
    {
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField, Hide] public string SchemaVersion { get; private set; }
        [field: SerializeField, TextArea, ShowIf(nameof(Meta))] public string Meta { get; private set; }

        [SerializeField] private List<LocalizationKey> m_keys;
        public readonly IReadOnlyList<LocalizationKey> Keys => m_keys?.AsReadOnly();

        public LocalizationTableSchema(string schemaVersion, string tableId, string meta, List<LocalizationKey> keys)
        {
            if (string.IsNullOrWhiteSpace(tableId)) throw new LocalizationException("Table id cannot be null or empty!");

            SchemaVersion = schemaVersion;
            Id = tableId;
            Meta = meta;
            m_keys = keys ?? new();
        }

    }
    [Serializable]
    public struct LocalizationKey
    {
        public static readonly IReadOnlyDictionary<string, Type> AllowedTypes = new Dictionary<string, Type>()
        {
            { "string", typeof(string) },
            { "audio", typeof(AudioClip) },
            { "file", typeof(TextAsset) },
            { "texture", typeof(Texture2D) },
        };
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField, ShowIf(nameof(Meta))] public string Meta { get; private set; }
        [SerializeField] private List<string> m_types;
        [SerializeField] private List<ArgumentDefinition> m_args;
        [field: SerializeField, ShowIf(nameof(IsArray))] public bool IsArray { get; private set; }

        public readonly IReadOnlyList<string> Types => m_types.AsReadOnly();
        public readonly IReadOnlyList<ArgumentDefinition> Args => m_args.AsReadOnly();

        public LocalizationKey(string id, string meta, List<string> types, List<ArgumentDefinition> args, bool isArray)
        {
            if (types == null || types.Count == 0)
                throw new LocalizationException("Key must have at least one type!");
            foreach (var type in types)
                if (!AllowedTypes.ContainsKey(type))
                    throw new LocalizationException($"Key type {type} is not supported, it must be: {string.Join(", ", AllowedTypes.Keys)}!");

            Id = id;
            Meta = meta;
            m_types = types ?? new();
            m_args = args ?? new();
            IsArray = isArray;
        }
    }
    [Serializable]
    public struct ArgumentDefinition
    {
        public static readonly IReadOnlyDictionary<string, Type> AllowedTypes = new Dictionary<string, Type>()
        {
            { "object", typeof(object) },
            { "string", typeof(string) },
            { "int", typeof(int) },
            { "float", typeof(float) },
            { "double", typeof(double) },
            { "bool", typeof(bool) }
        };
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public string Type { get; private set; }
        public readonly Type ArgType => AllowedTypes[Type];
        public ArgumentDefinition(string name, string type)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new LocalizationException("Argument name must not be null or empty!");
            if (!AllowedTypes.ContainsKey(type)) throw new LocalizationException($"Argument type {type} is not supported, it must be: {string.Join(", ", AllowedTypes.Keys)}!");
            Name = name;
            Type = type;
        }
    }
    [Serializable]
    public class LocalizationException : Exception
    {
        public LocalizationException(string message) : base(message) { }
    }
}