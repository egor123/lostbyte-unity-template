using System;
using UnityEngine;

namespace Lostbyte.Toolkit.FactSystem.Nodes
{
    [Serializable]
    public struct BoolNode : IBoolNode
    {
        [field: SerializeField] public bool Value { get; set; }
        public readonly bool Evaluate(IKeyContainer defaultKey) => Value;
        public override readonly string ToString() => Value.ToString().ToLower();
        public readonly Type ValueType => typeof(bool);

    }
}