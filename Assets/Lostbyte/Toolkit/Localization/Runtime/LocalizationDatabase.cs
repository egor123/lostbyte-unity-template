using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lostbyte.Toolkit.Localization
{
    public class LocalizationDatabase : ScriptableObject
    {
        [SerializeField, SerializeReference] private List<LocalizedTable> m_tables = new();

        [System.Serializable]
        public class Entry<T>
        {
            public string Key;
            public T Value;
        }
        private readonly Dictionary<string, LocalizedTable> _tables = new();
        public IReadOnlyDictionary<string, LocalizedTable> Tables => _tables;
        public LocalizedTable CurrentTable => _tables[LocalizationSettings.Locale];

        public void OnBeforeSerialize()
        {
            m_tables.Clear();
            foreach (var pair in _tables)
                m_tables.Add(pair.Value);
        }

        public void OnAfterDeserialize()
        {
            _tables.Clear();
            for (int i = 0; i < Mathf.Min(m_tables.Count); i++)
                _tables[m_tables[i].name] = m_tables[i];
        }
    }
}
