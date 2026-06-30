using System;
using Lostbyte.Toolkit.CustomEditor;
using Lostbyte.Toolkit.FactSystem;
using UnityEngine;
using UnityEngine.Audio;

namespace Lostbyte.Toolkit.Audio
{
    [Tag("Settings/Audio")]
    [SupportedFactTypes(typeof(float))]
    public class AudioMixerReaction : FactReaction
    {
        public AudioMixer Mixer;
        public string Group;
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

        protected override void OnValueChanged(object newValue)
        {
            if (Mixer == null || string.IsNullOrEmpty(Group)) return;
            float dB = MappingMode == VolumeMapping.LinearToDecibel
                ? LinearToDecibel((float)newValue)
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