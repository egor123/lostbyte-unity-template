using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lostbyte.Toolkit.Audio
{
    public class SFXSource : MonoBehaviour
    {
        [SerializeField] private AudioSource m_source;

        internal void Play(SFXTrack sound, float delay = 0f)
        {
            m_source.Stop();
            gameObject.SetActive(true);
            m_source.Stop();
            m_source.clip = sound.Clips[Random.Range(0, sound.Clips.Count)];
            m_source.volume = Mathf.Clamp(sound.Volume + Random.Range(-sound.VolumeRandomisation / 2f, sound.VolumeRandomisation / 2f), 0, 1);
            m_source.pitch = Mathf.Clamp(sound.Pitch + Random.Range(-sound.PitchRandomisation / 2f, sound.PitchRandomisation / 2f), -3f, 3f);
            m_source.PlayDelayed(delay);
            StartCoroutine(DelayReturnToPool(delay + m_source.clip.length));
        }

        private IEnumerator DelayReturnToPool(float delayTime)
        {
            yield return new WaitForSeconds(delayTime);
            SFXManager.Instance.AddToSFXPool(this);
            gameObject.SetActive(false);
        }
    }
}
