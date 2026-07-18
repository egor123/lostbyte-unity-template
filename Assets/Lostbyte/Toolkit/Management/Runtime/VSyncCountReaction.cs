using Lostbyte.Toolkit.CustomEditor;
using Lostbyte.Toolkit.FactSystem;
using UnityEngine;

namespace Lostbyte.Toolkit.Management
{
    [Tag("System")]
    [SupportedFactTypes(typeof(int))]
    public class VSyncCountReaction : FactReaction
    {
        public override FactReaction Copy() => new VSyncCountReaction();
        public override void OnLoad(object data) => OnValueChanged(null, Value);
        protected override void OnValueChanged(object oldValue, object newValue)
        {
            QualitySettings.vSyncCount = (int) newValue;
        }
    }
}
