using System;
using UnityEngine;

namespace Lostbyte.Toolkit.FactSystem
{
    public class EnumValueHolder : ValueHolder<Enum>
    {
        public override IValueHolder Copy() => new EnumValueHolder() { Value = Value };
        [field: SerializeField, SerializeReference, SerializedEnum] public override Enum Value { get; set; }
    }
}