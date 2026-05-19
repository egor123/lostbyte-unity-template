using System.Collections;
using System.Collections.Generic;
using Lostbyte.Toolkit.CustomEditor.Graphs;
using UnityEngine;

namespace Lostbyte.Toolkit.Director
{
    [CustomGraphNode("Redirect Node")]
    public class RedirectNode : PlayableTrackNode
    {
        [GraphIn("")] public PlayableTrackNode[] In;
        [GraphOut("")] public PlayableTrackNode Out;

        public override IPlayableClipNodeBehaviour GetClip(PlayableTrackBehaviour track) => Out.GetClip(track);

    }
}
