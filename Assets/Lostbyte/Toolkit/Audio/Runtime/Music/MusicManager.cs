using System;
using System.Collections.Generic;
using Lostbyte.Toolkit.CustomEditor;
using UnityEngine;
using UnityEngine.Audio;

namespace Lostbyte.Toolkit.Audio.Music
{
    public class MusicManager : MonoBehaviour
    {
        public static MusicManager Instance { get; private set; }

        [Serializable]
        private class LayerData
        {
            [ReadOnly] public AudioSource Source;
            [SerializeField, ReadOnly] private float m_volume;
            [NonSerialized] public double DestroyTime;
            [NonSerialized] public double FadeStartTime;
            [NonSerialized] public double FadeDuration;
            [NonSerialized] public float StartVolume;

            public void SetVolume(float volume)
            {
                volume = Mathf.Clamp01(volume);
                if (Source != null) Source.volume = volume;
                m_volume = volume;
            }

            public void PlayScheduled(AudioClip clip, double startTime)
            {
                if (Source == null) return;
                Source.clip = clip;
                Source.PlayScheduled(startTime);
            }

            public void SetScheduledEndTime(double time)
            {
                if (Source == null) return;
                Source.SetScheduledEndTime(time);
                DestroyTime = time;
            }
        }

        public AudioSource AudioSourcePrefab;
        public AudioMixerGroup MainGroup, StingerGroup;

        [Header("Default Transition")]
        [Tooltip("Transition mode to use when all players are unregistered (stopping the music).")]
        public TransitionSyncMode DefaultSyncMode = TransitionSyncMode.EndOfSegment;

        [Tooltip("Fade out duration in beats when stopping the music.")]
        public int DefaultFadeOutBeats = 4;

        [Header("State Tracking")]
        [SerializeField, ReadOnly] private MusicPlayer m_currentPlayer;
        [SerializeField, ReadOnly] private MusicPlayer m_pendingPlayer;
        [SerializeField, ReadOnly] private MusicTrackData m_currentTrack;
        [SerializeField, ReadOnly] private MusicSegmentData m_segment;

        [SerializeField, ReadOnly] private float m_segmentTime;
        [SerializeField, ReadOnly] private int m_segmentBeat;

        [SerializeField, ReadOnly] private List<LayerData> m_activeLayers = new();
        private List<LayerData> m_fadingLayers = new();

        [SerializeField, ReadOnly] private LayerData m_stingerLayer;

        private double m_currentSegmentStartTime;
        private double m_nextSegmentStartTime;
        private const double LOOK_AHEAD_TIME = 0.5;

        [SerializeField, ReadOnly] private bool m_isPlayingEndSegment = false;
        [SerializeField, ReadOnly] private bool m_isStopping = false;

        private double m_trackFadeInStartTime;
        private double m_trackFadeInDuration;

        private static List<MusicPlayer> s_registeredPlayers = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            EvaluatePlayers();
        }

        private void Update()
        {
            CleanupFadingLayers();
            if (m_segment == null || m_currentTrack == null) return;
            double dspTime = AudioSettings.dspTime;
            if (dspTime >= m_currentSegmentStartTime)
            {
                m_segmentTime = (float)(dspTime - m_currentSegmentStartTime);
                float secPerBeat = 60f / m_currentTrack.BPM;
                m_segmentBeat = Mathf.FloorToInt(m_segmentTime / secPerBeat);
            }
            UpdateVolumes(instant: false);
            if (dspTime > m_nextSegmentStartTime - LOOK_AHEAD_TIME)
            {
                ScheduleNextSegment();
            }
        }

        public static void RegisterPlayer(MusicPlayer player)
        {
            if (!s_registeredPlayers.Contains(player))
            {
                s_registeredPlayers.Add(player);
                if (Instance != null) Instance.EvaluatePlayers();
            }
        }

        public static void UnregisterPlayer(MusicPlayer player)
        {
            if (s_registeredPlayers.Contains(player))
            {
                s_registeredPlayers.Remove(player);
                if (Instance != null) Instance.EvaluatePlayers();
            }
        }

        private void EvaluatePlayers()
        {
            MusicPlayer highest = null;
            foreach (var p in s_registeredPlayers)
            {
                if (highest == null || p.Priority > highest.Priority)
                {
                    highest = p;
                }
            }
            bool targetChanged = false;
            if (m_isStopping)
            {
                if (highest != null) targetChanged = true;
            }
            else
            {
                if (m_pendingPlayer != null)
                {
                    if (highest != m_pendingPlayer) targetChanged = true;
                }
                else
                {
                    if (highest != m_currentPlayer) targetChanged = true;
                }
            }

            if (!targetChanged) return;

            m_pendingPlayer = highest;

            if (highest == null)
            {
                m_isStopping = true;
                if (DefaultSyncMode != TransitionSyncMode.EndOfSegment)
                {
                    double targetTime = CalculateSyncTime(DefaultSyncMode, AudioSettings.dspTime);
                    double bpm = (m_currentTrack != null && m_currentTrack.BPM > 0) ? m_currentTrack.BPM : 120.0;
                    double fadeOutDuration = DefaultFadeOutBeats * (60.0 / bpm);

                    foreach (var layer in m_activeLayers)
                    {
                        layer.SetScheduledEndTime(targetTime + fadeOutDuration);
                        layer.FadeStartTime = targetTime;
                        layer.FadeDuration = fadeOutDuration;
                        layer.StartVolume = layer.Source.volume;
                        layer.Source.loop = true;
                    }

                    m_fadingLayers.AddRange(m_activeLayers);
                    m_activeLayers.Clear();

                    if (m_currentTrack != null) m_currentTrack.GetEndSegment();

                    m_currentPlayer = null;
                    m_currentTrack = null;
                    m_segment = null;
                    m_isPlayingEndSegment = false;
                }
            }
            else
            {
                m_isStopping = false;

                if (m_currentPlayer == null || m_activeLayers.Count == 0)
                {
                    if (m_currentTrack != null) m_currentTrack.GetEndSegment();
                    TransitionToPending(AudioSettings.dspTime + 0.1);
                }
                else if (m_pendingPlayer.SyncMode != TransitionSyncMode.EndOfSegment)
                {
                    double targetTime = CalculateSyncTime(m_pendingPlayer.SyncMode, AudioSettings.dspTime);
                    double bpm = (m_currentTrack != null && m_currentTrack.BPM > 0) ? m_currentTrack.BPM : 120.0;
                    double fadeOutDuration = (m_currentPlayer != null) ? m_currentPlayer.FadeOutBeats * (60.0 / bpm) : 2.0;
                    foreach (var layer in m_activeLayers)
                    {
                        layer.SetScheduledEndTime(targetTime + fadeOutDuration);
                        layer.FadeStartTime = targetTime;
                        layer.FadeDuration = fadeOutDuration;
                        layer.StartVolume = layer.Source.volume;
                        layer.Source.loop = true;
                    }
                    m_fadingLayers.AddRange(m_activeLayers);
                    m_activeLayers.Clear();
                    if (m_currentTrack != null) m_currentTrack.GetEndSegment();
                    TransitionToPending(targetTime);
                }
            }
        }

        private void TransitionToPending(double startTime)
        {
            m_currentPlayer = m_pendingPlayer;
            m_currentTrack = m_currentPlayer.Track;
            m_pendingPlayer = null;
            m_isPlayingEndSegment = false;
            m_isStopping = false;

            if (m_currentTrack != null)
            {
                m_trackFadeInStartTime = startTime;
                double bpm = m_currentTrack.BPM > 0 ? m_currentTrack.BPM : 120.0;
                m_trackFadeInDuration = (m_currentPlayer != null) ? m_currentPlayer.FadeInBeats * (60.0 / bpm) : 2.0;
                ScheduleSegment(m_currentTrack.GetSegment(), startTime);
                UpdateVolumes(instant: true);
            }
        }

        private void ScheduleSegment(MusicSegmentData data, double startTime)
        {
            m_segment = data;
            m_currentSegmentStartTime = startTime;

            for (int i = 0; i < data.Stems.Length; i++)
            {
                if (i >= m_activeLayers.Count)
                {
                    var obj = Instantiate(AudioSourcePrefab, transform);
                    obj.outputAudioMixerGroup = MainGroup;
                    obj.name = $"Music Source (Layer {m_activeLayers.Count})";
                    m_activeLayers.Add(new LayerData { Source = obj });
                }

                var stem = data.Stems[i];
                var layer = m_activeLayers[i];
                layer.Source.loop = false;
                layer.PlayScheduled(stem.Clip, startTime);
            }

            double secPerBeat = 60.0 / m_currentTrack.BPM;
            double segmentDuration = secPerBeat * data.LengthInBeats;
            m_nextSegmentStartTime = startTime + segmentDuration;
        }

        private void ScheduleNextSegment()
        {
            if ((m_pendingPlayer != null && m_pendingPlayer.SyncMode == TransitionSyncMode.EndOfSegment) ||
                (m_isStopping && DefaultSyncMode == TransitionSyncMode.EndOfSegment))
            {
                if (!m_isPlayingEndSegment)
                {
                    var endSegment = m_currentTrack.GetEndSegment();
                    if (endSegment != null)
                    {
                        m_isPlayingEndSegment = true;
                        ScheduleSegment(endSegment, m_nextSegmentStartTime);
                        return;
                    }
                }
                else
                {
                    var nextEndSegment = m_segment.GetNextSegment();
                    if (nextEndSegment != null)
                    {
                        ScheduleSegment(nextEndSegment, m_nextSegmentStartTime);
                        return;
                    }
                }
                double bpm = (m_currentTrack != null && m_currentTrack.BPM > 0) ? m_currentTrack.BPM : 120.0;
                double fadeOutDuration;
                if (m_isStopping)
                    fadeOutDuration = DefaultFadeOutBeats * (60.0 / bpm);
                else
                    fadeOutDuration = (m_currentPlayer != null) ? m_currentPlayer.FadeOutBeats * (60.0 / bpm) : 2.0;
                foreach (var layer in m_activeLayers)
                {
                    layer.SetScheduledEndTime(m_nextSegmentStartTime + fadeOutDuration);
                    layer.FadeStartTime = m_nextSegmentStartTime;
                    layer.FadeDuration = fadeOutDuration;
                    layer.StartVolume = layer.Source.volume;
                    layer.Source.loop = true;
                }
                m_fadingLayers.AddRange(m_activeLayers);
                m_activeLayers.Clear();
                if (m_isStopping)
                {
                    m_currentPlayer = null;
                    m_currentTrack = null;
                    m_segment = null;
                    m_isPlayingEndSegment = false;
                }
                else
                {
                    TransitionToPending(m_nextSegmentStartTime);
                }
                return;
            }
            var nextSegment = m_segment.GetNextSegment();
            if (nextSegment != null)
                ScheduleSegment(nextSegment, m_nextSegmentStartTime);
            else if (m_currentTrack != null)
            {
                nextSegment = m_currentTrack.GetSegment();
                if (nextSegment != null)
                    ScheduleSegment(nextSegment, m_nextSegmentStartTime);
            }
        }

        private void CleanupFadingLayers()
        {
            double dspTime = AudioSettings.dspTime;
            for (int i = m_fadingLayers.Count - 1; i >= 0; i--)
            {
                var layer = m_fadingLayers[i];
                if (dspTime > layer.DestroyTime)
                {
                    if (layer.Source != null)
                        Destroy(layer.Source.gameObject);
                    m_fadingLayers.RemoveAt(i);
                }
                else
                {
                    if (layer.FadeDuration > 0 && dspTime >= layer.FadeStartTime)
                    {
                        float t = (float)((dspTime - layer.FadeStartTime) / layer.FadeDuration);
                        layer.Source.volume = Mathf.Lerp(layer.StartVolume, 0f, t);
                    }
                    else if (layer.FadeDuration <= 0 && dspTime >= layer.FadeStartTime)
                    {
                        layer.Source.volume = 0f;
                    }
                }
            }
        }

        private void UpdateVolumes(bool instant)
        {
            if (m_segment == null) return;

            for (int i = 0; i < m_segment.Stems.Length; i++)
            {
                if (i >= m_activeLayers.Count) continue;

                var layer = m_activeLayers[i];
                float targetVol = 0;
                float fadeTime = 2f;

                if (m_segment != null && i < m_segment.Stems.Length)
                    targetVol = m_segment.Stems[i].DefaultVolume;

                if (m_currentTrack != null && m_currentTrack.StemMixRules != null)
                {
                    foreach (var rule in m_currentTrack.StemMixRules)
                    {
                        if (rule.TargetLayer == i)
                        {
                            var vol = rule.EvaluateTargetVolume();
                            if (vol.HasValue)
                            {
                                targetVol = vol.Value;
                                fadeTime = rule.FadeSmoothTime;
                                break;
                            }
                        }
                    }
                }

                float fadeMultiplier = 1f;
                if (m_trackFadeInDuration > 0)
                {
                    double dspTime = AudioSettings.dspTime;
                    if (dspTime < m_trackFadeInStartTime)
                        fadeMultiplier = 0f;
                    else
                        fadeMultiplier = Mathf.Clamp01((float)((dspTime - m_trackFadeInStartTime) / m_trackFadeInDuration));
                }

                targetVol *= fadeMultiplier;

                if (instant)
                {
                    layer.SetVolume(targetVol);
                }
                else
                {
                    float currentVol = layer.Source.volume;
                    if (!Mathf.Approximately(currentVol, targetVol))
                    {
                        float maxDelta = fadeTime > 0f ? 1f / fadeTime * Time.deltaTime : 1f;
                        layer.SetVolume(Mathf.MoveTowards(currentVol, targetVol, maxDelta));
                    }
                }
            }
        }

        public void PlayStinger(StemData stem, TransitionSyncMode snapToGrid = TransitionSyncMode.NextBeat)
        {
            if (stem.Clip == null || m_currentTrack == null) return;
            if (m_stingerLayer == null || m_stingerLayer.Source == null)
            {
                var source = Instantiate(AudioSourcePrefab, transform);
                source.outputAudioMixerGroup = StingerGroup;
                source.name = "Stinger Source";
                m_stingerLayer = new() { Source = source };
            }
            double dspTime = AudioSettings.dspTime;
            double scheduledTime = CalculateSyncTime(snapToGrid, dspTime);
            m_stingerLayer.SetVolume(stem.DefaultVolume);
            m_stingerLayer.PlayScheduled(stem.Clip, scheduledTime);
        }

        private double CalculateSyncTime(TransitionSyncMode syncMode, double currentDspTime)
        {
            if (syncMode == TransitionSyncMode.Immediate) return currentDspTime;
            if (m_currentTrack == null) return currentDspTime;

            double secPerBeat = 60.0 / m_currentTrack.BPM;
            int bpb = m_currentTrack.BeatsPerBar;
            double secPerBar = secPerBeat * bpb;
            double elapsedSinceSegmentStart = currentDspTime - m_currentSegmentStartTime;

            switch (syncMode)
            {
                case TransitionSyncMode.NextBeat:
                    double completedBeats = Math.Floor(elapsedSinceSegmentStart / secPerBeat);
                    double targetBeatTime = m_currentSegmentStartTime + ((completedBeats + 1) * secPerBeat);
                    if (targetBeatTime < currentDspTime + 0.05)
                        targetBeatTime += secPerBeat;
                    return targetBeatTime;
                case TransitionSyncMode.NextBar:
                    double completedBars = Math.Floor(elapsedSinceSegmentStart / secPerBar);
                    double targetBarTime = m_currentSegmentStartTime + ((completedBars + 1) * secPerBar);
                    if (targetBarTime < currentDspTime + 0.05)
                        targetBarTime += secPerBar;
                    return targetBarTime;
                case TransitionSyncMode.EndOfSegment:
                    return m_nextSegmentStartTime;
                default:
                    return currentDspTime;
            }
        }
    }
}