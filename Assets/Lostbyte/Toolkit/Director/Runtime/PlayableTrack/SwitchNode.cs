using System;
using System.Collections.Generic;
using Lostbyte.Toolkit.CustomEditor.Graphs;
using Lostbyte.Toolkit.FactSystem;

namespace Lostbyte.Toolkit.Director
{
    [CustomGraphNode("Logic/Switch Node")]
    public class SwitchNode : PlayableTrackNode
    {
        [GraphIn("In")] public PlayableTrackNode[] In;

        [GraphField] public List<Option> Nodes;

        [Serializable]
        public struct Option
        {
            [GraphField("")] public Condition Condition;
            [GraphOut("Out")] public PlayableTrackNode Out;
        }
        public override IPlayableClipNodeBehaviour GetClip(PlayableTrackBehaviour track) => new SwitchNodeBehaviour(this, track);
    }

    public class SwitchNodeBehaviour : PlayableClipNodeBehaviour<SwitchNode>
    {
        public SwitchNodeBehaviour(SwitchNode node, PlayableTrackBehaviour track) : base(node, track) { }
        private IPlayableClipNodeBehaviour _nextNode = null;
        public override bool IsReady => true;
        private bool _conditionIsMet = false;
        public override bool IsFinished => Node.Nodes.Count == 0 || _conditionIsMet;
        public override IPlayableClipNodeBehaviour GetNext(PlayableTrackBehaviour track) => _nextNode;

        public override void OnContinue() => _nextNode = null;
        public override void OnEnd() { }
        public override void OnPause() { }
        public override void OnStart() => _nextNode = null;
        public override void OnUpdate()
        {
            foreach (var option in Node.Nodes)
            {
                if (option.Condition.IsMet)
                {
                    if (option.Out)
                        _nextNode = option.Out.GetClip(Track);
                    _conditionIsMet = true;
                    return;
                }
            }
        }
    }
}
