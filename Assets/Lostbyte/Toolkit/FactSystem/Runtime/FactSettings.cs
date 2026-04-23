using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Lostbyte.Toolkit.CustomEditor;
using UnityEngine;

namespace Lostbyte.Toolkit.FactSystem
{
    public class FactSettings : ScriptableObject
    {
        [SerializeField] private FactDatabase m_Database;
        [field: SerializeField] public string SaveExtension { get; private set; } = ".bin";
        [field: SerializeField] public string TempExtension { get; private set; } = ".tmp";

        public FactDatabase Database => m_Database;
        public static FactSettings TryLoad()
        {
            // string filter = $"t:{nameof(FactSettings)}";
            FactSettings settings = Resources.LoadAll<FactSettings>("").FirstOrDefault();
            return settings;
        }

    }
}