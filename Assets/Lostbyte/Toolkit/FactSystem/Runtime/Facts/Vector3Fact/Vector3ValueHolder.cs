using UnityEngine;

namespace Lostbyte.Toolkit.FactSystem
{
    public class Vector3ValueHolder : ValueHolder<Vector3>
    {
        public override IValueHolder Copy() => new Vector3ValueHolder() { Value = Value };
    }
}