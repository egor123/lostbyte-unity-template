using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lostbyte.Toolkit.FactSystem.Persistance
{
    [DefaultExecutionOrder(-100)]
    public class KeyLoader : MonoBehaviour
    {
        [field: SerializeField] public KeyContainer Key { get; private set; }

        [SerializeField] private bool m_loadOnAwake = true;
        [SerializeField] private bool m_saveOnDestroy = true;
        [SerializeField] private bool m_saveOnQuit = true;
        [SerializeField] private bool m_saveOnPause = false;

        private void Awake()
        {
            if (m_loadOnAwake)
                Key.Load(null, true);
        }

        private void OnDestroy()
        {
            if (m_saveOnDestroy)
                SaveSafe();
        }

        private void OnApplicationQuit()
        {
            if (m_saveOnQuit)
                SaveSafe();
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause && m_saveOnPause)
                SaveSafe();
        }

        // --------------------------------------------------

        public void Load() => Key.Load(null, true);

        public void Save() => Key.Save();

        public void Clear() => Key.Clear();

        // --------------------------------------------------

        private bool _hasSaved;

        private void SaveSafe()
        {
            if (_hasSaved) return; // prevent duplicate saves in same frame/lifecycle
            _hasSaved = true;
            Key.Save();
        }
    }
}
