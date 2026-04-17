using System;

namespace Lostbyte.Toolkit.FactSystem
{
    internal class EventValueWrapper : IEventWrapper
    {
        private Action OnRaise;
        public void Subscribe(Action callback) => OnRaise += callback;
        public void Unsubscribe(Action callback) => OnRaise -= callback;
        public void Raise() => OnRaise?.Invoke();
    }
}