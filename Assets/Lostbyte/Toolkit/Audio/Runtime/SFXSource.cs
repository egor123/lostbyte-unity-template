using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lostbyte.Toolkit.CustomEditor;

namespace Lostbyte.Toolkit.Audio
{
    public class SFXSource : MonoBehaviour
    {
        [SerializeField, Autowired] private AudioSource m_source;
        private Transform _parent;
        private Vector3 _localPos;

        internal void Play(Transform parent, Vector3 position, SFXClip sound, float delay = 0f)
        {
            _parent = parent;
            if (_parent != null) _localPos = position;
            else transform.position = position;
            m_source.Stop();
            gameObject.SetActive(true);
            m_source.Stop();
            if (sound.Clips != null && sound.Clips.Count > 0)
            {
                m_source.loop = false;
                m_source.clip = sound.Clips[Random.Range(0, sound.Clips.Count)];
                m_source.volume = Mathf.Clamp(sound.Volume + Random.Range(-sound.VolumeRandomisation / 2f, sound.VolumeRandomisation / 2f), 0, 1);
                m_source.pitch = Mathf.Clamp(sound.Pitch + Random.Range(-sound.PitchRandomisation / 2f, sound.PitchRandomisation / 2f), -3f, 3f);
                m_source.PlayDelayed(delay);
                StartCoroutine(DelayReturnToPool(delay + m_source.clip.length));
            }
            else
            {
                StartCoroutine(DelayReturnToPool(0));
            }
        }

        private void Update()
        {
            if (_parent == null) return;
            transform.position = _parent.TransformPoint(_localPos);
        }

        private IEnumerator DelayReturnToPool(float delayTime)
        {
            yield return new WaitForSeconds(delayTime);
            SFXManager.Instance.AddToSFXPool(this);
            gameObject.SetActive(false);
            _parent = null;
            _localPos = Vector3.zero;
        }
    }
}
