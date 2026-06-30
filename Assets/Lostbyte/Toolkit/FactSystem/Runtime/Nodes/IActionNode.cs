using System;
using Lostbyte.Toolkit.Common;

namespace Lostbyte.Toolkit.FactSystem.Nodes
{
    public interface IActionNode : INode
    {
        void Execute(IKeyContainer defaultKey);
    }
}