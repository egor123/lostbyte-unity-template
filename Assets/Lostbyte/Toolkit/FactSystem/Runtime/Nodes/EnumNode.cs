using System;
using UnityEngine;

namespace Lostbyte.Toolkit.FactSystem.Nodes
{
    [Serializable]
    public struct EnumNode : INumericNode
    {
        [NonSerialized] public string ValueName;
        [SerializeReference] public Enum Value;
        public readonly float Evaluate(IKeyContainer defaultKey) => Convert.ToInt32(Value);
        public override readonly string ToString() => Value.ToString();
        public readonly Type ValueType => typeof(float);
        public readonly void Validate() { if (Value == null) throw new Exception("Enum value is null"); }

    }
}