using System;
using UnityEngine;

namespace Lostbyte.Toolkit.FactSystem.Nodes
{
    [Serializable]
    public struct VectorNode : IVectorNode
    {
        public Vector4 Value { get; set; }
        public readonly Vector4 Evaluate(IKeyContainer defaultKey) => Value;
        public override readonly string ToString() => Value.ToString();
        public readonly Type ValueType => typeof(bool);

    }
}