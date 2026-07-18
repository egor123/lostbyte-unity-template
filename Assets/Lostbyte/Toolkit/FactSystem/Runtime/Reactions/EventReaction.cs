using System;
using UnityEngine;

namespace Lostbyte.Toolkit.FactSystem
{
    [Serializable]
    public abstract class EventReaction
    {
        [SerializeField, HideInInspector] private string _guid;
        public string Guid => string.IsNullOrEmpty(_guid) ? (_guid = System.Guid.NewGuid().ToString()) : _guid;

        public EventDefinition Event { get; private set; }
        public KeyContainer Key { get; private set; }
        protected IEventWrapper Wrapper { get; private set; }

        public virtual void Initialize(KeyContainer key, EventDefinition @event)
        {
            Wrapper?.Unsubscribe(Raise);

            Key = key;
            Event = @event;

            Wrapper = Key.GetWrapper(Event);
            Wrapper.Subscribe(Raise);
        }
        public virtual void Dispose()
        {
            Wrapper?.Unsubscribe(Raise);
            Wrapper = null;
        }
        public void Raise()
        {
            if (Key.PreConditionIsMet) OnRaise();
        }
        protected abstract void OnRaise();
        public virtual void OnLoad(object data) { }
        public virtual object OnSave() => null;
        public abstract EventReaction Copy();
    }
}
