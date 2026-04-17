namespace Lostbyte.Toolkit.FactSystem
{
    public class IntValueHolder : ValueHolder<int>
    {
        public override IValueHolder Copy() => new IntValueHolder() { Value = Value };
    }
}