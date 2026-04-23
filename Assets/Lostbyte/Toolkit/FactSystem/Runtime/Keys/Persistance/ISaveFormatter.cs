using System.IO;

namespace Lostbyte.Toolkit.FactSystem.Persistance
{
    public interface ISaveFormatter
    {
        void Serialize(Stream serializationStream, object graph);
        object Deserialize(Stream serializationStream);
    }
}