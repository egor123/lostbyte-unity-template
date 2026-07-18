using System.Collections.Generic;
using Lostbyte.Toolkit.CustomEditor.Graphs;
using Lostbyte.Toolkit.FactSystem;
using UnityEngine;

namespace Lostbyte.Toolkit.Audio.Music
{
    [CustomGraphNode("Logic/Condition")]
    public class MusicConditionNode : MusicTrackNode
    {
        [GraphIn] public MusicTrackNode[] In;
        [GraphField] public List<Option> Nodes;

        [System.Serializable]
        public struct Option
        {
            [GraphField("")] public Condition Condition;
            [GraphOut("Out")] public MusicTrackNode[] Out;
        }
        public override MusicSegmentData GetCurrentSegment()
        {
            foreach (var node in Nodes)
            {
                if (!node.Condition.IsMet) continue;
                if (node.Out.Length == 0) return null;
                var mNode = node.Out[Random.Range(0, node.Out.Length)];
                return mNode.GetCurrentSegment();
            }
            return null;
        }

        public override MusicSegmentData GetNextSegment()
        {
            foreach (var node in Nodes)
            {
                if (!node.Condition.IsMet) continue;
                if (node.Out.Length == 0) return null;
                var mNode = node.Out[Random.Range(0, node.Out.Length)];
                return mNode.GetNextSegment();
            }
            return null;
        }
    }
}
