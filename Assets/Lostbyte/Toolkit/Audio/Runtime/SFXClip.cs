using System.Collections;
using System.Collections.Generic;
using Lostbyte.Toolkit.CustomEditor;
using UnityEngine;

namespace Lostbyte.Toolkit.Audio
{
    [CreateAssetMenu(fileName = "SFXClip", menuName = "Audio/SFXClip")]
    public class SFXClip : ScriptableObject
    {
        [field: SerializeField, Required] public List<AudioClip> Clips { get; private set; }
        [field: SerializeField, Range(0f, 1f)] public float MinVolume { get; private set; } = 1f;
        [field: SerializeField, Range(0f, 1f)] public float MaxVolume { get; private set; } = 1f;
        [field: SerializeField, Range(-3f, 3f)] public float MinPitch { get; private set; } = 1f;
        [field: SerializeField, Range(-3f, 3f)] public float MaxPitch { get; private set; } = 1f;
        [field: SerializeField, Range(-1f, 1f)] public float StereoPan { get; private set; } = 0f;
        [field: SerializeField, Range(0f, 1f), Tooltip("From 2D to 3D")] public float SpatialBlend { get; private set; } = 0f;
        [field: SerializeField, Range(0f, 1.1f)] public float ReverbZoneMix { get; private set; } = 0f;
        [field: SerializeField, Range(0f, 5f)] public float DopplerLevel { get; private set; } = 1f;
        [field: SerializeField, Range(0f, 360f)] public float Spread { get; private set; } = 0f;
        [field: SerializeField] public AudioRolloffMode RolloffMode { get; private set; } = AudioRolloffMode.Logarithmic;
        [field: SerializeField, Min(0f)] public float MinDistance { get; private set; } = 1f;
        [field: SerializeField, Min(0f)] public float MaxDistance { get; private set; } = 500f;

        public void Play(Vector3 worldPosition = default, float delay = 0f) => SFXManager.PlaySFX(null, worldPosition, this, delay);
        public void Play(Transform parent, Vector3 localPosition, float delay = 0f) => SFXManager.PlaySFX(parent, localPosition, this, delay);
        public void Play(Transform parent, float delay = 0f) => SFXManager.PlaySFX(parent, Vector3.zero, this, delay);
    }
}
