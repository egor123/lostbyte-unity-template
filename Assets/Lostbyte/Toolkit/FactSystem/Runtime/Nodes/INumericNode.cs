namespace Lostbyte.Toolkit.FactSystem.Nodes
{
    public interface INumericNode : INode
    {
        float Evaluate(IKeyContainer defaultKey);
    }
}