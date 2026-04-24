using UnityEngine;

namespace Lostbyte.Toolkit.FactSystem.Persistance
{
    public class FactPathStorage : ISaveStorage
    {
        [SerializeField] private KeyContainer m_key;
        [SerializeField] private StringFactDifenition m_fact;
        public void Write(ISaveFormatter formatter, object data)
        {
            string path = SaveStorageUtils.GetFullPath(m_key.GetValue(m_fact));
            var settings = FactSettings.TryLoad();
            SaveStorageUtils.Write(formatter, path + settings.SaveExtension, path + settings.TempExtension, data);
        }

        public T Read<T>(ISaveFormatter formatter)
        {
            string path = SaveStorageUtils.GetFullPath(m_key.GetValue(m_fact));
            var settings = FactSettings.TryLoad();
            return SaveStorageUtils.Read<T>(formatter, path + settings.SaveExtension, path + settings.TempExtension);
        }

        public void Delete()
        {
            string path = SaveStorageUtils.GetFullPath(m_key.GetValue(m_fact));
            var settings = FactSettings.TryLoad();
            SaveStorageUtils.Delete(path + settings.SaveExtension, path + settings.TempExtension);
        }

        public bool Exists() => SaveStorageUtils.Exists(SaveStorageUtils.GetFullPath(m_key.GetValue(m_fact)));
    }
}