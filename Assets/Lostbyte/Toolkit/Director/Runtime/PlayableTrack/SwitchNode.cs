using System;
using System.Collections.Generic;
using System.Linq;
using Lostbyte.Toolkit.Common;
using Lostbyte.Toolkit.CustomEditor.Graphs;
using Lostbyte.Toolkit.FactSystem;

namespace Lostbyte.Toolkit.Director
{
    [CustomGraphNode("Switch Node")]
    public class SwitchNode : PlayableTrackNode
    {
        [GraphIn("In")] public PlayableTrackNode[] In;

        [GraphField] public List<Option> Nodes;

        [Serializable]
        public struct Option
        {
            [GraphField] public Condition Condition;
            [GraphOut("Out")] public PlayableTrackNode Out;
        }
        public override IPlayableClipNodeBehaviour GetClip(PlayableTrackBehaviour track) => new SwitchNodeBehaviour(this, track);
    }

    public class SwitchNodeBehaviour : PlayableClipNodeBehaviour<SwitchNode>
    {
        public SwitchNodeBehaviour(SwitchNode node, PlayableTrackBehaviour track) : base(node, track)
        {
            _nodes = Node.Nodes?.Select(n => new SerializedTuple<Condition, IPlayableClipNodeBehaviour>(n.Condition, n.Out != null ? n.Out.GetClip(track) : null)).ToList();
        }
        private IPlayableClipNodeBehaviour _nextNode;
        private readonly List<SerializedTuple<Condition, IPlayableClipNodeBehaviour>> _nodes;
        public override bool IsReady => true;
        private bool _conditionIsMet = false;
        public override bool IsFinished => _nodes.Count == 0 || _conditionIsMet;
        public override IPlayableClipNodeBehaviour GetNext(PlayableTrackBehaviour track) => _nextNode;

        public override void OnContinue() => _nextNode = null;
        public override void OnEnd() { }
        public override void OnPause() { }
        public override void OnStart() => _nextNode = null;
        public override void OnUpdate()
        {
            foreach (var node in _nodes)
            {
                if (node.Item1.IsMet)
                {
                    _nextNode = node.Item2;
                    _conditionIsMet = true;
                    return;
                }
            }
        }
    }
}
