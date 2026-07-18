using Lostbyte.Toolkit.CustomEditor.Graphs;
using UnityEngine;

namespace Lostbyte.Toolkit.Audio.Music
{
    [CustomGraphNode("Music/Segment")]
    public class MusicSegmentData : MusicTrackNode
    {
        [GraphIn] public MusicTrackNode[] In;
        [GraphOut] public MusicTrackNode[] Out;

        [Tooltip("The mathematical length of this loop in beatse, e.g., 4 bars in 4/4 time = 16 beats.")]
        [GraphField] public int LengthInBeats = 16;
        [Tooltip("All clips must be the exact same length and BPM.")]
        [GraphField] public StemData[] Stems;


        public override MusicSegmentData GetCurrentSegment() => this;
        public override MusicSegmentData GetNextSegment()
        {
            int l = Out.Length;
            if (l == 0) return this;
            int i = Random.Range(0, l);
            var next = Out[i].GetCurrentSegment();
            return next;
        }
    }
}