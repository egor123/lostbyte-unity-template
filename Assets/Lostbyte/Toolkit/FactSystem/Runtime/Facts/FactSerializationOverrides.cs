using System;
using UnityEngine;

namespace Lostbyte.Toolkit.FactSystem
{
    [Serializable]
    public struct FactSerializationOverride
    {
        [field: SerializeField] public FactDefinition Fact { get; set; }
        [field: SerializeField] public bool IsSerializable { get; set; }
    }
}