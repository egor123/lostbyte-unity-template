using System;
using Lostbyte.Toolkit.Common;
using Lostbyte.Toolkit.FactSystem;
using UnityEngine;

namespace Lostbyte.Toolkit.Localization
{
    public interface ILocArg
    {
        object RawValue { get; }
        void Subscribe(Action callback);
        void Unsubscribe(Action callback);
    }
    public interface ILocArg<T> : ILocArg
    {
        T Value { get; }
    }

    [Serializable]
    public struct LocArg : ILocArg
    {
        [SerializeField, SerializeReference] private object m_staticValue;
        [SerializeField] private KeyContainer m_key;
        [SerializeField] private FactDefinition m_fact;
        [SerializeField] private bool m_isDynamic;

        private IFactWrapper _dynamicWrapper;
        public object RawValue => GetWrapper()?.RawValue ?? m_staticValue;
        public object Value => GetWrapper()?.RawValue ?? m_staticValue;
        public LocArg(object value) => (m_staticValue, _dynamicWrapper, m_isDynamic, m_key, m_fact) = (value, null, false, null, null);
        public LocArg(IFactWrapper wrapper) => (m_staticValue, _dynamicWrapper, m_isDynamic, m_key, m_fact) = (default, wrapper, true, null, null);
        public void Subscribe(Action callback) { if (m_isDynamic) GetWrapper().Subscribe(callback); }
        public void Unsubscribe(Action callback) { if (m_isDynamic) GetWrapper().Unsubscribe(callback); }
        private IFactWrapper GetWrapper() => m_isDynamic ? (_dynamicWrapper ??= m_key.GetWrapper(m_fact)) : null;
        public static implicit operator LocArg(string value) => new(value);
        public static implicit operator LocArg(int value) => new(value);
        public static implicit operator LocArg(float value) => new(value);
        public static implicit operator LocArg(bool value) => new(value);

    }
    [Serializable]
    public struct LocStringArg : ILocArg<string>
    {
        [SerializeField] private string m_staticValue;
        [SerializeField] private KeyContainer m_key;
        [SerializeField] private FactDefinition<string> m_fact;
        [SerializeField] private bool m_isDynamic;

        private IFactWrapper<string> _dynamicWrapper;
        public string Value => GetWrapper()?.Value ?? m_staticValue;
        public object RawValue => GetWrapper()?.RawValue ?? m_staticValue;
        public LocStringArg(string value) => (m_staticValue, _dynamicWrapper, m_isDynamic, m_key, m_fact) = (value, null, false, null, null);
        public LocStringArg(IFactWrapper<string> wrapper) => (m_staticValue, _dynamicWrapper, m_isDynamic, m_key, m_fact) = (default, wrapper, true, null, null);
        public void Subscribe(Action callback) { if (m_isDynamic) GetWrapper().Subscribe(callback); }
        public void Unsubscribe(Action callback) { if (m_isDynamic) GetWrapper().Unsubscribe(callback); }
        private IFactWrapper<string> GetWrapper() => m_isDynamic ? (_dynamicWrapper ??= m_key.GetWrapper(m_fact)) : null;

        public static implicit operator LocStringArg(string value) => new(value);
    }
    [Serializable]
    public struct LocIntArg : ILocArg<int>
    {
        [SerializeField] private int m_staticValue;
        [SerializeField] private KeyContainer m_key;
        [SerializeField] private FactDefinition<int> m_fact;
        [SerializeField] private bool m_isDynamic;

        private IFactWrapper<int> _dynamicWrapper;
        public int Value => GetWrapper()?.Value ?? m_staticValue;
        public object RawValue => GetWrapper()?.RawValue ?? m_staticValue;
        public LocIntArg(int value) => (m_staticValue, _dynamicWrapper, m_isDynamic, m_key, m_fact) = (value, null, false, null, null);
        public LocIntArg(IFactWrapper<int> wrapper) => (m_staticValue, _dynamicWrapper, m_isDynamic, m_key, m_fact) = (default, wrapper, true, null, null);
        public void Subscribe(Action callback) { if (m_isDynamic) GetWrapper().Subscribe(callback); }
        public void Unsubscribe(Action callback) { if (m_isDynamic) GetWrapper().Unsubscribe(callback); }
        private IFactWrapper<int> GetWrapper() => m_isDynamic ? (_dynamicWrapper ??= m_key.GetWrapper(m_fact)) : null;
        public static implicit operator LocIntArg(int value) => new(value);
    }
    [Serializable]
    public struct LocFloatArg : ILocArg<float>
    {
        [SerializeField] private float m_staticValue;
        [SerializeField] private KeyContainer m_key;
        [SerializeField] private FactDefinition<float> m_fact;
        [SerializeField] private bool m_isDynamic;

        private IFactWrapper<float> _dynamicWrapper;
        public float Value => GetWrapper()?.Value ?? m_staticValue;
        public object RawValue => GetWrapper()?.RawValue ?? m_staticValue;
        public LocFloatArg(float value) => (m_staticValue, _dynamicWrapper, m_isDynamic, m_key, m_fact) = (value, null, false, null, null);
        public LocFloatArg(IFactWrapper<float> wrapper) => (m_staticValue, _dynamicWrapper, m_isDynamic, m_key, m_fact) = (default, wrapper, true, null, null);
        public void Subscribe(Action callback) { if (m_isDynamic) GetWrapper().Subscribe(callback); }
        public void Unsubscribe(Action callback) { if (m_isDynamic) GetWrapper().Unsubscribe(callback); }
        private IFactWrapper<float> GetWrapper() => m_isDynamic ? (_dynamicWrapper ??= m_key.GetWrapper(m_fact)) : null;

        public static implicit operator LocFloatArg(float value) => new(value);
    }
    [Serializable]
    public struct LocBoolArg : ILocArg<bool>
    {
        [SerializeField] private bool m_staticValue;
        [SerializeField] private KeyContainer m_key;
        [SerializeField] private FactDefinition<bool> m_fact;
        [SerializeField] private bool m_isDynamic;

        private IFactWrapper<bool> _dynamicWrapper;
        public bool Value => GetWrapper()?.Value ?? m_staticValue;
        public object RawValue => GetWrapper()?.RawValue ?? m_staticValue;
        public LocBoolArg(bool value) => (m_staticValue, _dynamicWrapper, m_isDynamic, m_key, m_fact) = (value, null, false, null, null);
        public LocBoolArg(IFactWrapper<bool> wrapper) => (m_staticValue, _dynamicWrapper, m_isDynamic, m_key, m_fact) = (default, wrapper, true, null, null);
        public void Subscribe(Action callback) { if (m_isDynamic) GetWrapper().Subscribe(callback); }
        public void Unsubscribe(Action callback) { if (m_isDynamic) GetWrapper().Unsubscribe(callback); }
        private IFactWrapper<bool> GetWrapper() => m_isDynamic ? (_dynamicWrapper ??= m_key.GetWrapper(m_fact)) : null;

        public static implicit operator LocBoolArg(bool value) => new(value);
    }

    public static class LocArgExtensions
    {
        public static LocArg AsLocArg(this IFactWrapper wrapper) => new(wrapper);
        public static LocStringArg AsLocArg(this IFactWrapper<string> wrapper) => new(wrapper);
        public static LocIntArg AsLocArg(this IFactWrapper<int> wrapper) => new(wrapper);
        public static LocFloatArg AsLocArg(this IFactWrapper<float> wrapper) => new(wrapper);
        public static LocBoolArg AsLocArg(this IFactWrapper<bool> wrapper) => new(wrapper);

    }
}
