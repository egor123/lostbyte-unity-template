using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lostbyte.Toolkit.CustomEditor;

namespace Lostbyte.Toolkit.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class SFXSource : MonoBehaviour
    {
        [SerializeField, Autowired(isForced: true), Hide] private AudioSource m_source;
        private Transform _parent;
        private Vector3 _localPos;
        private float _timer;
        private Coroutine _corutine;
        internal void Play(Transform parent, Vector3 position, SFXClip sound, float delay = 0f)
        {
            _parent = parent;
            if (_parent != null) _localPos = position;
            else transform.position = position;
            gameObject.SetActive(true);
            m_source.Stop();
            if (sound.Clips != null && sound.Clips.Count > 0)
            {
                m_source.loop = false;
                m_source.clip = sound.Clips[Random.Range(0, sound.Clips.Count)];
                m_source.volume = Mathf.Clamp(Random.Range(sound.MinVolume, sound.MaxVolume), 0, 1);
                m_source.pitch = Mathf.Clamp(Random.Range(sound.MinPitch, sound.MaxPitch), -3f, 3f);
                m_source.spatialBlend = sound.SpatialBlend;
                m_source.reverbZoneMix = sound.ReverbZoneMix;
                m_source.panStereo = sound.StereoPan;
                m_source.dopplerLevel = sound.DopplerLevel;
                m_source.spread = sound.Spread;
                m_source.rolloffMode = sound.RolloffMode;
                m_source.minDistance = sound.MinDistance;
                m_source.maxDistance = sound.MaxDistance;


                m_source.PlayDelayed(delay);
                if (Application.isPlaying)
                {
                    float duration = m_source.clip.length / Mathf.Abs(m_source.pitch);
                    _corutine = StartCoroutine(DelayReturnToPool(delay + duration));
                }
            }
            else if (Application.isPlaying)
            {
                _corutine = StartCoroutine(DelayReturnToPool(0));
            }
        }

        private IEnumerator DelayReturnToPool(float delayTime)
        {
            float timer = 0f;
            while (timer < delayTime)
            {
                if (_parent != null) transform.position = _parent.TransformPoint(_localPos);
                timer += Time.unscaledDeltaTime;
                yield return null;
            }
            SFXManager.AddToSFXPool(this);
        }
        internal void ForceReturnToPool()
        {
            if (_corutine != null) StopCoroutine(_corutine);
            m_source.Stop();
            SFXManager.AddToSFXPool(this);
        }
    }
}
