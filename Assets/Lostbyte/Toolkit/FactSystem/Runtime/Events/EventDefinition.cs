using System;
using UnityEngine;

namespace Lostbyte.Toolkit.FactSystem
{
    public class EventDefinition : Definition
    {
        [field: SerializeField, TextArea] public string Description { get; set; }
        internal IEventWrapper GetValueWrapper() => new EventValueWrapper();
    }
}