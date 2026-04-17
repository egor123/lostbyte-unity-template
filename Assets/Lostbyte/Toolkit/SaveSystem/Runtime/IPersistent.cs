namespace Lostbyte.Toolkit.SaveSystem
{
    public interface IPersistent
    {
        void OnSave();
        void OnLoad();
    }
}
