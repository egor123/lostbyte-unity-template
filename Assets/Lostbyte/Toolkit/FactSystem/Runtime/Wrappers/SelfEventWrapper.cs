using System;
using UnityEngine;

namespace Lostbyte.Toolkit.FactSystem
{
    [Serializable]
    public class SelfEventWrapper : IEventWrapper
    {
        [SerializeField] private KeyReference m_key;
        [field: SerializeField] public EventDefinition Event { get; private set; }
        public SelfEventWrapper(KeyReference key, EventDefinition @event) => (m_key, Event) = (key, @event);
        public KeyContainer Key => m_key.Key;
        private IEventWrapper _wrapper;
        private IEventWrapper Wrapper
        {
            get
            {
                if (_wrapper == null)
                {
                    if (Key != null && Event != null)
                        _wrapper = Key.GetWrapper(Event);
                    else _wrapper = new EventValueWrapper();
                }
                return _wrapper;
            }
        }
        public void Subscribe(Action callback) => Wrapper.Subscribe(callback);
        public void Unsubscribe(Action callback) => Wrapper.Unsubscribe(callback);
        public void Raise() => Wrapper.Raise();
    }
}