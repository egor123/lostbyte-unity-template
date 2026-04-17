using System;
using System.Collections;
using System.Collections.Generic;
using Lostbyte.Toolkit.FactSystem;
using UnityEngine;

namespace Lostbyte.Toolkit.Audio
{
    [CreateAssetMenu(fileName = nameof(MusicSettings), menuName = "Audio/MusicSettings")]
    public class MusicSettings : ScriptableObject
    {
        [field: SerializeField] public AnimationCurve FadeInCurve { get; private set; } = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [field: SerializeField] public AnimationCurve FadeOutCurve { get; private set; } = AnimationCurve.EaseInOut(0, 1, 1, 0);

        [field: SerializeField] public List<MusicSettingsGroup> Groups { get; private set; } = new();
    }
    [Serializable]
    public class MusicSettingsGroup
    {
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public AudioClip[] Clips { get; private set; }
        [field: SerializeField] public AudioClip[] TransitionsOut { get; private set; }
        [field: SerializeField, Range(0, 1)] public float Volume { get; private set; } = 1;
        [field: SerializeField, Range(0, 5)] public float FadeInDuration { get; private set; } = 1;
        [field: SerializeField, Range(0, 5)] public float FadeOutDuration { get; private set; } = 1;
        [field: SerializeField, Range(0, 60)] public float MinPlayTime { get; private set; } = 10f;
        [field: SerializeField] public Condition Condition { get; private set; }
    }
    public class MusicGroupRunner
    {
        public readonly MusicSettingsGroup Group;
        public bool IsReady { get; private set; }
        private readonly Condition _condition;
        public MusicGroupRunner(MusicSettingsGroup group, IKeyContainer key)
        {
            Group = group;
            _condition = group.Condition.Copy();
            _condition.SetDefaultKey(key);
        }
        public void Enable()
        {
            if (_condition == null)
            {
                IsReady = true;
            }
            else
            {
                _condition?.Subscribe(SetValue);
                SetValue(_condition.IsMet);
            }
        }
        public void Disable()
        {
            _condition?.Unsubscribe(SetValue);
        }
        private void SetValue(bool value)
        {
            IsReady = value;
        }
    }
}
