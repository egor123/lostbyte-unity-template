using System;

namespace Lostbyte.Toolkit.FactSystem.Nodes
{
    public interface INode
    {
        int Precedence => int.MaxValue;
        Type ValueType { get; }
        void Validate() { }
        void Subscribe(IKeyContainer defaultKey, Action<object> callback) { }
        void Unsubscribe(IKeyContainer defaultKey, Action<object> callback) { }

    }
}