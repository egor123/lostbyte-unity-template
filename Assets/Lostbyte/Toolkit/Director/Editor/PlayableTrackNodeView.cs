
using Lostbyte.Toolkit.CustomEditor.Editor.Graphs;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Lostbyte.Toolkit.Director.Editor
{
    public class PlayableTrackNodeView : CustomGraphNode<PlayableTrackGraphView, PlayableTrack, PlayableTrackNodeView, PlayableTrackNode>
    {
        public PlayableTrackNodeView(PlayableTrack asset, PlayableTrackGraphView graph, PlayableTrackNode node) : base(asset, graph, node)
        {

        }

        public override Vector2 LoadPosition() => Node.Position;
        public override void SavePosition(Vector2 position) => Node.Position = position;
    }
}
