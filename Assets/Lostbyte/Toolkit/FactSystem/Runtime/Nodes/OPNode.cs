using System;
using UnityEngine;

namespace Lostbyte.Toolkit.FactSystem.Nodes
{
    [Serializable]
    public struct OPNode : IBoolNode, INumericNode
    {
        [SerializeReference] public INode LNode, RNode;
        public string Op;
        readonly bool IValueNode<bool>.Evaluate(IKeyContainer defaultKey)
        {
            if (LNode is IBoolNode l1 && RNode is IBoolNode r1)
            {
                return Op switch
                {
                    "==" => l1.Evaluate(defaultKey) == r1.Evaluate(defaultKey),
                    "!=" => l1.Evaluate(defaultKey) != r1.Evaluate(defaultKey),
                    _ => false
                };
            }
            if (LNode is IStringNode l2 && RNode is IStringNode r2)
            {
                return Op switch
                {
                    "==" => l2.Evaluate(defaultKey) == r2.Evaluate(defaultKey),
                    "!=" => l2.Evaluate(defaultKey) != r2.Evaluate(defaultKey),
                    _ => false
                };
            }
            if (LNode is INumericNode l3 && RNode is INumericNode r3)
            {
                return Op switch
                {
                    "==" => l3.Evaluate(defaultKey) == r3.Evaluate(defaultKey),
                    "!=" => l3.Evaluate(defaultKey) != r3.Evaluate(defaultKey),
                    ">" => l3.Evaluate(defaultKey) > r3.Evaluate(defaultKey),
                    ">=" => l3.Evaluate(defaultKey) >= r3.Evaluate(defaultKey),
                    "<" => l3.Evaluate(defaultKey) < r3.Evaluate(defaultKey),
                    "<=" => l3.Evaluate(defaultKey) <= r3.Evaluate(defaultKey),
                    _ => false
                };
            }
            return false;
        }
        readonly float IValueNode<float>.Evaluate(IKeyContainer defaultKey)
        {
            if (LNode is INumericNode l && RNode is INumericNode r)
            {
                return Op switch
                {
                    "+" => l.Evaluate(defaultKey) + r.Evaluate(defaultKey),
                    "-" => l.Evaluate(defaultKey) - r.Evaluate(defaultKey),
                    "*" => l.Evaluate(defaultKey) * r.Evaluate(defaultKey),
                    "/" => l.Evaluate(defaultKey) / r.Evaluate(defaultKey),
                    "%" => l.Evaluate(defaultKey) % r.Evaluate(defaultKey),
                    "^" => Mathf.Pow(l.Evaluate(defaultKey), r.Evaluate(defaultKey)),
                    _ => 0
                };
            }
            return 0;
        }

        public readonly int Precedence => Op switch
        {
            "^" => 7,
            "*" or "/" or "%" => 6,
            "+" or "-" => 5,
            "<" or ">" or "<=" or ">=" => 4,
            "==" or "!=" => 3,
            _ => 0
        };

        public override readonly string ToString() => $"{NodeUtils.ToStringWithParens(this, LNode)} {Op} {NodeUtils.ToStringWithParens(this, RNode)}";
        public readonly Type ValueType => Op switch
        {
            "==" or "!=" or ">" or ">=" or "<" or "<=" => typeof(bool),
            "+" or "-" or "*" or "/" or "%" or "^" => typeof(float),
            _ => throw new Exception($"Unknown operator: {Op}")
        };
        public readonly void Validate() { if (LNode.ValueType != RNode.ValueType) throw new Exception("Types does not match"); }
        public readonly void Subscribe(IKeyContainer defaultKey, Action<object> callback) { LNode.Subscribe(defaultKey, callback); RNode.Subscribe(defaultKey, callback); }
        public readonly void Unsubscribe(IKeyContainer defaultKey, Action<object> callback) { LNode.Unsubscribe(defaultKey, callback); RNode.Unsubscribe(defaultKey, callback); }

    }
}