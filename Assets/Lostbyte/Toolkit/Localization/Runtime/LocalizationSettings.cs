using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Lostbyte.Toolkit.Common;
using Lostbyte.Toolkit.CustomEditor;
using Lostbyte.Toolkit.FactSystem;
using Lostbyte.Toolkit.Management;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Lostbyte.Toolkit.Localization
{
    [CreateAssetMenu(fileName = nameof(LocalizationSettings), menuName = "Localization/Settings")]
    public class LocalizationSettings : ScriptableObject
    {
        public const string k_addressableTableGroupName = "Localization Tables";
        public const string k_addressableAssetGroupName = "Localized Data";

        [SerializeField, Required] private LocalizationDatabase m_database;
        [SerializeField] private FactWrapper<string> m_localeFact;
        [SerializeField] private string[] m_locales = { };

        public static ReadOnlySpan<string> Locales => Instance.m_locales;
        public static LocalizationDatabase Database => Instance.m_database;
        public static string Locale => Database.CurrentLocale;

        private static LocalizationSettings _instance;
        public static LocalizationSettings Instance
        {
            get
            {
#if UNITY_EDITOR
                if (!Application.isPlaying) return TryLoad();
#endif
                if (_instance == null) Init();
                return _instance;
            }
        }
#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ClearState()
        {
            if (_instance == null) return;
            _instance.m_localeFact.Unsubscribe(_instance.m_database.ChangeLocale);
            _instance = null;
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void Init()
        {
            if (_instance == null)
            {
                _instance = TryLoad();
                if (_instance == null)
                {
                    Print.MError("Localization Settings asset is missing!");
                    return;
                }

                var db = Database;
                db.m_targetLocale = null;
                db.CurrentLocale = null;

                _instance.m_localeFact.Subscribe(db.ChangeLocale);
                if (Application.isPlaying)
                {
                    Bootstrapper.RegisterTask(new SetupLocalesTask());
                    Bootstrapper.RegisterTask(new SetupLocalizationDatabaseTask());
                }
                Print.MLog("Initiated");
            }
        }
        private class SetupLocalesTask : IBootstrapTask
        {
            public LocalizationDatabase DB;
            public int Priority => 0;
            public BootstrapResult Execute() => LoadLocalesTask();

            private async Task LoadLocalesTask()
            {
#if UNITY_EDITOR
                Addressables.InitializeAsync().WaitForCompletion();
#else
                await Addressables.InitializeAsync().Task;
#endif

                var discoveredLocales = new HashSet<string>();
                foreach (var locator in Addressables.ResourceLocators)
                {
                    foreach (var key in locator.Keys)
                    {
                        if (key is string keyString && keyString.StartsWith("LOCALE_"))
                        {
                            string cleanLocale = keyString[7..];
                            discoveredLocales.Add(cleanLocale);
                        }
                    }
                }
                string[] availableLocales = discoveredLocales.ToArray();
                Instance.m_locales = availableLocales;
                if (availableLocales.Length == 0)
                {
                    Print.MWarn("No locales found!");
                    return;
                }
                Print.MLog($"Discovered {availableLocales.Length} locales: {string.Join(", ", availableLocales)}");
            }
        }
        private class SetupLocalizationDatabaseTask : IBootstrapTask
        {
            public int Priority => 1;
            public BootstrapResult Execute()
            {
#if UNITY_EDITOR
                Addressables.InitializeAsync().WaitForCompletion();
                Database.ChangeLocaleSync(_instance.m_localeFact.Value);
                return BootstrapResult.Completed;
#else
                return Database.ChangeLocaleAsync(_instance.m_localeFact.Value);
#endif
            }
        }



        internal static LocalizationSettings TryLoad() => Resources.LoadAll<LocalizationSettings>("").FirstOrDefault();
        public static void AddListenerOnLocaleChange(Action<string> callback) => Database.OnLocalizationChanged += callback;
        public static void RemoveListenerOnLocaleChange(Action<string> callback) => Database.OnLocalizationChanged -= callback;
    }
}
