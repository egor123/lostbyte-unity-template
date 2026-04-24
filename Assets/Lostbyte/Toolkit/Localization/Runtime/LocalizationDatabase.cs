using System;
using System.Collections;
using System.Collections.Generic;
using Lostbyte.Toolkit.Common;
using Lostbyte.Toolkit.CustomEditor;
using UnityEngine;

namespace Lostbyte.Toolkit.Localization
{
    public class LocalizationDatabase : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField, ReadOnly] private List<SourceFile> m_sourceItems = new();
        [SerializeField, ReadOnly] private List<SerializedTuple<string, LocalizedTable>> m_tables = new();

        [Serializable]
        public struct SourceFile
        {
            public string Name;
            public SourceItem[] keys;
            [Serializable]
            public struct SourceItem
            {
                public string id;
                public string meta;
                public string[] args;
            }
        }
        private readonly Dictionary<string, LocalizedTable> _tables = new();

        public LocalizedTable GetTable(string tableName) => _tables.TryGetValue($"{tableName}_{LocalizationSettings.LocaleName}", out var val) ? val : null;

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            _tables.Clear();
            for (int i = 0; i < Mathf.Min(m_tables.Count); i++)
                if (m_tables[i] != null && !string.IsNullOrEmpty(m_tables[i].Item1))
                    _tables[m_tables[i].Item1] = m_tables[i].Item2;

        }
    }
}
