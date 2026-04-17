using System;
using UnityEngine;

namespace Lostbyte.Toolkit.FactSystem
{
    public abstract class Definition : ScriptableObject
    {
        [field: SerializeField] public string Guid { get; internal set;  }
    }
}