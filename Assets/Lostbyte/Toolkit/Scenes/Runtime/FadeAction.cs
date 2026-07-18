using Lostbyte.Toolkit.Common;
using Lostbyte.Toolkit.TimelineExtensions;
using UnityEngine;
using UnityEngine.Playables;

namespace Lostbyte.Toolkit.Scenes
{
    [TimelineExtension(Name = "VFX/Fade", ColorHex = "#858585")]
    public class FadeAction : BaseTimelineAction
    {
        public FadeType Fade = FadeType.FadeIn;

        public override void OnStart(Playable playable, Object boundObject)
        {
            Set(0);
        }

        public override void ProcessFrame(Playable playable, FrameData info, Object boundObject)
        {
            Set(info.weight);
        }

        public override void OnStop(Playable playable, Object boundObject)
        {
            var graph = playable.GetGraph();
            if (!graph.IsPlaying()) return;
            if (playable.GetTime() <= 0)
            {
                Set(0);
                return;
            }
            var root = graph.GetRootPlayable(0);
            bool isPlayingForward = root.IsValid() && root.GetSpeed() > 0;
            if (isPlayingForward)
            {
                Set(1);
            }
            else
            {
                Set(0);
            }
        }

        private void Set(float p)
        {
            var sm = SceneManager.Instance;
            if (sm == null) return;
            if (Fade == FadeType.FadeIn) sm.LoadingScreen.SetFadeIn(p);
            else if (Fade == FadeType.FadeOut) sm.LoadingScreen.SetFadeOut(p);
        }
    }
}