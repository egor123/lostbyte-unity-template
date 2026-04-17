using System;

namespace Lostbyte.Toolkit.FactSystem.Nodes
{
    [Serializable]
    public struct StringNode : IStringNode
    {
        public string Value;
        public readonly string Evaluate(IKeyContainer defaultKey) => Value;
        public override readonly string ToString() => Value;
        public readonly Type ValueType => typeof(string);
    }
}