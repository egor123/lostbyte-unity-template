using Lostbyte.Toolkit.Audio.Music;
using Lostbyte.Toolkit.CustomEditor.Editor.Graphs;
using UnityEngine;

namespace Lostbyte.Toolkit.Audio.Editor
{
    public class MusicTrackNodeView : CustomGraphNode<MusicTrackGraphView, MusicTrackData, MusicTrackNodeView, MusicTrackNode>
    {
        public MusicTrackNodeView(MusicTrackData asset, MusicTrackGraphView graph, MusicTrackNode node) : base(asset, graph, node)
        {
        }

        public override Vector2 LoadPosition() => Node.Position;

        public override void SavePosition(Vector2 position) => Node.Position = position;
    }
}
