using UnityEngine;
using UnityEngine.Playables;

namespace Lostbyte.Toolkit.TimelineExtensions
{
    [TimelineExtension(Name = "UI/Alpha", BindingType = typeof(CanvasGroup), ColorHex = "#FFA500")]
    public class CanvasGroupAction : BaseTimelineAction
    {
        private CanvasGroup _group;
        private float _startAlpha;
        public override void OnStart(Playable playable, Object boundObject)
        {
            Init(boundObject);
            if (_group == null) return;
            _startAlpha = _group.alpha;
        }
        public override void ProcessFrame(Playable playable, FrameData info, Object boundObject)
        {
            if (_group == null) return;
            _group.alpha = info.weight;
        }

        public override void OnStop(Playable playable, Object boundObject)
        {
            if (_group == null) return;
            _group.alpha = _startAlpha;
        }
        private void Init(Object boundObject)
        {
            if (boundObject is not GameObject obj) return;
            _group = obj.GetComponent<CanvasGroup>();
        }
    }
}
