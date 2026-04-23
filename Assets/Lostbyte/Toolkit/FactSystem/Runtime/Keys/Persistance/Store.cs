using System.Collections.Generic;
using UnityEngine;

namespace Lostbyte.Toolkit.FactSystem.Persistance
{
    public class Store
    {
        private Dictionary<string, object> _store = new();
        private readonly List<IPersistent> _persistents = new();

        internal void SetStore(Dictionary<string, object> store) => _store = store;
        internal Dictionary<string, object> GetStore() => _store;
        internal bool IsEmpty => _store == null || _store.Count == 0;
        internal void OnLoad() => _persistents.ForEach(p => p.OnLoad(this));
        internal void OnSave() => _persistents.ForEach(p => p.OnSave(this));
        internal void Subscribe(IPersistent persistent)
        {
            _persistents.Add(persistent);
            persistent.OnLoad(this);
        }
        internal void Unsubscribe(IPersistent persistent) => _persistents.Remove(persistent);

        public T GetData<T>(string path, T @default = default)
        {
            if (_store != null)
            {
                string[] p = path.Split("/");
                Dictionary<string, object> data = _store;
                for (int i = 0; i < p.Length; i++)
                {
                    if (data.TryGetValue(p[i], out var obj))
                    {
                        if (i == p.Length - 1)
                        {
                            return obj is T v ? v : @default;
                        }
                        else if (obj is Dictionary<string, object> dict)
                        {
                            data = dict;
                            continue;
                        }
                    }
                    break;
                }
            }
            return @default;
        }

        public void SetData<T>(string path, T value)
        {
            string[] p = path.Split("/");
            _store ??= new();
            Dictionary<string, object> data = _store;
            for (int i = 0; i < p.Length; i++)
            {
                if (i == p.Length - 1)
                {
                    data[p[^1]] = value;
                    return;
                }
                if (data.TryGetValue(p[i], out var obj))
                {
                    if (obj is Dictionary<string, object> dict)
                    {
                        data = dict;
                        continue;
                    }
                }
                else
                {
                    var dict = new Dictionary<string, object>();
                    data[p[i]] = dict;
                    data = dict;
                    continue;
                }
                break;
            }
            Debug.LogError($"Cannot set data to \"{path}\"");
        }
    }
}