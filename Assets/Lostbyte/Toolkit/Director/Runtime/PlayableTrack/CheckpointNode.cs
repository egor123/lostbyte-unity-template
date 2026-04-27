using Lostbyte.Toolkit.CustomEditor.Graphs;

namespace Lostbyte.Toolkit.Director
{
    [CustomGraphNode("Checkpoint Node")]
    public class CheckpointNode : PlayableTrackNode
    {
        [GraphIn("In")] public PlayableTrackNode[] In;
        [GraphOut("Out")] public PlayableTrackNode NextNode;
        [GraphOut("On Continue")] public PlayableTrackNode OnContinueNode;

        [GraphField] public CheckpointBehaviour CheckpointBehaviour = CheckpointBehaviour.Continue;
        public override IPlayableClipNodeBehaviour GetClip(PlayableTrackBehaviour track)
        {
            track.Checkpoint = this;
            return NextNode ? NextNode.GetClip(track) : null;
        }
        public IPlayableClipNodeBehaviour GetOnContinueClip(PlayableTrackBehaviour track) => OnContinueNode ? OnContinueNode.GetClip(track) : null;
    }
    public enum CheckpointBehaviour { Restart, Continue, TrySkip, Exit }
}
