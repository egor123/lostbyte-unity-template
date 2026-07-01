using Lostbyte.Toolkit.CustomEditor.Graphs;

namespace Lostbyte.Toolkit.Director
{
    [CustomGraphNode("Logic/Redirect Node")]
    public class RedirectNode : PlayableTrackNode
    {
        [GraphIn("")] public PlayableTrackNode[] In;
        [GraphOut("")] public PlayableTrackNode Out;

        public override IPlayableClipNodeBehaviour GetClip(PlayableTrackBehaviour track) => Out.GetClip(track);

    }
}
