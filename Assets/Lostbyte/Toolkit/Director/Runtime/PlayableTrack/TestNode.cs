using System;
using System.Collections.Generic;
using Lostbyte.Toolkit.CustomEditor;
using Lostbyte.Toolkit.CustomEditor.Graphs;
using UnityEngine;

namespace Lostbyte.Toolkit.Director
{
    [CustomGraphNode("Test Node")]
    public class TestNode : PlayableTrackNode
    {
        [GraphIn("In")] public PlayableTrackNode[] In;
        [GraphOut("Out")] public PlayableTrackNode[] Out;
        [GraphField] public string Test = "Hello World";
        [GraphField] public List<Option> Options;

        [Serializable]
        public struct Option
        {
            [GraphField] public int Number, Number2;
            [GraphOut] public PlayableTrackNode[] Out;
        }
        public override IPlayableClipNodeBehaviour GetClip(PlayableTrackBehaviour track) => Out[0].GetClip(track);
    }
}