using Lostbyte.Toolkit.CustomEditor.Graphs;

namespace Lostbyte.Toolkit.Audio.Music
{
    [CustomGraphNode("Logic/GOTO")]
    public class GotoNode : MusicTrackNode
    {
        [GraphIn] public MusicTrackNode[] In;
        public MusicTrackNode Next;

        public override MusicSegmentData GetCurrentSegment()
        {
            if (Next == null) return null;
            return Next.GetCurrentSegment();
        }

        public override MusicSegmentData GetNextSegment()
        {
            if (Next == null) return null;
            return Next.GetNextSegment();
        }
    }
}
