using System;
using UnityEngine;

namespace Lostbyte.Toolkit.FactSystem
{

    [Serializable]
    public class FactWrapper<T> : IFactWrapper<T>
    {
        [field: SerializeField] public KeyContainer Key { get; private set; }
        [field: SerializeField] public FactDefinition<T> Fact { get; private set; }
        public FactWrapper(KeyContainer key, FactDefinition<T> fact) => (Key, Fact) = (key, fact);
        private IFactWrapper<T> _wrapper;
        private IFactWrapper<T> Wrapper
        {
            get
            {
                if (_wrapper == null)
                {
                    if (Key != null && Fact != null)
                        _wrapper = Key.GetWrapper(Fact);
                    else
                        _wrapper = new FactValueWrapper<T>(default);
                }
                return _wrapper;
            }
        }
        public T Value
        {
            get => Wrapper.Value;
            set => Wrapper.Value = value;
        }
        public object RawValue
        {
            get => Wrapper.RawValue;
            set => Wrapper.RawValue = value;
        }
        public void Subscribe(Action callback) => Wrapper.Subscribe(callback);
        public void Unsubscribe(Action callback) => Wrapper.Unsubscribe(callback);
        public void Subscribe(Action<T> callback) => Wrapper.Subscribe(callback);
        public void Unsubscribe(Action<T> callback) => Wrapper.Unsubscribe(callback);
        public void Subscribe(Action<T, T> callback) => Wrapper.Subscribe(callback);
        public void Unsubscribe(Action<T, T> callback) => Wrapper.Unsubscribe(callback);
        public void Subscribe(Action<object> callback) => Wrapper.Subscribe(callback);
        public void Unsubscribe(Action<object> callback) => Wrapper.Unsubscribe(callback);
    }
}