using Lostbyte.Toolkit.CustomEditor.Graphs;
using Lostbyte.Toolkit.Director;
using UnityEngine;

namespace Lostbyte.Toolkit.Scenes
{
    [CustomGraphNode("Narrative/Fade Node")]
    public class FadeNode : PlayableTrackNode
    {
        [GraphIn("In")] public PlayableTrackNode[] In;
        [GraphOut("Out")] public PlayableTrackNode Out;
        [GraphField] public FadeType Fade = FadeType.FadeIn;
        [GraphField] public float IdleTime = 0f;
        [GraphField] public float FadeTime = 1f;

        public override IPlayableClipNodeBehaviour GetClip(PlayableTrackBehaviour track) => new FadeNodeBehaviour(this, track);
    }
    public class FadeNodeBehaviour : PlayableClipNodeBehaviour<FadeNode>
    {
        public FadeNodeBehaviour(FadeNode node, PlayableTrackBehaviour track) : base(node, track) { }
        public override bool IsReady => true;
        public override bool IsFinished => Time >= Node.IdleTime + Node.FadeTime;
        public override IPlayableClipNodeBehaviour GetNext(PlayableTrackBehaviour track) => Node.Out ? Node.Out.GetClip(track) : null;
        public override void OnStart()
        {
            if (SceneManager.Instance == null) return;
            if (Node.Fade.Equals(FadeType.FadeIn))
            {
                SceneManager.Instance.LoadingScreen.SetFadeIn(0f);
            }
            else if (Node.Fade.Equals(FadeType.FadeOut))
            {
                SceneManager.Instance.LoadingScreen.SetFadeOut(0f);
            }
        }
        public override void OnContinue() { }
        public override void OnPause()
        {
            Time = Node.IdleTime + Node.FadeTime;
            OnEnd();
        }
        public override void OnUpdate()
        {
            if (SceneManager.Instance == null) return;
            if (Node.Fade.Equals(FadeType.FadeIn))
            {
                var progress = Mathf.Clamp01(Time / Node.FadeTime);
                SceneManager.Instance.LoadingScreen.SetFadeIn(progress);
            }
            else if (Node.Fade.Equals(FadeType.FadeOut))
            {
                var progress = Mathf.Clamp01((Time - Node.IdleTime) / Node.FadeTime);
                SceneManager.Instance.LoadingScreen.SetFadeOut(progress);
            }
        }
        public override void OnEnd()
        {
            if (SceneManager.Instance == null) return;
            if (Node.Fade.Equals(FadeType.FadeIn))
            {
                SceneManager.Instance.LoadingScreen.SetFadeIn(1f);
            }
            else if (Node.Fade.Equals(FadeType.FadeOut))
            {
                SceneManager.Instance.LoadingScreen.SetFadeOut(1f);
            }
        }
    }
}
