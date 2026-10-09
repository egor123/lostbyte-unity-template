using Cinemachine;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Lostbyte.Toolkit.TimelineExtensions
{
    [TimelineExtension(Name = "Cinemachine/Dolly", BindingType = typeof(CinemachineVirtualCamera), ColorHex = "#a51818")]
    public class CameraDollyAction : BaseTimelineAction
    {
        public override ClipCaps ClipCaps => ClipCaps.Blending;
        private CinemachineTrackedDolly _dolly;
        public override void OnStart(Playable playable, Object boundObject)
        {
            if (boundObject is not GameObject obj || !obj.TryGetComponent<CinemachineVirtualCamera>(out var camera)) return;
            _dolly = camera.GetCinemachineComponent<CinemachineTrackedDolly>();
        }

        public override void ProcessFrame(Playable playable, FrameData info, Object boundObject)
        {
            if (_dolly == null) return;
            _dolly.m_PathPosition = info.weight;
        }
    }
}
