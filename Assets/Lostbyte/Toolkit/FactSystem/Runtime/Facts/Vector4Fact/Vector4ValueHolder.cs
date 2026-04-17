using UnityEngine;

namespace Lostbyte.Toolkit.FactSystem
{
    public class Vector4ValueHolder : ValueHolder<Vector4>
    {
        public override IValueHolder Copy() => new Vector4ValueHolder() { Value = Value };
    }
}