using System.Collections.Generic;
using Lostbyte.Toolkit.CustomEditor.Graphs;
using Lostbyte.Toolkit.FactSystem;

namespace Lostbyte.Toolkit.Director
{
    [CustomGraphNode("Narrative/Dialogue Node")]
    public class DialogueNode : PlayableTrackNode
    {
        [GraphIn("In")] public PlayableTrackNode[] In;
        [GraphOut("Out")] public PlayableTrackNode Out;
        [GraphField] public KeyContainer Actor;
        [GraphField] public List<Paragraph> Paragraphs = new();

        public override IPlayableClipNodeBehaviour GetClip(PlayableTrackBehaviour track) => new DialogueNodeBehaviour(this, track);
    }
    public class DialogueNodeBehaviour : PlayableClipNodeBehaviour<DialogueNode>
    {
        private int _state;
        private int _idx = 0;
        private float _t = 0f;
        public DialogueNodeBehaviour(DialogueNode node, PlayableTrackBehaviour track) : base(node, track) {}
        public override bool IsReady => true;
        public override bool IsFinished => _idx >= Node.Paragraphs.Count;
        public override IPlayableClipNodeBehaviour GetNext(PlayableTrackBehaviour track) => Node.Out ? Node.Out.GetClip(track) : null;
        public override void OnStart()
        {
            _idx = 0;
            _state = 0;
        }
        public override void OnContinue()
        {
            _state = 0;
        }
        public override void OnEnd()
        {
            SubtitlesManager.Instance.Clear();
        }
        public override void OnPause()
        {
            Time = 0;
            _state = 0;
            SubtitlesManager.Instance.Clear();
        }
        public override void OnUpdate()
        {
            var paragraph = Node.Paragraphs[_idx];
            if (_state == 0)
            {
                SubtitlesManager.Instance.Set(Node.Actor, paragraph.String);
                _state++;
            }
            else if (_state == 1 && SubtitlesManager.Instance.CurrentText == null)
            {
                _t = Time;
                _state++;
            }
            else if (_state == 2 && Time - _t > paragraph.Gap)
            {
                _state = 0;
                _idx++;
            }
        }
    }
}