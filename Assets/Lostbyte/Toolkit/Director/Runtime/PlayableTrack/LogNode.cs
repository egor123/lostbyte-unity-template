using Lostbyte.Toolkit.Common;
using Lostbyte.Toolkit.CustomEditor.Graphs;

namespace Lostbyte.Toolkit.Director
{
    [CustomGraphNode("Info/Log Node")]
    public class LogNode : PlayableTrackNode
    {
        [GraphIn("In")] public PlayableTrackNode[] In;
        [GraphOut("Out")] public PlayableTrackNode Out;
        [GraphField("")] public string Message;
        public override IPlayableClipNodeBehaviour GetClip(PlayableTrackBehaviour track)
        {
            Print.Log(Message);
            return Out != null ? Out.GetClip(track) : null;
        }

    }
}
