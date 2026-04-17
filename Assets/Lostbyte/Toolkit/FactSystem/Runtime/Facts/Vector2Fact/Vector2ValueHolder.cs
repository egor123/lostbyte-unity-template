using UnityEngine;

namespace Lostbyte.Toolkit.FactSystem
{
    public class Vector2ValueHolder : ValueHolder<Vector2>
    {
        public override IValueHolder Copy() => new Vector2ValueHolder() { Value = Value };
    }
}