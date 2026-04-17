namespace Lostbyte.Toolkit.Director
{
    public class CheckpointNode : PlayableTrackNode
    {
        public PlayableTrackNode NextNode;
        public PlayableTrackNode OnContinueNode;
        public CheckpointBehaviour CheckpointBehaviour = CheckpointBehaviour.Continue;
        public override IPlayableClipNodeBehaviour GetClip(PlayableTrackBehaviour track)
        {
            track.Checkpoint = this;
            return NextNode ? NextNode.GetClip(track) : null;
        }
        public IPlayableClipNodeBehaviour GetOnContinueClip(PlayableTrackBehaviour track) => OnContinueNode ? OnContinueNode.GetClip(track) : null;
    }
    public enum CheckpointBehaviour { Restart, Continue, TrySkip, Exit }
}
