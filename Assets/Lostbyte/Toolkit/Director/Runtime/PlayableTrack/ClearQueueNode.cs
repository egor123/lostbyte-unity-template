using Lostbyte.Toolkit.CustomEditor.Graphs;

namespace Lostbyte.Toolkit.Director
{
    [CustomGraphNode("Logic/Clear Queue Node")]
    public class ClearQueueNode : PlayableTrackNode
    {
        [GraphIn("In")] public PlayableTrackNode[] In;
        [GraphOut("Out")] public PlayableTrackNode Out;

        public override IPlayableClipNodeBehaviour GetClip(PlayableTrackBehaviour track)
        {
            Director.Instance.ClearQueue();
            return Out != null ? Out.GetClip(track) : null;
        }
    }
}
