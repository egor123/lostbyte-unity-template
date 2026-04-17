using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Lostbyte.Toolkit.CustomEditor;
using Lostbyte.Toolkit.FactSystem;

namespace Lostbyte.Toolkit.Audio
{
    public class FactAudio : MonoBehaviour
    {
        [SerializeField, Autowired(Autowired.Type.Parent, isForced: true), Hide] private KeyReference m_key;
        [field: SerializeField] public FactAudioSettings Settings { get; private set; }
        [field: SerializeField] public List<AudioSource> Sources { get; private set; } = new();
        private readonly List<int> _priorities = new();
        private List<AudioSettingsRunner> _runners;
        private List<AudioSettingsRunner> Runners => _runners ??= m_key ? Settings.Settings.Select(s => s.Create(m_key.Key, this)).ToList() : null;

        public void Play(AudioSettings settings)
        {
            if (Sources == null || Sources.Count == 0 || settings.Clips == null || settings.Clips.Length == 0)
                return;
            ValidatePriorities();
            
            AudioSource source = null;
            int idx = -1;
            int priority = int.MaxValue;
            for (int i = 0; i < Sources.Count; i++)
            {
                AudioSource currentSource = Sources[i];
                int currentPriority = _priorities[i];
                if (!currentSource.isPlaying)
                {
                    idx = i;
                    source = currentSource;
                    priority = -1;
                }
                else
                {
                    if (!settings.AllowMultiple && settings.Clips.Contains(currentSource.clip)) return;
                    if (source == null && settings.StopActive || currentPriority < priority)
                    {
                        idx = i;
                        source = currentSource;
                        priority = currentPriority;
                    }
                }
            }
            if (source == null) return;

            float pitch = Random.Range(
                (settings.Pitch - settings.PitchRandomization) / 2f,
                (settings.Pitch + settings.PitchRandomization) / 2f);
            pitch = Mathf.Clamp(pitch, -3f, 3f); // Allow wide pitch range

            float volume = Random.Range(
                (settings.Volume - settings.VolumeRandomization) / 2f,
                (settings.Volume + settings.VolumeRandomization) / 2f);
            volume = Mathf.Clamp01(volume);

            AudioClip clip = settings.Clips[Random.Range(0, settings.Clips.Length)];
            _priorities[idx] = settings.Priority;
            source.pitch = pitch;
            source.volume = volume;
            source.clip = clip;
            source.Play();
        }
        private void ValidatePriorities()
        {
            while (_priorities.Count != Sources.Count)
            {
                if (_priorities.Count > Sources.Count) _priorities.RemoveAt(_priorities.Count - 1);
                else _priorities.Add(-1);
            }
        }
        private void OnEnable() => Runners?.ForEach(i => i.Enable());
        private void OnDisable() => Runners?.ForEach(i => i.Disable());
    }
}
