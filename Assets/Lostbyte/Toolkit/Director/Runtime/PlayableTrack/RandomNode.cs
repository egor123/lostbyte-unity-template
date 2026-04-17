using System.Collections.Generic;
using System.Linq;
using Lostbyte.Toolkit.Common;
using UnityEngine;

namespace Lostbyte.Toolkit.Director
{
    public class RandomNode : PlayableTrackNode
    {
        public List<SerializedTuple<float, PlayableTrackNode>> WeightedNodes;

        public override IPlayableClipNodeBehaviour GetClip(PlayableTrackBehaviour track)
        {
            if (WeightedNodes == null || WeightedNodes.Count == 0) return null;
            float value = Random.Range(0f, WeightedNodes.Sum(n => n.Item1));
            foreach (var item in WeightedNodes)
                if ((value -= item.Item1) <= 0)
                    return item.Item2 ? item.Item2.GetClip(track) : null;
            return null;
        }
    }
}
