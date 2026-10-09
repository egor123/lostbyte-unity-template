using System.Collections.Generic;
using Lostbyte.Toolkit.CustomEditor.Graphs;
using Lostbyte.Toolkit.FactSystem;
using Lostbyte.Toolkit.Localization;

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
        public DialogueNodeBehaviour(DialogueNode node, PlayableTrackBehaviour track) : base(node, track) { }
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
    public class DialogueBehaviour : IPlayableClipBehaviour
    {
        public static void Schedule(KeyContainer actor, Priority priority = Priority.Default, OnContinueBehaviour schedule = OnContinueBehaviour.Schedule, params Paragraph[] paragraphs)
        {
            Director.Schedule(new DialogueBehaviour(actor, paragraphs) { SchedulingBehaviour = schedule }, priority: priority);
        }
        public static void Schedule(KeyContainer actor, Priority priority = Priority.Default, OnContinueBehaviour schedule = OnContinueBehaviour.Schedule, params (LocalizedReference<string> text, float gap)[] paragraphs)
        {
            var p = new Paragraph[paragraphs.Length];
            for (int i = 0; i < paragraphs.Length; i++)
            {
                (var text, var gap) = paragraphs[i];
                p[i] = new Paragraph() { String = text, Gap = gap };
            }
            Schedule(actor, priority, schedule, p);
        }
        public InteruptBehaviour InteruptBehaviour { get; set; } = InteruptBehaviour.Skip;
        public OnContinueBehaviour SchedulingBehaviour { get; set; } = OnContinueBehaviour.Schedule;
        public float Time { get; set; }
        public KeyContainer Actor { get; set; }
        public Paragraph[] Paragraphs { get; set; }
        public DialogueBehaviour(KeyContainer actor, params Paragraph[] paragraphs) => (Actor, Paragraphs) = (actor, paragraphs);
        private int _state;
        private int _idx = 0;
        private float _t = 0f;

        public bool IsFinished() => _idx >= Paragraphs.Length;
        public void OnStart() => _idx = _state = 0;
        public void OnContinue() => _state = 0;
        public void OnPause()
        {
            Time = _state = 0;
            SubtitlesManager.Instance.Clear();
        }
        public void OnUpdate()
        {
            var paragraph = Paragraphs[_idx];
            if (_state == 0)
            {
                SubtitlesManager.Instance.Set(Actor, paragraph.String);
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
        public void OnEnd() => SubtitlesManager.Instance.Clear();
    }

}