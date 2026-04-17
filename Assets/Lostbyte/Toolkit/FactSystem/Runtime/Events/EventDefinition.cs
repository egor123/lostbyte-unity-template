using System;
using UnityEngine;

namespace Lostbyte.Toolkit.FactSystem
{
    public class EventDefinition : Definition
    {
        internal IEventWrapper GetValueWrapper() => new EventValueWrapper();
    }
}