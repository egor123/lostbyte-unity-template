using System;
using Lostbyte.Toolkit.CustomEditor;
using Lostbyte.Toolkit.FactSystem;
using UnityEngine;

namespace Lostbyte.Toolkit.Management
{
    [Tag("System")]
    [SupportedFactTypes(typeof(int), typeof(Enum))]
    public class FullScreenModeReaction : FactReaction
    {
        public override FactReaction Copy() => new FullScreenModeReaction();
        public override void OnLoad(object data) => OnValueChanged(null, Value);
        protected override void OnValueChanged(object oldValue, object newValue)
        {
            int mode = Convert.ToInt32(newValue);
            Screen.fullScreenMode = mode switch
            {
                0 => FullScreenMode.ExclusiveFullScreen,
                1 => FullScreenMode.FullScreenWindow,
                2 => FullScreenMode.MaximizedWindow,
                3 => FullScreenMode.Windowed,
                _ => throw new Exception("Unsupported Fullscreen Mode")
            };
        }
    }
}
