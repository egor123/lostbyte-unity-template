using System;

namespace Lostbyte.Toolkit.FactSystem.Nodes
{
    public interface IValueNode<T> : INode
    {
        T Evaluate(IKeyContainer defaultKey);
    }
}