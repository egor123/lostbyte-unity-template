using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lostbyte.Toolkit.Common;
using Lostbyte.Toolkit.CustomEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace Lostbyte.Toolkit.Localization
{
    [CreateAssetMenu(fileName = nameof(LocalizationDatabase), menuName = "Localization/Database")]
    public class LocalizationDatabase : ScriptableObject
    {
        [field: SerializeField, ReadOnly] public LocalizationSchema Schema { get; private set; }
        public string CurrentLocale { get; internal set; }
        private readonly Dictionary<string, LocalizedTable> m_activeTables = new();
        private AsyncOperationHandle<IList<LocalizedTable>> m_currentLoadHandle;
        public event Action<string> OnLocalizationChanged;
        internal string m_targetLocale;

        public void ChangeLocale(string newLocale) => ChangeLocaleAsync(newLocale).Forget();

        public async Task ChangeLocaleAsync(string newLocale)
        {
            if (!TryGetLocationsHandle(newLocale, out var locationsHandle)) return;
            await locationsHandle.Task;

            if (!TryGetTempHandle(locationsHandle, newLocale, out var tempAssetsHandle)) return;
            await tempAssetsHandle.Task;

            if (m_targetLocale != newLocale)
            {
                Addressables.Release(tempAssetsHandle);
                Addressables.Release(locationsHandle);
                return;
            }

            ReleaseCurrentLocale();
            SetLocalizationData(locationsHandle, tempAssetsHandle, newLocale);
        }

        public void ChangeLocaleSync(string newLocale)
        {
            if (!TryGetLocationsHandle(newLocale, out var locationsHandle)) return;
            locationsHandle.WaitForCompletion();

            if (!TryGetTempHandle(locationsHandle, newLocale, out var tempAssetsHandle)) return;
            tempAssetsHandle.WaitForCompletion();

            ReleaseCurrentLocale();
            SetLocalizationData(locationsHandle, tempAssetsHandle, newLocale);
        }
        private void ReleaseCurrentLocale()
        {
            foreach (var table in m_activeTables.Values)
                if (table != null)
                    table.Release();
            m_activeTables.Clear();
            if (m_currentLoadHandle.IsValid())
                Addressables.Release(m_currentLoadHandle);
        }
        private bool TryGetLocationsHandle(string newLocale, out AsyncOperationHandle<IList<IResourceLocation>> locationsHandle)
        {
            if (CurrentLocale == newLocale || string.IsNullOrEmpty(newLocale))
            {
                locationsHandle = default;
                return false;
            }
            m_targetLocale = newLocale;
            locationsHandle = Addressables.LoadResourceLocationsAsync($"LOCALE_{newLocale}", typeof(LocalizedTable));
            return true;
        }
        private bool TryGetTempHandle(AsyncOperationHandle<IList<IResourceLocation>> locationsHandle, string newLocale, out AsyncOperationHandle<IList<LocalizedTable>> tempAssetsHandle)
        {
            if (m_targetLocale != newLocale)
            {
                tempAssetsHandle = default;
                Addressables.Release(locationsHandle);
                return false;
            }
            if (locationsHandle.Status != AsyncOperationStatus.Succeeded || locationsHandle.Result.Count == 0)
            {
                tempAssetsHandle = default;
                Print.MError($"Found 0 tables for label '{newLocale}'. Are the tables properly labeled in Addressables?");
                Addressables.Release(locationsHandle);
                return false;
            }
            tempAssetsHandle = Addressables.LoadAssetsAsync<LocalizedTable>(locationsHandle.Result, null);
            return true;
        }
        private void SetLocalizationData(AsyncOperationHandle<IList<IResourceLocation>> locationsHandle, AsyncOperationHandle<IList<LocalizedTable>> tempAssetsHandle, string newLocale)
        {
            m_currentLoadHandle = tempAssetsHandle;
            if (m_currentLoadHandle.Status == AsyncOperationStatus.Succeeded)
            {
                CurrentLocale = newLocale;
                foreach (var table in m_currentLoadHandle.Result)
                    m_activeTables[table.TableId] = table;
                Print.MLog($"Successfully loaded {m_activeTables.Count} tables for {newLocale}.");
                OnLocalizationChanged?.Invoke(newLocale);
            }
            else
                Print.MError($"Failed to load tables for {newLocale}. Reason: {m_currentLoadHandle.OperationException}");
            Addressables.Release(locationsHandle);
        }

        public static LocalizedTable GetTable(string tableId)
        {
            var db = LocalizationSettings.Database;
            var found = db.m_activeTables.TryGetValue(tableId, out var table);
            Print.MAssert(found, $"Table '{tableId}' not found in locale '{db.CurrentLocale}'");
            return table;
        }
        public static T GetValue<T>(string tableId, string keyId, params object[] args)
        {
            var table = GetTable(tableId);
            if (table == null) return default;

            object res = null;

            // --- SINGLE ITEMS ---
            if (typeof(T) == typeof(string))
                res = table.StringEntries.TryGetValue(keyId, out var v) ? (args?.Length > 0 ? Formatter.Format(v, args) : v) : null;
            else if (typeof(T) == typeof(Texture))
                res = table.TextureEntries.TryGetValue(keyId, out var v) ? v.Asset != null ? v.Asset : v.LoadAssetAsync<Texture>().WaitForCompletion() : null;
            else if (typeof(T) == typeof(AudioClip))
                res = table.AudioEntries.TryGetValue(keyId, out var v) ? v.Asset != null ? v.Asset : v.LoadAssetAsync<AudioClip>().WaitForCompletion() : null;
            else if (typeof(T) == typeof(TextAsset))
                res = table.FileEntries.TryGetValue(keyId, out var v) ? v.Asset != null ? v.Asset : v.LoadAssetAsync<TextAsset>().WaitForCompletion() : null;

            // --- ARRAYS ---
            else if (typeof(T) == typeof(string[]))
                res = table.StringArrayEntries.TryGetValue(keyId, out var v) ? (args?.Length > 0 ? v.Select(s => Formatter.Format(s, args)).ToArray() : v.ToArray()) : null;
            else if (typeof(T) == typeof(Texture[]))
                res = table.TextureArrayEntries.TryGetValue(keyId, out var v) ? v.Select(r => r.Asset as Texture != null ? r.Asset : r.LoadAssetAsync<Texture>().WaitForCompletion()).ToArray() : null;
            else if (typeof(T) == typeof(AudioClip[]))
                res = table.AudioArrayEntries.TryGetValue(keyId, out var v) ? v.Select(r => r.Asset as AudioClip != null ? r.Asset : r.LoadAssetAsync<AudioClip>().WaitForCompletion()).ToArray() : null;
            else if (typeof(T) == typeof(TextAsset[]))
                res = table.FileArrayEntries.TryGetValue(keyId, out var v) ? v.Select(r => r.Asset as TextAsset != null ? r.Asset : r.LoadAssetAsync<TextAsset>().WaitForCompletion()).ToArray() : null;

            else
                Print.MWarn($"Unsupported type: {typeof(T)}");

            return res is T finalResult ? finalResult : default;
        }

        public static async Task<T> GetValueAsync<T>(string tableId, string keyId, params object[] args)
        {
            var table = GetTable(tableId);
            if (table == null) return default;

            object res = null;

            // --- SINGLE ITEMS ---
            if (typeof(T) == typeof(string))
                res = table.StringEntries.TryGetValue(keyId, out var v) ? (args?.Length > 0 ? Formatter.Format(v, args) : v) : null;
            else if (typeof(T) == typeof(Texture))
                res = table.TextureEntries.TryGetValue(keyId, out var v) ? (v.Asset as Texture != null ? v.Asset : await v.LoadAssetAsync<Texture>().Task) : null;
            else if (typeof(T) == typeof(AudioClip))
                res = table.AudioEntries.TryGetValue(keyId, out var v) ? (v.Asset as AudioClip != null ? v.Asset : await v.LoadAssetAsync<AudioClip>().Task) : null;
            else if (typeof(T) == typeof(TextAsset))
                res = table.FileEntries.TryGetValue(keyId, out var v) ? (v.Asset as TextAsset != null ? v.Asset : await v.LoadAssetAsync<TextAsset>().Task) : null;

            // --- ARRAYS ---
            else if (typeof(T) == typeof(string[]))
                res = table.StringArrayEntries.TryGetValue(keyId, out var v) ? (args?.Length > 0 ? v.Select(s => Formatter.Format(s, args)).ToArray() : v.ToArray()) : null;
            else if (typeof(T) == typeof(Texture[]))
                res = table.TextureArrayEntries.TryGetValue(keyId, out var v) ? await Task.WhenAll(v.Select(async r => r.Asset as Texture != null ? r.Asset : await r.LoadAssetAsync<Texture>().Task)) : null;
            else if (typeof(T) == typeof(AudioClip[]))
                res = table.AudioArrayEntries.TryGetValue(keyId, out var v) ? await Task.WhenAll(v.Select(async r => r.Asset as AudioClip != null ? r.Asset : await r.LoadAssetAsync<AudioClip>().Task)) : null;
            else if (typeof(T) == typeof(TextAsset[]))
                res = table.FileArrayEntries.TryGetValue(keyId, out var v) ? await Task.WhenAll(v.Select(async r => r.Asset as TextAsset != null ? r.Asset : await r.LoadAssetAsync<TextAsset>().Task)) : null;

            else
                Print.MWarn($"Unsupported type: {typeof(T)}");

            return res is T finalResult ? finalResult : default;
        }
    }
}
