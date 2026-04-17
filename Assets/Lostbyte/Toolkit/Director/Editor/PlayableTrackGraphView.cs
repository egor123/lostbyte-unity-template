
using Lostbyte.Toolkit.CustomEditor.Editor.Graphs;

namespace Lostbyte.Toolkit.Director.Editor
{
    public class PlayableTrackGraphView : CustomGraphView<PlayableTrackGraphView, PlayableTrack, PlayableTrackNodeView, PlayableTrackNode>
    {
        public override void Load(PlayableTrack asset)
        {
            base.Load(asset);
            StartNodeView start = new(Asset, this, null);
            if (asset != null)
            {
                start.SetPosition(new(start.LoadPosition(), GetDefaultNodeSize()));
                start.Load();
            }
            AddElement(start);
        }
    }
}
