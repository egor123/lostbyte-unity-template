using System;
using UnityEngine;

namespace Lostbyte.Toolkit.FactSystem
{
    public abstract class FactDefinition : Definition
    {
        [field: SerializeField] public bool IsSerializable { get; set; }
        [field: SerializeField, TextArea] public string Description { get; set; }
        public abstract Type GenericType { get; }
        public abstract object DefaultValueRaw { get; set; }
        internal abstract IFactWrapper GetValueWrapper();
    }
    public abstract class FactDefinition<T> : FactDefinition
    {
        [field: SerializeField] public virtual T DefaultValue { get; set; }
        public override Type GenericType => typeof(T);
        public override object DefaultValueRaw { get => DefaultValue; set => DefaultValue = (T)value; }
        internal override IFactWrapper GetValueWrapper() => new FactValueWrapper<T>(DefaultValue);
    }
}