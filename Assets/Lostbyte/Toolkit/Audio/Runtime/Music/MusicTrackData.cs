using Lostbyte.Toolkit.CustomEditor;
using UnityEngine;

namespace Lostbyte.Toolkit.Audio.Music
{
    [CreateAssetMenu(fileName = "NewMusicTrack", menuName = "Audio/Music Track")]
    public class MusicTrackData : ScriptableObject
    {
        [Hide] public Vector2 EntryNodePosition = new(0, 0), ExitNodePosition = new(0, 300);

        [Tooltip("Beats Per Minute. Drives all DSP timing calculations.")]
        public float BPM = 120f;
        [Tooltip("Beats per measure (e.g., 4 for 4/4 time)")]
        public int BeatsPerBar = 4;
        public StemMixRule[] StemMixRules;

        [Header("Segments")]
        [Tooltip("The very first segment that plays when this track starts.")]
        public MusicTrackNode[] EntrySegments;
        public MusicTrackNode[] ExitSegments;

        public MusicSegmentData GetSegment()
        {
            if (EntrySegments.Length == 0) return null;
            return EntrySegments[Random.Range(0, EntrySegments.Length)].GetCurrentSegment();
        }
        public MusicSegmentData GetEndSegment()
        {
            if (ExitSegments.Length == 0) return null;
            return ExitSegments[Random.Range(0, ExitSegments.Length)].GetCurrentSegment();
        }

        public double GetSecondsPerBeat() => BPM <= 0 ? 0 : 60.0 / BPM;
        public double GetSecondsPerBar() => GetSecondsPerBeat() * BeatsPerBar;
    }
}
