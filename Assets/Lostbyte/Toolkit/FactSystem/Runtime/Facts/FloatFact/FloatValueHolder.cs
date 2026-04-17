namespace Lostbyte.Toolkit.FactSystem
{
    public class FloatValueHolder : ValueHolder<float>
    {
        public override IValueHolder Copy() => new FloatValueHolder() { Value = Value };
    }
}