using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lostbyte.Toolkit.Audio
{
    [CreateAssetMenu(fileName = "SFXClip", menuName = "Audio/SFXClip")]
    public class SFXClip : ScriptableObject
    {
        [field: SerializeField] public List<AudioClip> Clips { get; private set; }
        [field: SerializeField, Range(0f, 1f)] public float Volume { get; private set; } = 1f;
        [field: SerializeField, Range(0f, 1f)] public float VolumeRandomisation { get; private set; } = .1f;
        [field: SerializeField, Range(-3f, 3f)] public float Pitch { get; private set; } = 1f;
        [field: SerializeField, Range(0f, 1f)] public float PitchRandomisation { get; private set; } = .1f;

        public void Play(Vector3 worldPosition, float delay = 0f) => SFXManager.Instance.PlaySFX(null, worldPosition, this, delay);
        public void Play(Transform parent, Vector3 localPosition, float delay = 0f) => SFXManager.Instance.PlaySFX(parent, localPosition, this, delay);
    }
}
