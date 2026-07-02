using System;
using System.Collections.Generic;
using System.Linq;
using Lostbyte.Toolkit.Common;
using UnityEngine;

namespace Lostbyte.Toolkit.FactSystem
{
    [Serializable]
    public struct EventRegistration
    {
        public EventDefinition Event;
        [SerializeReference] public List<EventReaction> Reactions;

        public readonly EventRegistration Copy() => new()
        {
            Event = Event,
            Reactions = Reactions.Select(r => r.Copy()).ToList()
        };
        public override readonly bool Equals(object obj) => obj is EventRegistration reg && reg.Event == Event;
        public override readonly int GetHashCode() => Event.GetHashCode();
    }
}
