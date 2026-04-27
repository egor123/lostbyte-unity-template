using Lostbyte.Toolkit.CustomEditor.Graphs;

namespace Lostbyte.Toolkit.Director
{
    [CustomGraphNode("Wait Node")]
    public class WaitNode : PlayableTrackNode
    {
        [GraphIn("In")] public PlayableTrackNode[] In;
        [GraphOut("Out")] public PlayableTrackNode NextNode;

        [GraphField] public float Time;
        public override IPlayableClipNodeBehaviour GetClip(PlayableTrackBehaviour track) => new WaitNodeBehaviour(this, track);
    }
    public class WaitNodeBehaviour : PlayableClipNodeBehaviour<WaitNode>
    {
        public WaitNodeBehaviour(WaitNode node, PlayableTrackBehaviour track) : base(node, track) { }
        public override bool IsReady => true;
        public override bool IsFinished => Time >= Node.Time;
        public override IPlayableClipNodeBehaviour GetNext(PlayableTrackBehaviour track) => Node.NextNode ? Node.NextNode.GetClip(track) : null;
        public override void OnContinue() { }
        public override void OnEnd() { }
        public override void OnPause() { }
        public override void OnStart() { }
        public override void OnUpdate() { }
    }
}
