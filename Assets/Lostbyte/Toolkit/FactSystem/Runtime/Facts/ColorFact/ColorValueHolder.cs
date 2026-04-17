using UnityEngine;

namespace Lostbyte.Toolkit.FactSystem
{
    public class ColorValueHolder : ValueHolder<Color>
    {
        public override IValueHolder Copy() => new ColorValueHolder() { Value = Value };
    }
}