using System;
using System.Collections.Generic;
using System.Linq;
using Lostbyte.Toolkit.Common;
using Lostbyte.Toolkit.CustomEditor;
using UnityEngine;

namespace Lostbyte.Toolkit.FactSystem
{
    [Serializable]
    public struct FactRegistration
    {
        public FactDefinition Fact;
        [SerializeReference] public IValueHolder ValueOverride;
        public Optional<bool> IsSerializable;
        [SerializeReference] public List<FactReaction> Reactions;

        public readonly FactRegistration Copy() => new()
        {
            Fact = Fact,
            ValueOverride = ValueOverride?.Copy() ?? null,
            IsSerializable = IsSerializable,
            Reactions = Reactions.Select(r => r.Copy()).ToList()
        };
        public override readonly bool Equals(object obj) => obj is FactRegistration reg && reg.Fact == Fact;
        public override readonly int GetHashCode() => Fact.GetHashCode();
    }
}
