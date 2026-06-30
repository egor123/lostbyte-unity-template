using Lostbyte.Toolkit.CustomEditor.Graphs;
using Lostbyte.Toolkit.FactSystem;

namespace Lostbyte.Toolkit.Director
{
    [CustomGraphNode("Statement Node")]
    public class StatementNode : PlayableTrackNode
    {
        [GraphIn("")] public PlayableTrackNode[] In;
        [GraphOut("")] public PlayableTrackNode Out;
        [GraphField] public Statement Statement;
        public override IPlayableClipNodeBehaviour GetClip(PlayableTrackBehaviour track)
        {
            Statement.Execute();
            return Out != null ? Out.GetClip(track) : null;
        }
    }
}
