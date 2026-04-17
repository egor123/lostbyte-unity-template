using System;
using UnityEngine;

namespace Lostbyte.Toolkit.FactSystem
{

    [Serializable]
    public class EventWrapper : IEventWrapper
    {
        [field: SerializeField] public KeyContainer Key { get; private set; }
        [field: SerializeField] public EventDefinition Event { get; private set; }
        public EventWrapper(KeyContainer key, EventDefinition @event) => (Key, Event) = (key, @event);
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