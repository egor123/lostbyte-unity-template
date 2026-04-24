using System;
using System.Collections.Generic;
using Lostbyte.Toolkit.Common;
using Lostbyte.Toolkit.FactSystem;
using Lostbyte.Toolkit.Management;
using UnityEngine;
using UnityEngine.Audio;

namespace Lostbyte.Toolkit.Audio
{
    public class AudioManager : Manager<AudioManager>
    {
        [Serializable]
        private struct MixerGroup
        {
            public string Name;
            public FactWrapper<float> Fact;
        }

        [SerializeField] private AudioMixer m_mixer;
        [SerializeField] private List<MixerGroup> m_groups = new();
        private readonly SubscriptionGroup _subscriptions = new();
        private const float MIN_LINEAR = 0.0001f;
        private const float MIN_DB = -80f; // typical Unity silence

        private void OnEnable()
        {
            if (Avalible)
                foreach (var group in m_groups)
                    _subscriptions.Subscribe(group.Fact.Subscribe, group.Fact.Unsubscribe, (float v) => SetVolume(v, group.Name));

        }
        private void Start()
        {
            if (Avalible)
                foreach (var group in m_groups)
                    SetVolume(group.Fact.Value, group.Name);

        }
        private void SetVolume(float value, string name)
        {
            float dB = LinearToDecibel(value);
            m_mixer.SetFloat(name, dB);
        }
        public static float LinearToDecibel(float linear)
        {
            linear = Mathf.Clamp(linear, MIN_LINEAR, 1f);
            return Mathf.Log10(linear) * 20f;
        }

        public static float DecibelToLinear(float dB)
        {
            if (dB <= MIN_DB)
                return 0f;
            return Mathf.Pow(10f, dB / 20f);
        }

        private void OnDisable()
        {
            _subscriptions.Dispose();
        }
    }
}
