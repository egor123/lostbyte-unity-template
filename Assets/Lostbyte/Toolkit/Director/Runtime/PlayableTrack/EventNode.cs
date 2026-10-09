using Lostbyte.Toolkit.CustomEditor.Graphs;
using Lostbyte.Toolkit.FactSystem;

namespace Lostbyte.Toolkit.Director
{
    [CustomGraphNode("Logic/Event Node")]
    public class EventNode : PlayableTrackNode
    {
        [GraphIn("In")] public PlayableTrackNode[] In;
        [GraphOut("Out")] public PlayableTrackNode Out;
        [GraphField("")] public EventWrapper Event;
        public override IPlayableClipNodeBehaviour GetClip(PlayableTrackBehaviour track)
        {
            Event.Raise();
            return Out != null ? Out.GetClip(track) : null;
        }
    }
}
