using Lostbyte.Toolkit.Common;
using Lostbyte.Toolkit.CustomEditor;
using Lostbyte.Toolkit.CustomEditor.Graphs;
using UnityEngine.Playables;

namespace Lostbyte.Toolkit.Director
{
    [CustomGraphNode("Narrative/Timeline Node")]
    public class TimelineNode : PlayableTrackNode
    {
        [GraphIn("In")] public PlayableTrackNode[] In;
        [GraphOut("Out")] public PlayableTrackNode Out;
        [GraphField, Required] public PlayableAsset Asset;
        public override IPlayableClipNodeBehaviour GetClip(PlayableTrackBehaviour track) => new TimelineNodeBehaviour(this, track);
    }
    public class TimelineNodeBehaviour : PlayableClipNodeBehaviour<TimelineNode>
    {
        public TimelineNodeBehaviour(TimelineNode node, PlayableTrackBehaviour track) : base(node, track) { }
        private PlayableDirector Player => Director.Instance.Player;
        public override bool IsReady => true;
        public override bool IsFinished => _isFinished;
        private bool _isFinished = false;
        public override IPlayableClipNodeBehaviour GetNext(PlayableTrackBehaviour track) => Node.Out ? Node.Out.GetClip(track) : null;
        public override void OnStart()
        {
            _isFinished = false;
            if (Player == null || Node.Asset == null)
            {
                Print.MError("Cannot play asset!");
                _isFinished = true;
                return;
            }
            Player.playableAsset = Node.Asset;
            Player.time = 0;
            Player.Play();
        }
        public override void OnContinue()
        {
            if (Player == null) return;
            Player.Play();
        }
        public override void OnEnd()
        {
            if (Player == null) return;
            Player.Pause();
            Player.time = 0;
            Player.Evaluate();
            Player.Stop();
        }
        public override void OnPause()
        {
            if (Player == null || Player.state != PlayState.Playing) return;
            Player.Pause();
        }
        public override void OnUpdate()
        {
            if (Player == null || Player.state != PlayState.Playing || Player.time >= Player.duration)
                _isFinished = true;
        }
    }
}
