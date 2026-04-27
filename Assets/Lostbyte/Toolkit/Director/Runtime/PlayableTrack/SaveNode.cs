
using Lostbyte.Toolkit.CustomEditor.Graphs;
using Lostbyte.Toolkit.FactSystem;

namespace Lostbyte.Toolkit.Director
{
    [CustomGraphNode("Save Node")]
    public class SaveNode : PlayableTrackNode
    {
        [GraphIn("In")] public PlayableTrackNode[] In;
        [GraphOut("Out")] public PlayableTrackNode NextNode;

        [GraphField] public KeyContainer Key;

        public override IPlayableClipNodeBehaviour GetClip(PlayableTrackBehaviour track) => new SaveNodeBehaviour(this, Key, track);
        public class SaveNodeBehaviour : PlayableClipNodeBehaviour<SaveNode>
        {
            private KeyContainer _key;
            public SaveNodeBehaviour(SaveNode node, KeyContainer key, PlayableTrackBehaviour track) : base(node, track) => _key = key;
            public override bool IsReady => true;
            public override bool IsFinished => true;
            public override IPlayableClipNodeBehaviour GetNext(PlayableTrackBehaviour track)
            {
                _key.Save();
                return Node.NextNode ? Node.NextNode.GetClip(track) : null;
            }
            public override void OnContinue() { }
            public override void OnEnd() { }
            public override void OnPause() { }
            public override void OnStart() { }
            public override void OnUpdate() { }
        }
    }
}
