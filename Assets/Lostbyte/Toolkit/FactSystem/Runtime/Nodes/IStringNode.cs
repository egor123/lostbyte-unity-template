namespace Lostbyte.Toolkit.FactSystem.Nodes
{
    public interface IStringNode : INode
    {
        string Evaluate(IKeyContainer defaultKey);
    }
}