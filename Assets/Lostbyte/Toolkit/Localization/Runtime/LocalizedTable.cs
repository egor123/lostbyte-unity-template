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
        [field: SerializeField, ReadOnly] private List<SerializedKeyValuePair<string, AssetReference>> m_addressableEnries = new();
        [field: SerializeField, ReadOnly] private List<SerializedKeyValuePair<string, AssetReference[]>> m_addressableArrayEnries = new();

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

            foreach ((var key, var value) in m_addressableEnries)
            {
                if (value == null || string.IsNullOrEmpty(value.AssetGUID)) continue;
                if (value is AssetReferenceT<TextAsset> fRef) fileDict[key] = fRef;
                else if (value is AssetReferenceT<Texture> tRef) textureDict[key] = tRef;
                else if (value is AssetReferenceT<AudioClip> aRef) audioDict[key] = aRef;
                else Print.Error($"Deserialization error: unkonw type {value.GetType()} for key {key} in {Locale}/{TableId}");
            }

            FileEntries = fileDict;
            AudioEntries = audioDict;
            TextureEntries = textureDict;

            var fileArrayDict = new Dictionary<string, IReadOnlyList<AssetReferenceT<TextAsset>>>();
            var audioArrayDict = new Dictionary<string, IReadOnlyList<AssetReferenceT<AudioClip>>>();
            var textureArrayDict = new Dictionary<string, IReadOnlyList<AssetReferenceT<Texture>>>();

            foreach ((var key, var value) in m_addressableArrayEnries)
            {
                int length = value.Length;
                if (key == null || length == 0) continue;
                if (value[0] is AssetReferenceT<TextAsset>)
                {
                    var textArr = new AssetReferenceT<TextAsset>[length];
                    for (int i = 0; i < length; i++)
                        textArr[i] = (AssetReferenceT<TextAsset>)value[i];
                    fileArrayDict[key] = textArr;
                }
                else if (value[0] is AssetReferenceT<Texture>)
                {
                    var audioArr = new AssetReferenceT<AudioClip>[length];
                    for (int i = 0; i < length; i++)
                        audioArr[i] = (AssetReferenceT<AudioClip>)value[i];
                    audioArrayDict[key] = audioArr;
                }
                else if (value[0] is AssetReferenceT<AudioClip>)
                {
                    var texArr = new AssetReferenceT<Texture>[length];
                    for (int i = 0; i < length; i++)
                        texArr[i] = (AssetReferenceT<Texture>)value[i];
                    textureArrayDict[key] = texArr;
                }
                else Print.Error($"Deserialization error: unkonw asset {value[0]} for key {key} in {Locale}/{TableId}", this);
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
