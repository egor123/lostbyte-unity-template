using UnityEngine;

namespace Lostbyte.Toolkit.Tween
{
    public class TransformTween : Tween
    {
        internal TransformTween(MonoBehaviour initiator, Transform transform, Space space = Space.Self, float duartion = 1) : base(initiator, duartion) { _transform = transform; _space = space; }
        internal Vector3? TargetPosition { get; set; }
        internal Quaternion? TargetRotation { get; set; }
        internal Vector3? TargetScale { get; set; }
        private readonly Space _space;
        private readonly Transform _transform;
        private Vector3 _startPosition;
        private Quaternion _startRotation;
        private Vector3 _startScale; //Always local!
        internal override void Init()
        {
            switch (_space)
            {
                case Space.Self:
                    _startPosition = _transform.localPosition;
                    _startRotation = _transform.localRotation;
                    _startScale = _transform.localScale;
                    break;
                case Space.World:
                    _startPosition = _transform.position;
                    _startRotation = _transform.rotation;
                    _startScale = _transform.localScale;
                    break;
            }
        }
        internal void OffsetTargetPosition(Vector3 vector)
        {
            if (!TargetPosition.HasValue)
                TargetPosition = _space == Space.Self ? _transform.localPosition : _transform.position;
            TargetPosition += vector;
        }
        internal void OffsetTargetRotation(Vector3 vector)
        {
            if (!TargetRotation.HasValue)
                TargetRotation = _space == Space.Self ? _transform.localRotation : _transform.rotation;
            TargetRotation *= Quaternion.Euler(vector);
        }
        internal void OffsetTargetScale(Vector3 vector)
        {
            if (!TargetScale.HasValue)
                TargetScale = _transform.localScale;
            TargetScale = new(TargetScale.Value.x*vector.x,TargetScale.Value.y*vector.y,TargetScale.Value.z*vector.z);
        }
        protected override void DoTween(float delta)
        {
            switch (_space)
            {
                case Space.Self:
                    if (TargetPosition != null) _transform.localPosition = Vector3.LerpUnclamped(_startPosition, TargetPosition.Value, delta);
                    if (TargetRotation != null) _transform.localRotation = Quaternion.LerpUnclamped(_startRotation, TargetRotation.Value, delta);
                    if (TargetScale != null) _transform.localScale = Vector3.LerpUnclamped(_startScale, TargetScale.Value, delta);
                    break;
                case Space.World:
                    if (TargetPosition != null) _transform.position = Vector3.LerpUnclamped(_startPosition, TargetPosition.Value, delta);
                    if (TargetRotation != null) _transform.rotation = Quaternion.LerpUnclamped(_startRotation, TargetRotation.Value, delta);
                    if (TargetScale != null) _transform.localScale = Vector3.LerpUnclamped(_startScale, TargetScale.Value, delta);
                    break;
            }
        }
    }
}
