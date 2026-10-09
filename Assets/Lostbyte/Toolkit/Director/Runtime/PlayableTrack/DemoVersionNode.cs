using Lostbyte.Toolkit.Common;
using Lostbyte.Toolkit.CustomEditor.Graphs;

namespace Lostbyte.Toolkit.Director
{
    [CustomGraphNode("Logic/Demo Version Node")]
    public class DemoVersionNode : PlayableTrackNode
    {
        [GraphIn("In")] public PlayableTrackNode[] In;
        [GraphOut] public PlayableTrackNode DemoVersionOut;
        [GraphOut] public PlayableTrackNode FullVersionOut;
        public override IPlayableClipNodeBehaviour GetClip(PlayableTrackBehaviour track)
        {
#if DEMO_BUILD
            Print.MLog("Demo Version Out");
            return DemoVersionOut != null ? DemoVersionOut.GetClip(track) : null;
#else
            Print.MLog("Full Version Out");
            return FullVersionOut != null ? FullVersionOut.GetClip(track) : null;
#endif
        }
    }
}
