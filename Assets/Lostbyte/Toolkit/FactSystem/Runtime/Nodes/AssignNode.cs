using System;
using Lostbyte.Toolkit.Common;
using UnityEngine;

namespace Lostbyte.Toolkit.FactSystem.Nodes
{
    [Serializable]
    public struct AssignNode : IActionNode
    {
        public FactNode Target;
        [field: SerializeReference] public INode ValueNode;

        public readonly Type ValueType => typeof(Empty);

        public readonly void Execute(IKeyContainer defaultKey)
        {
            var key = Target.Key != null ? Target.Key : defaultKey;
            if (Target.Fact is FloatFactDefinition fFact && ValueNode is INumericNode fNode) key.SetValue(fFact, fNode.Evaluate(defaultKey));
            else if (Target.Fact is IntFactDefinition iFact && ValueNode is INumericNode iNode) key.SetValue(iFact, (int)iNode.Evaluate(defaultKey));
            else if (Target.Fact is BoolFactDefinition bFact && ValueNode is IBoolNode bNode) key.SetValue(bFact, bNode.Evaluate(defaultKey));
            else if (Target.Fact is StringFactDefinition sFact && ValueNode is IStringNode sNode) key.SetValue(sFact, sNode.Evaluate(defaultKey));
            else if (Target.Fact is Vector2FactDefinition v2Fact && ValueNode is IVectorNode v2Node) key.SetValue(v2Fact, v2Node.Evaluate(defaultKey));
            else if (Target.Fact is Vector3FactDefinition v3Fact && ValueNode is IVectorNode v3Node) key.SetValue(v3Fact, v3Node.Evaluate(defaultKey));
            else if (Target.Fact is Vector4FactDefinition v4Fact && ValueNode is IVectorNode v4Node) key.SetValue(v4Fact, v4Node.Evaluate(defaultKey));
            else if (Target.Fact is ColorFactDefinition cFact && ValueNode is IVectorNode cNode) key.SetValue(cFact, cNode.Evaluate(defaultKey));
            else if (Target.Fact is EnumFactDefinition eFact && ValueNode is EnumNode eNode) key.SetValue(eFact, (Enum)Enum.GetValues(eFact.EnumType).GetValue((int)eNode.Evaluate(defaultKey)));
            else if (Target.Fact is EnumFactDefinition enFact && ValueNode is INumericNode enNode) key.SetValue(enFact, (Enum)Enum.GetValues(enFact.EnumType).GetValue((int)enNode.Evaluate(defaultKey)));
        }
        public readonly void Validate()
        {
            Target.Validate();
            ValueNode.Validate();
            if (Target.Fact is FloatFactDefinition && ValueNode is INumericNode) return;
            else if (Target.Fact is IntFactDefinition && ValueNode is INumericNode) return;
            else if (Target.Fact is BoolFactDefinition && ValueNode is IBoolNode) return;
            else if (Target.Fact is StringFactDefinition && ValueNode is IStringNode) return;
            else if (Target.Fact is Vector2FactDefinition && ValueNode is IVectorNode) return;
            else if (Target.Fact is Vector3FactDefinition && ValueNode is IVectorNode) return;
            else if (Target.Fact is Vector4FactDefinition && ValueNode is IVectorNode) return;
            else if (Target.Fact is ColorFactDefinition && ValueNode is IVectorNode) return;
            else if (Target.Fact is EnumFactDefinition && ValueNode is EnumNode) return;
            else if (Target.Fact is EnumFactDefinition && ValueNode is INumericNode) return;
            throw new Exception("Type missmatch");
        }

        public override readonly string ToString() => $"{Target} = {ValueNode}";
    }
}