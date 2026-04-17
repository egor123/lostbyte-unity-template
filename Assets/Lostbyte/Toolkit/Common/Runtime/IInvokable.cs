namespace Lostbyte.Toolkit.Common
{
    public interface IInvokable
    {
        void Invoke();
    }
    public interface IInvokable<T>
    {
        void Invoke(T value);
    }
    public interface IInvokable<T, K>
    {
        K Invoke(T value);
    }
}
