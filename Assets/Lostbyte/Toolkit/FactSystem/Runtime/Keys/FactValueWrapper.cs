using System;

namespace Lostbyte.Toolkit.FactSystem
{
    internal class FactValueWrapper<T> : IFactWrapper<T>
    {
        private Action OnChange;
        private Action<object> OnChangeRaw;
        private Action<T> OnChangeNew;
        private Action<T, T> OnChangeOldNew;
        private T _value = default;
        public FactValueWrapper(T defaultValue) => _value = defaultValue;
        public T Value
        {
            get => _value;
            set
            {
                if (_value == null && value == null) return;
                if (value != null && _value.Equals(value)) return;
                T old = _value;
                _value = value;
                OnChange?.Invoke();
                OnChangeRaw?.Invoke(_value);
                OnChangeNew?.Invoke(_value);
                OnChangeOldNew?.Invoke(old, _value);
            }
        }
        public object RawValue { get => Value; set => Value = (T)value; }
        public void Subscribe(Action<T> callback) => OnChangeNew += callback;
        public void Unsubscribe(Action<T> callback) => OnChangeNew -= callback;
        public void Subscribe(Action<T, T> callback) => OnChangeOldNew += callback;
        public void Unsubscribe(Action<T, T> callback) => OnChangeOldNew -= callback;
        public void Subscribe(Action callback) => OnChange += callback;
        public void Unsubscribe(Action callback) => OnChange -= callback;
        public void Subscribe(Action<object> callback) => OnChangeRaw += callback;
        public void Unsubscribe(Action<object> callback) => OnChangeRaw -= callback;
    }
}