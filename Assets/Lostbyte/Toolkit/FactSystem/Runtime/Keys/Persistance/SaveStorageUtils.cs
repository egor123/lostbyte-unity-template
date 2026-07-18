using System;
using System.IO;
using Lostbyte.Toolkit.Common;
using UnityEngine;

#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace Lostbyte.Toolkit.FactSystem.Persistance
{
    public static class SaveStorageUtils
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        // Imports the JavaScript function from .jslib plugin
        [DllImport("__Internal")]
        private static extern void SyncFiles();
#endif

        public static string GetFullPath(string path) => Path.Combine(Application.persistentDataPath, path);

        public static void Write(ISaveFormatter formatter, string savePath, string tempPath, object data)
        {
            try
            {
                using (var stream = File.Create(tempPath))
                {
                    formatter.Serialize(stream, data);
                }
                File.Copy(tempPath, savePath, overwrite: true);
                File.Delete(tempPath);
                Print.MLog($"Successfully saved: {savePath}");

#if UNITY_WEBGL && !UNITY_EDITOR
                SyncFiles();
#endif
            }
            catch (Exception e)
            {
                Print.MError($"Failed to save '{savePath}': {e.Message}");
            }
        }

        public static T Read<T>(ISaveFormatter formatter, string savePath, string tempPath)
        {
            if (!File.Exists(savePath) && !File.Exists(tempPath))
            {
                Print.MLog($"No saved files found: {savePath}");
                return default;
            }
            if (TryDeserializeFile(savePath, formatter, out T data))
            {
                Print.MLog($"Successfully loaded: {savePath}");
                return data;
            }
            if (TryDeserializeFile(tempPath, formatter, out data))
            {
                Print.MWarn($"Successfully recovered file: {tempPath}");
                return data;
            }
            return default;
        }

        public static bool Exists(string path)
        {
            return File.Exists(path);
        }

        public static void Delete(string savePath, string tempPath)
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
            if (File.Exists(savePath))
                File.Delete(savePath);

#if UNITY_WEBGL && !UNITY_EDITOR
            SyncFiles();
#endif
        }

        private static bool TryDeserializeFile<T>(string path, ISaveFormatter formatter, out T result)
        {
            result = default;
            if (!File.Exists(path))
                return false;
            try
            {
                using var stream = File.OpenRead(path);
                if (formatter.Deserialize(stream) is T deserialized)
                {
                    result = deserialized;
                    return true;
                }
                else
                {
                    Print.MWarn($"Unexpected format in file: {path}");
                }
            }
            catch (Exception ex)
            {
                Print.Exception(ex);
            }
            return false;
        }
    }
}