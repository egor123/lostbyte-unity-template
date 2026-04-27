using System;
using System.Collections.Generic;
using System.Linq;
using Lostbyte.Toolkit.CustomEditor.Graphs;
using UnityEngine;

namespace Lostbyte.Toolkit.Director
{
    [CustomGraphNode("Random Node")]
    public class RandomNode : PlayableTrackNode
    {
        [GraphIn("In")] public PlayableTrackNode[] In;

        [GraphField] public List<Option> Options;

        [Serializable]
        public struct Option
        {
            [GraphField] public float Weight;
            [GraphOut("Out")] public PlayableTrackNode Out;
        }

        public override IPlayableClipNodeBehaviour GetClip(PlayableTrackBehaviour track)
        {
            if (Options == null || Options.Count == 0) return null;
            float value = UnityEngine.Random.Range(0f, Options.Sum(n => n.Weight));
            foreach (var item in Options)
                if ((value -= item.Weight) <= 0)
                    return item.Out ? item.Out.GetClip(track) : null;
            return null;
        }
    }
}
