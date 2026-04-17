namespace Lostbyte.Toolkit.FactSystem.Nodes
{
    public interface IBoolNode : INode
    {
        bool Evaluate(IKeyContainer defaultKey);
    }
}