using System;
using Lostbyte.Toolkit.CustomEditor;
using Lostbyte.Toolkit.FactSystem;
using UnityEngine;

namespace Lostbyte.Toolkit.Management
{
    [Tag("System")]
    [SupportedFactTypes(typeof(int), typeof(Enum))]
    public class QualityReaction : FactReaction
    {
        public override FactReaction Copy() => new QualityReaction();
        public override void OnLoad(object data) => OnValueChanged(null, Value);
        protected override void OnValueChanged(object oldValue, object newValue)
        {
            int level = Convert.ToInt32(newValue);
            if (level != QualitySettings.GetQualityLevel())
            {
                QualitySettings.SetQualityLevel(level, true);
            }
        }
    }
}
