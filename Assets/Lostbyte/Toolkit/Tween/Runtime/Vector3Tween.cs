using System;
using UnityEngine;

namespace Lostbyte.Toolkit.Tween
{
    public class Vector3Tween : Tween
    {
        internal Vector3 StartVector { private get; set; }
        internal Vector3 TargetVector { private get; set; }
        internal Action<Vector3> OnChange;
        internal Vector3Tween(MonoBehaviour initiator, Action<Vector3> onChange) : base(initiator, 1f) { OnChange = onChange; }
        internal override void Init() => OnChange.Invoke(StartVector);
        protected override void DoTween(float delta) => OnChange.Invoke(Vector3.LerpUnclamped(StartVector, TargetVector, delta));
    }
}
