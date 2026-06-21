using UnityEngine;
using UnityEngine.Playables;

namespace Lostbyte.Toolkit.TimelineExtensions
{
    [TimelineExtension(Name = "Light/Pulse", BindingType = typeof(Light), ColorHex = "#FFA500")]
    public class LightPulseAction : BaseTimelineAction
    {
        public float TargetIntensity = 5f;
        private Light _light;

        public override void OnStart(Playable playable, Object boundObject)
        {
            if (boundObject != null) _light = (boundObject as GameObject).GetComponent<Light>();
        }

        public override void ProcessFrame(Playable playable, FrameData info, Object boundObject)
        {
            if (_light == null) return;
            float p = (float)(playable.GetTime() / playable.GetDuration());
            _light.intensity = Mathf.Lerp(0, TargetIntensity, p);
        }
    }
}
