using Lostbyte.Toolkit.CustomEditor.Graphs;
using Lostbyte.Toolkit.Director;

namespace Lostbyte.Toolkit.Scenes
{
    [CustomGraphNode("Narrative/Loading Screen Lock Node")]
    public class LoadingScreenLockNode : PlayableTrackNode
    {
        [GraphIn("In")] public PlayableTrackNode[] In;
        [GraphOut("Out")] public PlayableTrackNode Out;
        [GraphField("Lock")] public bool Lock;
        public override IPlayableClipNodeBehaviour GetClip(PlayableTrackBehaviour track)
        {
            SceneManager.Instance.LoadingScreen.SetLock(Lock);
            return Out != null ? Out.GetClip(track) : null;
        }
    }
}
