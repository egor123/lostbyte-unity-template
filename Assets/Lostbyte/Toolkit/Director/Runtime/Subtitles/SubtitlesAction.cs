using Lostbyte.Toolkit.TimelineExtensions;
using UnityEngine.Playables;
using Lostbyte.Toolkit.FactSystem;
using Lostbyte.Toolkit.Localization;

namespace Lostbyte.Toolkit.Director
{
    [TimelineExtension(Name = "Subtitles/Clip", BindingType = typeof(KeyContainer), ColorHex = "#00ffd5")]
    public class SubtitlesAction : BaseTimelineAction
    {
        public LocalizedReference<string> String;
        private KeyContainer _actor;
        public override void OnStart(Playable playable, UnityEngine.Object boundObject)
        {
            SubtitlesManager.Instance.Clear();
            if (boundObject != null) _actor = boundObject as KeyContainer;
        }

        public override void ProcessFrame(Playable playable, FrameData info, UnityEngine.Object boundObject)
        {
            if (_actor == null) return;
            SubtitlesManager.Instance.SetFrame(_actor, String, (float)playable.GetTime(), (float)playable.GetDuration());
        }

        public override void OnStop(Playable playable, UnityEngine.Object boundObject)
        {
            SubtitlesManager.Instance.Clear();
        }

    }
}
