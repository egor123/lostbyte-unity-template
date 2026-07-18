using System.Collections.Generic;
using Lostbyte.Toolkit.Common;
using Lostbyte.Toolkit.CustomEditor.Graphs;

namespace Lostbyte.Toolkit.Director
{
    [CustomGraphNode("Logic/Parallel Node")]
    public class ParallelNode : PlayableTrackNode
    {
        [GraphIn("In")] public PlayableTrackNode[] In;
        [GraphOut("Out")] public PlayableTrackNode[] Out;
        [GraphOut("On Finish")] public PlayableTrackNode OnFinish;
        public override IPlayableClipNodeBehaviour GetClip(PlayableTrackBehaviour track) => new ParallelNodeBehaviour(this, track);
    }
    public class ParallelNodeBehaviour : PlayableClipNodeBehaviour<ParallelNode>
    {
        private readonly List<IPlayableClipNodeBehaviour> _activeBranches = new();
        public ParallelNodeBehaviour(ParallelNode node, PlayableTrackBehaviour track) : base(node, track) { }
        public override bool IsReady => true;
        public override bool IsFinished => _activeBranches.Count == 0;

        public override IPlayableClipNodeBehaviour GetNext(PlayableTrackBehaviour track)
        {
            if (Node.OnFinish != null)
                return Node.OnFinish.GetClip(track);
            return null;
        }

        public override void OnStart()
        {
            _activeBranches.Clear();
            if (Node.Out == null || Node.Out.Length == 0) return;
            foreach (var outNode in Node.Out)
            {
                if (outNode == null) continue;
                var childBehaviour = outNode.GetClip(Track);
                if (childBehaviour != null)
                {
                    childBehaviour.Time = 0;
                    childBehaviour.OnStart();
                    _activeBranches.Add(childBehaviour);
                }
            }
        }

        public override void OnUpdate()
        {
            for (int i = _activeBranches.Count - 1; i >= 0; i--)
            {
                var branch = _activeBranches[i];
                var clip = branch;
                bool isChildFinished = clip != null && clip.IsFinished;
                if (!isChildFinished)
                {
                    if (clip != null) clip.Time += UnityEngine.Time.deltaTime;
                    branch.OnUpdate();
                }
                else
                {
                    branch.OnEnd();
                    var nextNode = branch.GetNext(Track);
                    if (nextNode != null)
                    {
                        nextNode.Time = 0;
                        nextNode.OnStart();
                        _activeBranches[i] = nextNode;
                    }
                    else
                    {
                        _activeBranches.RemoveAt(i);
                    }
                }
            }
        }

        public override void OnPause()
        {
            foreach (var branch in _activeBranches)
                branch.OnPause();
        }

        public override void OnContinue()
        {
            foreach (var branch in _activeBranches)
                branch.OnContinue();
        }

        public override void OnEnd()
        {
            foreach (var branch in _activeBranches)
                branch.OnEnd();
            _activeBranches.Clear();
        }
    }
}
