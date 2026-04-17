using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lostbyte.Toolkit.Audio
{
    [CreateAssetMenu(fileName = "SFXTrack", menuName = "Audio/SFXTrack")]
    public class SFXTrack : ScriptableObject
    {
        [field: SerializeField] public List<AudioClip> Clips { get; private set; }
        [field: SerializeField, Range(0f, 1f)] public float Volume { get; private set; } = 1f;
        [field: SerializeField, Range(0f, 1f)] public float VolumeRandomisation { get; private set; } = .1f;
        [field: SerializeField, Range(-3f, 3f)] public float Pitch { get; private set; } = 1f;
        [field: SerializeField, Range(0f, 1f)] public float PitchRandomisation { get; private set; } = .1f;

        public void Play(Vector3 pos) => SFXManager.Instance.PlaySFX(this, pos);

    }
}
