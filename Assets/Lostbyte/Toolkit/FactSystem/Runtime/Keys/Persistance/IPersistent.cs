using System.Collections.Generic;

namespace Lostbyte.Toolkit.FactSystem.Persistance
{
    public interface IPersistent
    {
        void OnSave(Store store);
        void OnLoad(Store store);
    }
}
