using System;
using UnityEngine;

namespace Lostbyte.Toolkit.Tween
{
    public class StringTween : Tween
    {
        internal string TargetText { private get; set; }
        internal Action<string> OnChange;
        internal StringTween(MonoBehaviour initiator, Action<string> onChange) : base(initiator, 1f) { OnChange = onChange; }
        internal override void Init() => OnChange.Invoke("");
        protected override void DoTween(float delta) => OnChange.Invoke(TargetText[..Mathf.RoundToInt(delta * TargetText.Length)]);
    }
}
