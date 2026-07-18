using System;
using Lostbyte.Toolkit.Common;
using Lostbyte.Toolkit.CustomEditor;
using Lostbyte.Toolkit.FactSystem;
using UnityEngine;
using UnityEngine.Audio;

namespace Lostbyte.Toolkit.Audio
{
    [Tag("Audio")]
    [SupportedFactTypes(typeof(float))]
    public class AudioMixerReaction : FactReaction
    {
        public AudioMixer Mixer;
        public float MaxValue = 100f;
        public string Group = "master";
        public VolumeMapping MappingMode = VolumeMapping.LinearToDecibel;

        public enum VolumeMapping { LinearToDecibel, RawDecibel }

        private const float MIN_LINEAR = 0.0001f;
        private const float MIN_DB = -80f;

        public override FactReaction Copy() => new AudioMixerReaction
        {
            Mixer = Mixer,
            Group = Group,
            MappingMode = MappingMode
        };

        public override void OnLoad(object data) => OnValueChanged(null, Value);

        protected override void OnValueChanged(object oldValue, object newValue)
        {
            if (Mixer == null || string.IsNullOrEmpty(Group)) return;
            float dB = MappingMode == VolumeMapping.LinearToDecibel
                ? LinearToDecibel((float)newValue / MaxValue)
                : (float)newValue;
            Mixer.SetFloat(Group, dB);
        }

        public static float LinearToDecibel(float linear)
        {
            linear = Mathf.Clamp(linear, MIN_LINEAR, 1f);
            return Mathf.Log10(linear) * 20f;
        }

        public static float DecibelToLinear(float dB)
        {
            if (dB <= MIN_DB) return 0f;
            return Mathf.Pow(10f, dB / 20f);
        }
    }
}