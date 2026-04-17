using Lostbyte.Toolkit.SaveSystem;

namespace Lostbyte.Toolkit.Director
{
    public class SaveNode : PlayableTrackNode
    {
        public PlayableTrackNode NextNode;
        public override IPlayableClipNodeBehaviour GetClip(PlayableTrackBehaviour track) => new SaveNodeBehaviour(this, track);
        public class SaveNodeBehaviour : PlayableClipNodeBehaviour<SaveNode>
        {
            public SaveNodeBehaviour(SaveNode node, PlayableTrackBehaviour track) : base(node, track) { }
            public override bool IsReady => true;
            public override bool IsFinished => true;
            public override IPlayableClipNodeBehaviour GetNext(PlayableTrackBehaviour track)
            {
                SaveLoader.Instance.Save();
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
