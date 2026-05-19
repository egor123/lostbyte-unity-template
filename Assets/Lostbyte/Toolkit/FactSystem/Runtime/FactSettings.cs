using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Lostbyte.Toolkit.Common;
using Lostbyte.Toolkit.CustomEditor;
using UnityEngine;

namespace Lostbyte.Toolkit.FactSystem
{
    public class FactSettings : ScriptableObject
    {
        [SerializeField, Required] private FactDatabase m_Database;
        [ClearStatic] private static FactSettings _instance;
        [field: SerializeField] public string SaveExtension { get; private set; } = ".bin";
        [field: SerializeField] public string TempExtension { get; private set; } = ".tmp";

        public FactDatabase Database => m_Database;
        public static FactSettings TryLoad()
        {
            if (_instance != null) return _instance;
            FactSettings settings = Resources.LoadAll<FactSettings>("").FirstOrDefault();
            return settings;
        }
    }
}