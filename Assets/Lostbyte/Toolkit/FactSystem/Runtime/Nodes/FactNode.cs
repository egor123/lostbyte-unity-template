using System;
using UnityEngine;

namespace Lostbyte.Toolkit.FactSystem.Nodes
{
    [Serializable]
    public struct FactNode : IBoolNode, IStringNode, INumericNode, IVectorNode
    {
        public KeyContainer Key;
        public FactDefinition Fact;
        private IFactWrapper _wrapper;
        private readonly bool HasAnyKey(IKeyContainer defaultKey) => Key != null || defaultKey != null;
        private IFactWrapper GetWrapper(IKeyContainer defaultKey) => Key != null ? _wrapper ??= Key.Key.GetWrapper(Fact) : defaultKey?.Key.GetWrapper(Fact);
        public void Subscribe(IKeyContainer defaultKey, Action<object> callback) { if (HasAnyKey(defaultKey)) GetWrapper(defaultKey).Subscribe(callback); }
        public void Unsubscribe(IKeyContainer defaultKey, Action<object> callback) { if (HasAnyKey(defaultKey)) GetWrapper(defaultKey).Unsubscribe(callback); }
        public override readonly string ToString() => $"{(Key != null ? Key.Name : "this")}[{Fact.name}]";
        public readonly Type ValueType
        {
            get
            {
                if (Fact.GenericType == typeof(string)) return typeof(string);
                if (Fact.GenericType == typeof(int)) return typeof(float);
                if (Fact.GenericType == typeof(float)) return typeof(float);
                if (Fact.GenericType == typeof(bool)) return typeof(bool);
                if (Fact.GenericType == typeof(Enum)) return typeof(float);
                throw new Exception("Unsupported fact type");
            }
        }
        public readonly void Validate() { if (Fact == null) throw new Exception("Unknown fact name"); }

        public bool Evaluate(IKeyContainer defaultKey)
        {
            if (!HasAnyKey(defaultKey)) return false;
            var wrapper = GetWrapper(defaultKey);
            if (wrapper is FactValueWrapper<bool> bw) return bw.Value;
            return false;
        }

        string IStringNode.Evaluate(IKeyContainer defaultKey)
        {
            if (!HasAnyKey(defaultKey)) return "";
            var wrapper = GetWrapper(defaultKey);
            if (wrapper is FactValueWrapper<string> sw) return sw.Value;
            return "";
        }

        float INumericNode.Evaluate(IKeyContainer defaultKey)
        {
            if (!HasAnyKey(defaultKey)) return 0;
            var wrapper = GetWrapper(defaultKey);
            if (wrapper is FactValueWrapper<float> fw) return fw.Value;
            if (wrapper is FactValueWrapper<int> iw) return iw.Value;
            if (wrapper is FactValueWrapper<Enum> ew) return Convert.ToInt32(ew.Value);
            return 0;
        }

        Vector4 IVectorNode.Evaluate(IKeyContainer defaultKey)
        {
            if (!HasAnyKey(defaultKey)) return Vector4.zero;
            var wrapper = GetWrapper(defaultKey);
            if (wrapper is FactValueWrapper<Vector2> v2w) return v2w.Value;
            if (wrapper is FactValueWrapper<Vector3> v3w) return v3w.Value;
            if (wrapper is FactValueWrapper<Vector4> v4w) return v4w.Value;
            if (wrapper is FactValueWrapper<Color> cw) return cw.Value;
            return Vector4.zero;
        }
    }
}