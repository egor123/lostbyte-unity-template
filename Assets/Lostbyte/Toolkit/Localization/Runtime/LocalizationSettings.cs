using System;
using System.Linq;
using Lostbyte.Toolkit.FactSystem;
using UnityEngine;

namespace Lostbyte.Toolkit.Localization
{
    [CreateAssetMenu(fileName = nameof(LocalizationSettings), menuName = "Localization/Settings")]
    public class LocalizationSettings : ScriptableObject
    {
        [SerializeField] private FactWrapper<Enum> m_localeFact;
        [SerializeField] private string[] m_locales = { "en-US" };
        // [SerializeField] private string m_locale = "en-US";
        [SerializeField] private LocalizationDatabase m_database;

        public static ReadOnlySpan<string> Locales => Instance.m_locales;
        public static LocalizationDatabase Database => Instance.m_database;
        public static string LocaleName => Instance.m_locales[Convert.ToInt32(Instance.m_localeFact.Value)];

        public static Enum Locale
        {
            get => Instance.m_localeFact.Value;
            set
            {
                if (Array.IndexOf(Instance.m_locales, value) < 0)
                {
                    Debug.LogWarning($"Unknown locale: {value}!");
                }
                else
                {
                    Instance.m_localeFact.Value = value;
                }
            }
        }
        private Action<Enum> onLocaleChange;
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
                        return null;
                    }
                    _instance.m_localeFact.Subscribe(_instance.OnChangeLocale);
                }
                return _instance;
            }
        }
        private void OnChangeLocale()
        {
            onLocaleChange?.Invoke(m_localeFact.Value);
        }
        internal static LocalizationSettings TryLoad() => Resources.LoadAll<LocalizationSettings>("").FirstOrDefault();

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void OnRuntimeMethodLoad() => _instance = null;
#endif
        public static void AddListenerOnLocaleChange(Action<Enum> callback) => Instance.onLocaleChange += callback;
        public static void RemoveListenerOnLocaleChange(Action<Enum> callback) => Instance.onLocaleChange -= callback;
    }
}
