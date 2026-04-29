using System.Collections;
using System.Collections.Generic;
using Lostbyte.Toolkit.FactSystem;
using UnityEngine;

namespace Lostbyte.Toolkit.Localization
{
    public readonly struct LocArg<T>
    {
        public readonly object Value;
        private LocArg(object value) => Value = value;

        public static implicit operator LocArg<T>(T value) => new(value);
        public static LocArg<T> From(IFactWrapper<T> fact) => new(fact);

    }
}
