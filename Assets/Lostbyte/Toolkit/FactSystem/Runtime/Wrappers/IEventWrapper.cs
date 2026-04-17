using System;

namespace Lostbyte.Toolkit.FactSystem
{
    public interface IEventWrapper : IWrapper
    {
        public void Raise();
    }
}