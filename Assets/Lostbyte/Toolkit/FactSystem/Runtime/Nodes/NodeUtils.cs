namespace Lostbyte.Toolkit.FactSystem.Nodes
{
    public static class NodeUtils
    {
        internal static string ToStringWithParens(INode self, INode node) => node.Precedence < self.Precedence ? $"({node})" : node.ToString();
    }
}