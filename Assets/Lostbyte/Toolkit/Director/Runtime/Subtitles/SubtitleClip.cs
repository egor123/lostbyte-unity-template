using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Playables;

namespace Lostbyte.Toolkit.Director
{
    public class SubtitleClip : PlayableAsset
    {
        public LocalizedString SubtitleText;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<SubtitleBehaviour>.Create(graph);
            var behaviour = playable.GetBehaviour();
            behaviour.SubtitleText = SubtitleText;
            return playable;
        }
    }
}
