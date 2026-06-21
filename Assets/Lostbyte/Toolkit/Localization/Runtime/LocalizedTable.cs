using System.Collections;
using System.Collections.Generic;
using Lostbyte.Toolkit.Common;
using Lostbyte.Toolkit.CustomEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Lostbyte.Toolkit.Localization
{
    public class LocalizedTable : ScriptableObject, ISerializationCallbackReceiver
    {
        [field: SerializeField, ReadOnly] public string Locale { get; private set; }
        [field: SerializeField, ReadOnly] public string TableId { get; private set; }

        [field: SerializeField, ReadOnly] private List<SerializedKeyValuePair<string, string>> m_stringEntries = new();
        [field: SerializeField, ReadOnly] private List<SerializedKeyValuePair<string, string[]>> m_stringArrayEntries = new();
        [field: SerializeField, ReadOnly] private List<LocalizedAssetEntry> m_addressableEnries = new();
        [field: SerializeField, ReadOnly] private List<LocalizedAssetEntries> m_addressableArrayEnries = new();

        [System.Serializable]
        public struct LocalizedAssetEntry
        {
            public string Key;
            public string AssetType;
            public AssetReference Reference;
        }
        [System.Serializable]
        public struct LocalizedAssetEntries
        {
            public string Key;
            public string AssetType;
            public AssetReference[] References;
        }
        public IReadOnlyDictionary<string, string> StringEntries { get; private set; }
        public IReadOnlyDictionary<string, IReadOnlyList<string>> StringArrayEntries { get; private set; }
        public IReadOnlyDictionary<string, AssetReferenceT<TextAsset>> FileEntries { get; private set; }
        public IReadOnlyDictionary<string, IReadOnlyList<AssetReferenceT<TextAsset>>> FileArrayEntries { get; private set; }
        public IReadOnlyDictionary<string, AssetReferenceT<AudioClip>> AudioEntries { get; private set; }
        public IReadOnlyDictionary<string, IReadOnlyList<AssetReferenceT<AudioClip>>> AudioArrayEntries { get; private set; }
        public IReadOnlyDictionary<string, AssetReferenceT<Texture>> TextureEntries { get; private set; }
        public IReadOnlyDictionary<string, IReadOnlyList<AssetReferenceT<Texture>>> TextureArrayEntries { get; private set; }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            SetupStringDictionaries();
            SetupAddressableDictionaries();
        }

        private void SetupStringDictionaries()
        {
            var stringDict = new Dictionary<string, string>();
            foreach ((var key, var value) in m_stringEntries)
                stringDict[key] = value;
            StringEntries = stringDict;

            var stringArrayDict = new Dictionary<string, IReadOnlyList<string>>();
            foreach ((var key, var value) in m_stringArrayEntries)
                stringArrayDict[key] = value;
            StringArrayEntries = stringArrayDict;
        }
        private void SetupAddressableDictionaries()
        {
            var fileDict = new Dictionary<string, AssetReferenceT<TextAsset>>();
            var audioDict = new Dictionary<string, AssetReferenceT<AudioClip>>();
            var textureDict = new Dictionary<string, AssetReferenceT<Texture>>();

            foreach (var entry in m_addressableEnries)
            {
                string guid = entry.Reference.AssetGUID;
                switch (entry.AssetType.ToLower())
                {
                    case "file":
                        fileDict[entry.Key] = new AssetReferenceT<TextAsset>(guid);
                        break;
                    case "texture":
                        textureDict[entry.Key] = new AssetReferenceT<Texture>(guid);
                        break;
                    case "audio":
                        audioDict[entry.Key] = new AssetReferenceT<AudioClip>(guid);
                        break;
                    default:
                        Print.Error($"Deserialization error: unknown type '{entry.AssetType}' for key '{entry.Key}' in {Locale}/{TableId}");
                        break;
                }
            }

            FileEntries = fileDict;
            AudioEntries = audioDict;
            TextureEntries = textureDict;

            var fileArrayDict = new Dictionary<string, IReadOnlyList<AssetReferenceT<TextAsset>>>();
            var audioArrayDict = new Dictionary<string, IReadOnlyList<AssetReferenceT<AudioClip>>>();
            var textureArrayDict = new Dictionary<string, IReadOnlyList<AssetReferenceT<Texture>>>();

            foreach (var entries in m_addressableArrayEnries)
            {
                int length = entries.References.Length;
                if (entries.Key == null || length == 0) continue;
                switch (entries.AssetType.ToLower())
                {
                    case "file":
                        var textArr = new AssetReferenceT<TextAsset>[length];
                        for (int i = 0; i < length; i++)
                            textArr[i] = new AssetReferenceT<TextAsset>(entries.References[i].AssetGUID);
                        fileArrayDict[entries.Key] = textArr;
                        break;
                    case "texture":
                        var audioArr = new AssetReferenceT<AudioClip>[length];
                        for (int i = 0; i < length; i++)
                            audioArr[i] = new AssetReferenceT<AudioClip>(entries.References[i].AssetGUID);
                        audioArrayDict[entries.Key] = audioArr;
                        break;
                    case "audio":
                        var texArr = new AssetReferenceT<Texture>[length];
                        for (int i = 0; i < length; i++)
                            texArr[i] = new AssetReferenceT<Texture>(entries.References[i].AssetGUID);
                        textureArrayDict[entries.Key] = texArr;
                        break;
                    default:
                        Print.Error($"Deserialization error: unknown type '{entries.AssetType}' for key '{entries.Key}' in {Locale}/{TableId}");
                        break;
                }
            }

            FileArrayEntries = fileArrayDict;
            AudioArrayEntries = audioArrayDict;
            TextureArrayEntries = textureArrayDict;
        }
        internal void Release()
        {
            foreach (var vRef in TextureEntries.Values)
                if (vRef.IsValid()) vRef.ReleaseAsset();
            foreach (var vRef in AudioEntries.Values)
                if (vRef.IsValid()) vRef.ReleaseAsset();
            foreach (var vRef in FileEntries.Values)
                if (vRef.IsValid()) vRef.ReleaseAsset();
            foreach (var arrRef in TextureArrayEntries.Values)
                foreach (var vRef in arrRef)
                    if (vRef.IsValid()) vRef.ReleaseAsset();
            foreach (var arrRef in AudioArrayEntries.Values)
                foreach (var vRef in arrRef)
                    if (vRef.IsValid()) vRef.ReleaseAsset();
            foreach (var arrRef in FileArrayEntries.Values)
                foreach (var vRef in arrRef)
                    if (vRef.IsValid()) vRef.ReleaseAsset();
        }
    }
}
