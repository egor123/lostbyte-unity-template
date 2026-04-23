using System;
using System.Collections.Generic;
using Lostbyte.Toolkit.CustomEditor;
using UnityEngine;

namespace Lostbyte.Toolkit.FactSystem.Persistance
{
    [Serializable]
    public class SaveSystem
    {
        [field: SerializeField] public bool Enabled { get; private set; } = false;
        [SerializeField, SerializeReference, UniqeReference] private ISaveFormatter m_formatter;
        [SerializeField, SerializeReference, UniqeReference] private ISaveStorage m_storage;

        [field: SerializeField] public bool AutoLoad { get; private set; } = false;
        [field: SerializeField] public bool SaveOnChange { get; private set; } = false;

        internal void Write(object data) => m_storage.Write(m_formatter, data);
        internal T Read<T>() => m_storage.Read<T>(m_formatter);
    }
}