using System;
using UnityEngine;

namespace Lostbyte.Toolkit.FactSystem.Nodes
{
    [Serializable]
    public struct OrNode : IBoolNode
    {
        [SerializeReference] public IBoolNode LNode, RNode;
        public readonly bool Evaluate(IKeyContainer defaultKey) => LNode.Evaluate(defaultKey) || RNode.Evaluate(defaultKey);
        public readonly int Precedence => 1;
        public override readonly string ToString() => $"{NodeUtils.ToStringWithParens(this, LNode)} or {NodeUtils.ToStringWithParens(this, RNode)}";
        public readonly Type ValueType => typeof(bool);
        public readonly void Validate() { if (LNode.ValueType != typeof(bool) || RNode.ValueType != typeof(bool)) throw new Exception("Type is not bool"); }
        public readonly void Subscribe(IKeyContainer defaultKey, Action<object> callback) { LNode.Subscribe(defaultKey, callback); RNode.Subscribe(defaultKey, callback); }
        public readonly void Unsubscribe(IKeyContainer defaultKey, Action<object> callback) { LNode.Unsubscribe(defaultKey, callback); RNode.Unsubscribe(defaultKey, callback); }
    }
}