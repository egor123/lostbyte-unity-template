using System.Collections;
using System.Collections.Generic;
using Lostbyte.Toolkit.CustomEditor;
using Lostbyte.Toolkit.FactSystem;
using UnityEngine;

namespace Lostbyte.Toolkit.Audio
{
    [Tag("Audio")]
    public class SfxClipReaction : FactReaction
    {
        public SFXClip Clip;
        public override FactReaction Copy() => new SfxClipReaction() { Clip = Clip };
        protected override void OnValueChanged(object oldValue, object newValue) => Clip.Play();
    }
    [Tag("Audio")]
    public class EventSfxClipReaction : EventReaction
    {
        public SFXClip Clip;
        public override EventReaction Copy() => new EventSfxClipReaction() { Clip = Clip };
        protected override void OnRaise() => Clip.Play();
    }
}
