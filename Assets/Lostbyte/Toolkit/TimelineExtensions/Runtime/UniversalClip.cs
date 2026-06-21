using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Lostbyte.Toolkit.TimelineExtensions
{
    [Serializable]
    public class UniversalClip : PlayableAsset, ITimelineClipAsset
    {
        [SerializeReference] public BaseTimelineAction action;
        public virtual ClipCaps clipCaps => ClipCaps.All;
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<UniversalBehaviour>.Create(graph);
            playable.GetBehaviour().action = action;
            return playable;
        }
    }
}
