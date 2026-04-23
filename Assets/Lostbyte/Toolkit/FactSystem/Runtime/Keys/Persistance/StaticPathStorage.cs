using UnityEngine;

namespace Lostbyte.Toolkit.FactSystem.Persistance
{
    public class StaticPathStorage : ISaveStorage
    {
        [SerializeField] private string m_filePath;

        public void Write(ISaveFormatter formatter, object data)
        {
            string path = SaveStorageUtils.GetFullPath(m_filePath);
            var settings = FactSettings.TryLoad();
            SaveStorageUtils.Write(formatter, path + settings.SaveExtension, path + settings.TempExtension, data);
        }

        public T Read<T>(ISaveFormatter formatter)
        {
            string path = SaveStorageUtils.GetFullPath(m_filePath);
            var settings = FactSettings.TryLoad();
            return SaveStorageUtils.Read<T>(formatter, path + settings.SaveExtension, path + settings.TempExtension);
        }

        public void Delete()
        {
            string path = SaveStorageUtils.GetFullPath(m_filePath);
            var settings = FactSettings.TryLoad();
            SaveStorageUtils.Delete(path + settings.SaveExtension, path + settings.TempExtension);
        }

        public bool Exists() => SaveStorageUtils.Exists(SaveStorageUtils.GetFullPath(m_filePath));
    }
}