using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lostbyte.Toolkit.FactSystem
{
    [SelectionBase]
    public class KeyReference : MonoBehaviour, IKeyContainer
    {
        public string Name => Key.Name;
        [field: SerializeField] public KeyContainer ParentKey { get; private set; }
        [SerializeField] private KeyContainer m_key;
        [field: SerializeField] public List<FactValueOverride> ValueOverrides { get; private set; } = new();

        public KeyContainer Key
        {
            get
            {
                if (m_key == null)
                {
#if UNITY_EDITOR
                    if (!Application.isPlaying) return m_key;
#endif
                    if (ParentKey != null) m_key = ParentKey.RequestTempKey(gameObject.name, ValueOverrides);
                    else m_key = FactDatabase.Instance.RequestTempKey(gameObject.name, ValueOverrides);
                }
                return m_key;
            }
        }
        public void AddOnChangeListener(Action callback) => Key.AddOnChangeListener(callback);
        public void AddOnFactAddedListener(Action<FactDefinition> callback) => Key.AddOnFactAddedListener(callback);
        public T GetValue<T>(FactDefinition<T> fact) => Key.GetValue(fact);
        public void RemoveOnChangeListener(Action callback) => Key.RemoveOnChangeListener(callback);
        public void RemoveOnFactAddedListener(Action<FactDefinition> callback) => Key.RemoveOnFactAddedListener(callback);
        public void SetValue<T>(FactDefinition<T> fact, T value) => Key.SetValue(fact, value);
        public void Subscribe(FactDefinition fact, Action<object> callback) => Key.Subscribe(fact, callback);
        public void Subscribe<T>(FactDefinition<T> fact, Action callback) => Key.Subscribe(fact, callback);
        public void Subscribe<T>(FactDefinition<T> fact, Action<T> callback) => Key.Subscribe(fact, callback);
        public void Subscribe<T>(FactDefinition<T> fact, Action<T, T> callback) => Key.Subscribe(fact, callback);
        public void Unsubscribe(FactDefinition fact, Action<object> callback) => Key.Unsubscribe(fact, callback);
        public void Unsubscribe<T>(FactDefinition<T> fact, Action callback) => Key.Unsubscribe(fact, callback);
        public void Unsubscribe<T>(FactDefinition<T> fact, Action<T> callback) => Key.Unsubscribe(fact, callback);
        public void Unsubscribe<T>(FactDefinition<T> fact, Action<T, T> callback) => Key.Unsubscribe(fact, callback);
        public IFactWrapper GetWrapper(FactDefinition fact) => Key.GetWrapper(fact);
        public IFactWrapper<T> GetWrapper<T>(FactDefinition<T> fact) => Key.GetWrapper(fact);
        public void Raise(EventDefinition @event) => Key.Raise(@event);
        public void Subscribe(EventDefinition @event, Action callback) => Key.Subscribe(@event, callback);
        public void Unsubscribe(EventDefinition @event, Action callback) => Key.Unsubscribe(@event, callback);
        public IEventWrapper GetWrapper(EventDefinition @event) => Key.GetWrapper(@event);
        public IWrapper GetWrapper(Definition def) => Key.GetWrapper(def);
    }
}