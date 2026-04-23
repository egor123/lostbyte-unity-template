using System;
using UnityEngine;

namespace Lostbyte.Toolkit.Localization
{
    [CreateAssetMenu(fileName = nameof(LocalizationSettings), menuName = "Localization/Settings")]
    public class LocalizationSettings : ScriptableObject
    {
        [SerializeField] private string[] m_locales = { "en-US" };
        [SerializeField] private string m_locale = "en-US";
        [SerializeField] private LocalizationDatabase m_database;

        public static ReadOnlySpan<string> Locales => Instance.m_locales;
        public static LocalizationDatabase Database => Instance.m_database;
        public static string Locale
        {
            get => Instance.m_locale;
            set
            {
                if (Array.IndexOf(Instance.m_locales, value) < 0)
                {
                    Debug.LogWarning($"Unknown locale: {value}!");
                }
                else
                {
                    Instance.m_locale = value;
                    Instance.onLocaleChange?.Invoke(value);
                }
            }
        }
        private Action<string> onLocaleChange;
        private static LocalizationSettings _instance;
        private static LocalizationSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = TryLoad();
                    if (_instance == null)
                    {
                        Debug.LogError("Localization Settings asset is missing!");
                    }
                }
                return _instance;
            }
        }
        internal static LocalizationSettings TryLoad() => Resources.Load<LocalizationSettings>("");

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void OnRuntimeMethodLoad() => _instance = null;
#endif
        public static void AddListenerOnLocaleChange(Action<string> callback) => Instance.onLocaleChange += callback;
        public static void RemoveListenerOnLocaleChange(Action<string> callback) => Instance.onLocaleChange -= callback;
    }
}
