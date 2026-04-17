using UnityEngine;

namespace Lostbyte.Toolkit.FactSystem
{
    public abstract class ValueHolder<T> : IValueHolder
    {
        [field: SerializeField] public virtual T Value { get; set; }
        public object RawValue { get => Value; set => Value = (T)value; }
        public abstract IValueHolder Copy();
    }
}