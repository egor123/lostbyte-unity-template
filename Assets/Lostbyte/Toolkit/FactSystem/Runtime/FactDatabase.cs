using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Lostbyte.Toolkit.FactSystem
{
    [CreateAssetMenu(fileName = "FactDatabase", menuName = "Facts/Fact Database")]
    public class FactDatabase : ScriptableObject
    {
        [SerializeField] private List<KeyContainer> m_rootKeys = new();
        [field: SerializeField] internal List<FactDefinition> FactStorage { get; private set; } = new();
        [field: SerializeField] internal List<EventDefinition> EventStorage { get; private set; } = new();

        private List<KeyContainer> _rootKeys;
        public List<KeyContainer> RootKeys
        {
            get
            {
#if UNITY_EDITOR
                return Application.isPlaying ? _rootKeys ??= m_rootKeys.ToList() : m_rootKeys;
#else
                return _rootKeys ??= m_rootKeys.ToList();
#endif
            }
        }

        private readonly Dictionary<string, KeyContainer> _keysByGuid = new();
        private readonly Dictionary<string, FactDefinition> _factByGuid = new();
        private readonly Dictionary<string, EventDefinition> _eventByGuid = new();
        private static FactDatabase _instance;
        public static FactDatabase Instance
        {
            get
            {
                if (_instance == null) Initialize();
                return _instance;
            }
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        static void OnRuntimeMethodLoad()
        {
            _instance = null;
            Initialize();
        }
        private static void Initialize()
        {
            _instance = FactSettings.TryLoad().Database;
            _instance.Init();
        }
        private void Init()
        {
            _rootKeys = null;
            _keysByGuid.Clear();
            foreach (var key in RootKeys)
            {
                key.Load();
                AddKey(key);
            }
            _factByGuid.Clear();
            foreach (var fact in FactStorage)
                _factByGuid[fact.Guid] = fact;
            _eventByGuid.Clear();
            foreach (var @event in EventStorage)
                _eventByGuid[@event.Guid] = @event;
            Debug.Log("Fact Database initialized");
        }
        public KeyContainer GetKey(string id) => _keysByGuid[id];
        public FactDefinition GetFact(string id) => _factByGuid[id];
        public EventDefinition GetEvent(string id) => _eventByGuid[id];

        private void AddKey(KeyContainer key)
        {
            _keysByGuid[key.Guid] = key;
            foreach (var child in key.Children)
                AddKey(child);
        }
        public KeyContainer RequestTempKey(string name, List<FactValueOverride> overrides = null)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Debug.LogError("Requesting temp key is only allowed at runtime!");
                return null;
            }
#endif
            var key = CreateInstance<KeyContainer>();
            key.name = FactUtils.GenerateValidName(name + "_temp");
            key.IsSerializable = false;
            RootKeys.Add(key);
            overrides?.ForEach(o =>
            {
                if (o.Fact != null && o.Wrapper != null)
                {
                    key.ValueOverrides.Add(o.Copy());
                    if (!key.Facts.Contains(o.Fact))
                        key.Facts.Add(o.Fact);
                }
            });
            key.Load();
            return key;
        }
    }
}