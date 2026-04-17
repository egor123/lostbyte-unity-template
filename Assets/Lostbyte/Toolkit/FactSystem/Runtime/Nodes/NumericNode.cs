using System;

namespace Lostbyte.Toolkit.FactSystem.Nodes
{
    [Serializable]
    public struct NumericNode : INumericNode
    {
        public float Value;
        public readonly float Evaluate(IKeyContainer key) => Value;
        public override readonly string ToString() => Value.ToString().Replace(',', '.');
        public readonly Type ValueType => typeof(float);
    }
}