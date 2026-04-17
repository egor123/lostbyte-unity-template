using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
using UnityEngine;

namespace Lostbyte.Toolkit.SaveSystem
{
    public class SaveLoader : MonoBehaviour
    {
        [field: SerializeField] public string SaveFileName { get; set; } = "save";
        private const string k_saveExtension = ".bin";
        private const string k_tempExtension = ".tmp";
        private readonly List<IPersistent> _subscribers = new();
        private static SaveLoader _instance;
        public static SaveLoader Instance => _instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        static void OnRuntimeMethodLoad()
        {
            GameObject obj = new() { name = nameof(SaveLoader) };
            _instance = obj.AddComponent<SaveLoader>();
        }
        private Dictionary<string, object> _data = new();
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
        private static string GetFullPath(string path) => Path.Combine(Application.persistentDataPath, path);
        public void Subscribe(IPersistent persistent)
        {
            _subscribers.Add(persistent);
            if (_data != null) persistent.OnLoad();
        }
        public void Unsubscribe(IPersistent persistent) => _subscribers.Remove(persistent);
        public object GetData(string path)
        {
            if (_data != null)
            {
                string[] p = path.Split("/");
                Dictionary<string, object> data = _data;
                for (int i = 0; i < p.Length; i++)
                {
                    if (data.TryGetValue(p[i], out var obj))
                    {
                        if (i == p.Length - 1)
                        {
                            return obj;
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
            // Debug.LogWarning($"Cannot load from \"{path}\"");
            return null;
        }
        public void SetData(string path, object value)
        {
            string[] p = path.Split("/");
            Dictionary<string, object> data = _data;
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
            Debug.LogError($"Cannot save to \"{path}\"");
        }

        public void DeleteSave()
        {
            DeleteFile(SaveFileName);
        }
        public void Save()
        {
            _subscribers.ForEach(s => s.OnSave());
            SaveToFile(SaveFileName, _data);
        }
        public void Load()
        {
            _data = (Dictionary<string, object>)LoadFromFile(SaveFileName) ?? new();
            _subscribers.ForEach(s => s.OnLoad());
        }
        public void DeleteFile(string filePath)
        {
            string fullPath = GetFullPath(filePath);
            string tempPath = fullPath + k_tempExtension;
            string savePath = fullPath + k_saveExtension;
            if (File.Exists(tempPath))
                File.Delete(tempPath);
            if (File.Exists(savePath))
                File.Delete(savePath);
        }
        public object LoadFromFile(string filePath)
        {
            
            string fullPath = GetFullPath(filePath);
            string savePath = fullPath + k_saveExtension;
            string tempPath = fullPath + k_tempExtension;
            var formatter = Formatter.GetBinaryFormatter();
            if (!File.Exists(savePath) && !File.Exists(tempPath))
                return null;
            if (TryDeserializeFile(savePath, formatter, out Dictionary<string, object> data))
                return data;
            if (TryDeserializeFile(tempPath, formatter, out data))
                return data;
            return null;
        }

        private bool TryDeserializeFile(string path, IFormatter formatter, out Dictionary<string, object> result)
        {
            result = null;
            if (!File.Exists(path))
                return false;
            try
            {
                using var stream = File.OpenRead(path);
                if (formatter.Deserialize(stream) is Dictionary<string, object> deserialized)
                {
                    result = deserialized;
                    return true;
                }
                else
                {
                    Debug.LogWarning($"Unexpected format in file: {path}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            return false;
        }

        public void SaveToFile(string filePath, object data)
        {
            string fullPath = GetFullPath(filePath);
            string savePath = fullPath + k_saveExtension;
            string tempPath = fullPath + k_tempExtension;
            IFormatter formatter = Formatter.GetBinaryFormatter();
            try
            {
                using (var stream = File.Create(tempPath))
                {
                    formatter.Serialize(stream, data);
                }
                File.Copy(tempPath, savePath, overwrite: true);
                File.Delete(tempPath);

                Debug.Log($"Successfully saved: {savePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save '{savePath}': {e.Message}");
            }
        }

    }
}
