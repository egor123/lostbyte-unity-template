using System;
using Lostbyte.Toolkit.CustomEditor;
using UnityEngine;

namespace Lostbyte.Toolkit.Common
{
    [Serializable]
    public class Empty
    {
        // #if UNITY_EDITOR
        [SerializeField, Hide] private bool m_v; // used to fix some serialization issues
        // #endif
        public override string ToString() => "Empty";
        public override bool Equals(object obj) => obj is Empty;
        public override int GetHashCode() => base.GetHashCode();
        public static Empty Value { get; } = new Empty();
    }
}
