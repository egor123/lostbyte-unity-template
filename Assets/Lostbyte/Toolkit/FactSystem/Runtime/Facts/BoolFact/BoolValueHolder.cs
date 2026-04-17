namespace Lostbyte.Toolkit.FactSystem
{
    public class BoolValueHolder : ValueHolder<bool>
    {
        public override IValueHolder Copy() => new BoolValueHolder() { Value = Value };
    }
}