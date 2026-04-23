using System.IO;
using UnityEngine;

namespace Lostbyte.Toolkit.FactSystem.Persistance
{
    public interface ISaveStorage
    {
        void Write(ISaveFormatter formatter, object data);
        T Read<T>(ISaveFormatter formatter);
        bool Exists();
        void Delete();
    }
}