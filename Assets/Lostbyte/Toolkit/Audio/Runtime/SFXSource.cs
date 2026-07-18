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
        internal SFXClip _clip;
        internal void Play(Transform parent, Vector3 position, SFXClip clip, float delay = 0f)
        {
            _clip = clip;
            _parent = parent;
            if (_parent != null) _localPos = position;
            else transform.position = position;
            gameObject.SetActive(true);
            m_source.Stop();
            if (clip.Clips != null && clip.Clips.Count > 0)
            {
                m_source.loop = false;
                m_source.clip = clip.Clips[Random.Range(0, clip.Clips.Count)];
                m_source.volume = Mathf.Clamp(Random.Range(clip.MinVolume, clip.MaxVolume), 0, 1);
                m_source.pitch = Mathf.Clamp(Random.Range(clip.MinPitch, clip.MaxPitch), -3f, 3f);
                m_source.spatialBlend = clip.SpatialBlend;
                m_source.reverbZoneMix = clip.ReverbZoneMix;
                m_source.panStereo = clip.StereoPan;
                m_source.dopplerLevel = clip.DopplerLevel;
                m_source.spread = clip.Spread;
                m_source.rolloffMode = clip.RolloffMode;
                m_source.minDistance = clip.MinDistance;
                m_source.maxDistance = clip.MaxDistance;


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
