using System;

namespace Lostbyte.Toolkit.FactSystem
{
    public interface IFactWrapper : IWrapper
    {
        object RawValue { get; set; }
        void Subscribe(Action<object> callback);
        void Unsubscribe(Action<object> callback);
    }

    public interface IFactWrapper<T> : IFactWrapper
    {
        T Value { get; set; }
        void Subscribe(Action<T> callback);
        void Unsubscribe(Action<T> callback);
        void Subscribe(Action<T, T> callback);
        void Unsubscribe(Action<T, T> callback);
    }
}