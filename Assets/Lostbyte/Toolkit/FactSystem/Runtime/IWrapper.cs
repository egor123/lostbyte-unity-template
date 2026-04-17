using System;

namespace Lostbyte.Toolkit.FactSystem
{
    public interface IWrapper
    {
        void Subscribe(Action callback);
        void Unsubscribe(Action callback);
    }
}