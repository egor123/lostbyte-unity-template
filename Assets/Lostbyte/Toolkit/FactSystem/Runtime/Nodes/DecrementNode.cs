using System;
using Lostbyte.Toolkit.Common;

namespace Lostbyte.Toolkit.FactSystem.Nodes
{
    [Serializable]
    public struct DecrementNode : IActionNode
    {
        public FactNode Target;

        public readonly Type ValueType => typeof(Empty);

        public readonly void Execute(IKeyContainer defaultKey)
        {
            var key = Target.Key != null ? Target.Key : defaultKey;
            if (Target.Fact is FloatFactDefinition fFact) key.SetValue(fFact, key.GetValue(fFact) - 1);
            else if (Target.Fact is IntFactDefinition iFact) key.SetValue(iFact, key.GetValue(iFact) - 1);
            else if (Target.Fact is EnumFactDefinition eFact) key.SetValue(eFact, (Enum)Enum.GetValues(eFact.EnumType).GetValue(Convert.ToInt32(key.GetValue(eFact)) - 1));
        }
        public readonly void Validate()
        {
            Target.Validate();
            if (Target.Fact is FloatFactDefinition) return;
            else if (Target.Fact is IntFactDefinition) return;
            else if (Target.Fact is EnumFactDefinition) return;
            throw new Exception("Type missmatch");
        }
        public override readonly string ToString() => $"{Target}--";
    }
}