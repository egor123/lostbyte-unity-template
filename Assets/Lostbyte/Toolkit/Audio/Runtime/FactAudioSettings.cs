using System;
using System.Collections.Generic;
using Lostbyte.Toolkit.CustomEditor;
using Lostbyte.Toolkit.FactSystem;
using UnityEngine;

namespace Lostbyte.Toolkit.Audio
{
    [CreateAssetMenu(fileName = nameof(FactAudioSettings), menuName = "Facts/Audio/FactAudioSettings")]
    public class FactAudioSettings : ScriptableObject
    {
        public List<AudioSettings> Settings;
    }
    [Serializable]
    public struct AudioSettings
    {
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public AudioClip[] Clips { get; private set; }
        [field: SerializeField] public int Priority { get; private set; }
        [field: SerializeField] public bool StopActive { get; private set; }
        [field: SerializeField] public bool AllowMultiple { get; private set; }
        [field: SerializeField, Range(0, 1)] public float Volume { get; private set; }// = 1;
        [field: SerializeField, Range(0, 1)] public float VolumeRandomization { get; private set; } //= 0;
        [field: SerializeField, Range(0, 3)] public float Pitch { get; private set; }// = 1;
        [field: SerializeField, Range(0, 1)] public float PitchRandomization { get; private set; } //= 0;
        public readonly AudioSettingsRunner Create(KeyContainer key, FactAudio audio) => new(key, audio, this);
        [field: SerializeField, SerializeReference, UniqeReference] public IAudioTrigger Trigger { get; private set; }
    }
    public interface IAudioTrigger
    {
        IAudioTriggerRunner CompileRunner(KeyContainer key);
    }
    [Serializable]
    public class Trigger : IAudioTrigger
    {
        [field: SerializeField] public Definition Definition { get; private set; }

        public IAudioTriggerRunner CompileRunner(KeyContainer key) => new Runner() { Key = key, Definition = Definition };
        public class Runner : IAudioTriggerRunner
        {
            public KeyContainer Key;
            public Definition Definition;
            public void Enable(KeyContainer key, Action callback) => Key.GetWrapper(Definition).Subscribe(callback);
            public void Disable(KeyContainer key, Action callback) => Key.GetWrapper(Definition).Unsubscribe(callback);
        }
    }
    [Serializable]
    public class ConditionTrigger : IAudioTrigger
    {
        [field: SerializeField] public Condition Condition { get; private set; }
        public IAudioTriggerRunner CompileRunner(KeyContainer key)
        {
            Condition.SetDefaultKey(key);
            return new Runner() { Condition = Condition };
        }
        public class Runner : IAudioTriggerRunner
        {
            public Condition Condition;
            public void Enable(KeyContainer key, Action callback) => Condition.Subscribe(callback);
            public void Disable(KeyContainer key, Action callback) => Condition.Unsubscribe(callback);
        }
    }
    public interface IAudioTriggerRunner
    {
        void Enable(KeyContainer key, Action callback);
        void Disable(KeyContainer key, Action callback);
    }
    public class AudioSettingsRunner
    {
        private readonly KeyContainer _key;
        private readonly FactAudio _audio;
        private readonly AudioSettings _settings;
        private readonly IAudioTriggerRunner _trigger;
        public AudioSettingsRunner(KeyContainer key, FactAudio audio, AudioSettings settings) => (_key, _audio, _settings, _trigger) = (key, audio, settings, settings.Trigger?.CompileRunner(key));
        public void Play() => _audio.Play(_settings);
        public void Enable() => _trigger?.Enable(_key, Play);
        public void Disable() => _trigger?.Disable(_key, Play);
    }
}
