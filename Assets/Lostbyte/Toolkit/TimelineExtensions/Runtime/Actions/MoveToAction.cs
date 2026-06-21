using UnityEngine;
using UnityEngine.Playables;

namespace Lostbyte.Toolkit.TimelineExtensions
{
    [TimelineExtension(Name = "Transform/Move To", BindingType = typeof(Transform), ColorHex = "#1E90FF")]
    public class MoveToAction : BaseTimelineAction
    {
        public Space space = Space.Self;
        public Vector3 targetPosition;
        public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private Transform _transform;
        private Vector3 _startPosition;

        public override void OnStart(Playable playable, Object boundObject)
        {
            if (boundObject == null) return;
            _transform = (boundObject as GameObject).transform;
            _startPosition = space == Space.World ? _transform.position : _transform.localPosition;
        }

        public override void ProcessFrame(Playable playable, FrameData info, Object boundObject)
        {
            if (_transform == null) return;

            float p = (float)(playable.GetTime() / playable.GetDuration());
            float curveValue = easeCurve.Evaluate(p);

            Vector3 currentPos = Vector3.LerpUnclamped(_startPosition, targetPosition, curveValue);

            if (space == Space.World)
                _transform.position = currentPos;
            else
                _transform.localPosition = currentPos;
        }
    }
}
