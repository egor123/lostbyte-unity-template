using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lostbyte.Toolkit.Localization
{
    public class LocalizedTable : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField] private List<StringEntry> m_entries = new();
        [SerializeField] private LocalizedTable m_fallback;
        private readonly Dictionary<string, string> _stringStorage = new();

        [System.Serializable]
        public class StringEntry
        {
            public string Key;
            public string Value;
        }
        public string GetString(string key, params object[] args)
        {
            if (!_stringStorage.TryGetValue(key, out var str))
                return m_fallback.GetString(key, args);
            if (args == null || args.Length == 0)
                return str;
            return Formatter.Format(str, args);
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
                if (m_entries[i] != null && !string.IsNullOrEmpty(m_entries[i].Key))
                    _stringStorage[m_entries[i].Key] = m_entries[i].Value;
        }

    }
}
