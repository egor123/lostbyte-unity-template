using System;
using System.Collections;
using System.Collections.Generic;
using Lostbyte.Toolkit.CustomEditor;
using UnityEngine;

namespace Lostbyte.Toolkit.FactSystem.Persistance
{
    [Serializable]
    public class SaveSystem
    {
        [field: SerializeField] public bool Enabled { get; private set; } = false;
        [SerializeField, SerializeReference, UniqueReference] private ISaveFormatter m_formatter;
        [SerializeField, SerializeReference, UniqueReference] private ISaveStorage m_storage;

        [field: SerializeField] public bool AutoLoad { get; private set; } = false;
        [field: SerializeField] public bool SaveOnChange { get; private set; } = false;

        public void Write(object data) => m_storage.Write(m_formatter, data);
        public T Read<T>() => m_storage.Read<T>(m_formatter);
        public void Delete() => m_storage?.Delete();
        
        public static void Print(object value, int indent = 0)
        {
            string pad = new(' ', indent * 2);

            switch (value)
            {
                case Dictionary<string, object> dict:
                    foreach (var kv in dict)
                    {
                        if (IsContainer(kv.Value))
                        {
                            Debug.Log($"{pad}{kv.Key}:");
                            Print(kv.Value, indent + 1);
                        }
                        else
                        {
                            Debug.Log($"{pad}{kv.Key}: {FormatValue(kv.Value)}");
                        }
                    }
                    break;

                case IList list:
                    for (int i = 0; i < list.Count; i++)
                    {
                        var item = list[i];
                        if (IsContainer(item))
                        {
                            Debug.Log($"{pad}[{i}]:");
                            Print(item, indent + 1);
                        }
                        else
                        {
                            Debug.Log($"{pad}[{i}]: {FormatValue(item)}");
                        }
                    }
                    break;

                default:
                    Debug.Log($"{pad}{FormatValue(value)}");
                    break;
            }
        }
        private static bool IsContainer(object obj) =>
            obj is Dictionary<string, object> || obj is IList;

        private static string FormatValue(object obj) =>
            obj?.ToString() ?? "null";

    }
}