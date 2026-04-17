using System.Collections;
using System.Collections.Generic;
using Codice.CM.Client.Differences.Graphic;
using UnityEngine;

namespace Lostbyte.Toolkit.Localization
{
    public class LocalizedTable : ScriptableObject
    {
        [SerializeField, SerializeReference] private List<StringEntry> m_entries = new();
        [SerializeField] private LocalizedTable m_fallback;
        private readonly Dictionary<string, string> _stringStorage = new();

        [System.Serializable]
        public class StringEntry
        {
            public string Key;
            public string Value;
        }

        public void OnBeforeSerialize()
        {
            m_entries.Clear();
            foreach (var pair in _stringStorage)
                m_entries.Add(new() { Key = pair.Key, Value = pair.Value });
        }

        public void OnAfterDeserialize()
        {
            _stringStorage.Clear();
            for (int i = 0; i < Mathf.Min(m_entries.Count); i++)
                _stringStorage[m_entries[i].Key] = m_entries[i].Value;
        }

        public string GetString(string key) => _stringStorage[key];
        public string GetString(string key, params object[] args) => Formatter.Format(_stringStorage[key], args);
    }
}
