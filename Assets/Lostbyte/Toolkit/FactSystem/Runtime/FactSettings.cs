using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Lostbyte.Toolkit.CustomEditor;
using UnityEngine;

namespace Lostbyte.Toolkit.FactSystem
{
    internal class FactSettings : ScriptableObject
    {
        [SerializeField] private FactDatabase m_Database;
        [SerializeField] private string m_saveExtension = ".bin";
        [SerializeField] private string m_tempExtension = ".tmp";
        [SerializeField, SerializeReference, UniqeReference] private SaveFormatter m_formatter;

        public FactDatabase Database => m_Database;
        public static FactSettings TryLoad()
        {
            // string filter = $"t:{nameof(FactSettings)}";
            FactSettings settings = Resources.LoadAll<FactSettings>("").FirstOrDefault();
            return settings;
        }

    }
    public abstract class SaveFormatter
    {
        public object Deserialize(Stream serializationStream)
        {
            throw new System.NotImplementedException();
        }

        public void Serialize(Stream serializationStream, object graph)
        {
            throw new System.NotImplementedException();
        }
    }
}