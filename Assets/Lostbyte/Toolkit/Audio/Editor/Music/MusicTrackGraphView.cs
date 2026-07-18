using Lostbyte.Toolkit.Audio.Music;
using Lostbyte.Toolkit.CustomEditor.Editor.Graphs;

namespace Lostbyte.Toolkit.Audio.Editor
{
    public class MusicTrackGraphView : CustomGraphView<MusicTrackGraphView, MusicTrackData, MusicTrackNodeView, MusicTrackNode>
    {
        public override void Load(MusicTrackData asset)
        {
            base.Load(asset);
            EntryNode start = new(Asset, this, null);
            ExitNode exit = new(Asset, this, null);
            if (asset != null)
            {
                start.SetPosition(new(start.LoadPosition(), GetDefaultNodeSize()));
                start.Load();
                exit.SetPosition(new(exit.LoadPosition(), GetDefaultNodeSize()));
                exit.Load();
            }
            AddElement(start);
            AddElement(exit);
        }
    }
}
