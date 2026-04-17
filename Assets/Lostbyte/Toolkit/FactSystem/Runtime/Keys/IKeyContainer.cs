using System;

namespace Lostbyte.Toolkit.FactSystem
{
    public interface IKeyContainer
    {
        string Name { get; }
        KeyContainer Key { get; }
        void SetValue<T>(FactDefinition<T> fact, T value);
        T GetValue<T>(FactDefinition<T> fact);
        void Raise(EventDefinition @event);
        void AddOnFactAddedListener(Action<FactDefinition> callback);
        void RemoveOnFactAddedListener(Action<FactDefinition> callback);
        void AddOnChangeListener(Action callback);
        void RemoveOnChangeListener(Action callback);
        void Subscribe(FactDefinition fact, Action<object> callback);
        void Unsubscribe(FactDefinition fact, Action<object> callback);
        void Subscribe<T>(FactDefinition<T> fact, Action callback);
        void Unsubscribe<T>(FactDefinition<T> fact, Action callback);
        void Subscribe<T>(FactDefinition<T> fact, Action<T> callback);
        void Unsubscribe<T>(FactDefinition<T> fact, Action<T> callback);
        void Subscribe<T>(FactDefinition<T> fact, Action<T, T> callback);
        void Unsubscribe<T>(FactDefinition<T> fact, Action<T, T> callback);
        void Subscribe(EventDefinition @event, Action callback);
        void Unsubscribe(EventDefinition @event, Action callback);
        IFactWrapper GetWrapper(FactDefinition fact);
        IFactWrapper<T> GetWrapper<T>(FactDefinition<T> fact);
        IEventWrapper GetWrapper(EventDefinition @event);
        IWrapper GetWrapper(Definition def);
    }
}