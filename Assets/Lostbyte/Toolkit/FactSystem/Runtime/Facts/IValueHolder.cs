namespace Lostbyte.Toolkit.FactSystem
{
    public interface IValueHolder
    {
        object RawValue { get; set; }
        IValueHolder Copy();
    }
}