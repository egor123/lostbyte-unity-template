using System;
using UnityEngine;

namespace Lostbyte.Toolkit.FactSystem
{
    [Serializable]
    public struct FactValueOverride
    {
        [field: SerializeField] public FactDefinition Fact { get; set; }
        [field: SerializeField, SerializeReference] public IValueHolder Wrapper { get; set; }
        public FactValueOverride Copy() => new() { Fact = Fact, Wrapper = Wrapper.Copy() };
    }
}