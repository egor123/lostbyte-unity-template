using Lostbyte.Toolkit.CustomEditor.Graphs;
using UnityEngine;

namespace Lostbyte.Toolkit.Audio.Music
{
    [CustomGraphNode("Logic/Tag")]
    public class TagNode : MusicTrackNode
    {
        [GraphIn] public MusicTrackNode[] In;
        [GraphOut] public MusicTrackNode[] Out;

        public override MusicSegmentData GetCurrentSegment()
        {
            if (Out.Length == 0) return null;
            return Out[Random.Range(0, Out.Length)].GetCurrentSegment();
        }

        public override MusicSegmentData GetNextSegment()
        {
            if (Out.Length == 0) return null;
            return Out[Random.Range(0, Out.Length)].GetNextSegment();
        }
    }
}
