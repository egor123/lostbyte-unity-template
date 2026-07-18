using Lostbyte.Toolkit.CustomEditor.Graphs;
using Lostbyte.Toolkit.FactSystem;

namespace Lostbyte.Toolkit.Director
{
    [CustomGraphNode("Logic/Statement Node")]
    public class StatementNode : PlayableTrackNode
    {
        [GraphIn("In")] public PlayableTrackNode[] In;
        [GraphOut("Out")] public PlayableTrackNode Out;
        [GraphField("")] public Statement Statement;
        public override IPlayableClipNodeBehaviour GetClip(PlayableTrackBehaviour track)
        {
            Statement.Execute();
            return Out != null ? Out.GetClip(track) : null;
        }
    }
}
