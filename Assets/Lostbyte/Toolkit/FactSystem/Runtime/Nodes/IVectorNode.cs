using UnityEngine;

namespace Lostbyte.Toolkit.FactSystem.Nodes
{
    public interface IVectorNode : INode
    {
        Vector4 Evaluate(IKeyContainer defaultKey);
    }
}