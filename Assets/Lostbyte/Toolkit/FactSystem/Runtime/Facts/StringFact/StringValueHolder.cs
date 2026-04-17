namespace Lostbyte.Toolkit.FactSystem
{
    public class StringValueHolder : ValueHolder<string>
    {
        public override IValueHolder Copy() => new StringValueHolder() { Value = Value };
    }
}