using Lostbyte.Toolkit.CustomEditor.Graphs;
using UnityEngine;

namespace Lostbyte.Toolkit.Audio.Music
{
    [CustomGraphNode("Music/Stinger")]
    public class MusicStingerData : MusicTrackNode
    {
        [GraphIn] public MusicTrackNode[] In;
        [GraphOut] public MusicTrackNode[] Out;

        [Tooltip("How this stinger snaps to the currently playing track's rhythm.")]
        [GraphField] public TransitionSyncMode SnapToGrid = TransitionSyncMode.NextBeat;
        [GraphField] public StemData Stem;

        public override MusicSegmentData GetCurrentSegment()
        {
            MusicManager.Instance.PlayStinger(Stem, SnapToGrid);
            if (Out.Length == 0) return null;
            return Out[Random.Range(0, Out.Length)].GetCurrentSegment();
        }

        public override MusicSegmentData GetNextSegment()
        {
            MusicManager.Instance.PlayStinger(Stem, SnapToGrid);
            if (Out.Length == 0) return null;
            return Out[Random.Range(0, Out.Length)].GetNextSegment();
        }
    }
}
