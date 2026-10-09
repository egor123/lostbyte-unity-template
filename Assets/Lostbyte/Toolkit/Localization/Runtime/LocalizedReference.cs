using System;
using System.Collections.Generic;
using System.Linq;
using Lostbyte.Toolkit.Common;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Lostbyte.Toolkit.Localization
{
    [Serializable]
    public abstract class LocRef : IDisposable, IValidatable
    {
        [field: SerializeField] public string TableId { get; protected set; }
        [field: SerializeField] public string KeyId { get; protected set; }
        [SerializeField, SerializeReference] protected ILocArg[] m_args;
        public IReadOnlyCollection<ILocArg> Args => m_args;
        private object[] _cachedArgs;
        protected bool _isDynamicInitialized;
        protected int _subscriberCount;

        public abstract bool IsArray { get; }
        public abstract Type[] Types { get; }

        public LocRef(string tableId, string keyId, params ILocArg[] args) => (TableId, KeyId, m_args) = (tableId, keyId, args);

        protected object[] GetArgs()
        {
            UpdateArgs();
            return _cachedArgs;
        }

        public void SetArg(int idx, ILocArg arg)
        {
            m_args[idx] = arg;
            UpdateArgs();
        }

        private void UpdateArgs()
        {
            if (m_args == null || m_args.Length == 0) return;

            if (_cachedArgs == null || _cachedArgs.Length != m_args.Length)
                _cachedArgs = new object[m_args.Length];

            for (int i = 0; i < m_args.Length; i++)
                _cachedArgs[i] = m_args[i]?.RawValue;
        }

        protected void HandleSubscribe()
        {
            _subscriberCount++;
            if (_subscriberCount == 1)
            {
                EnsureDynamicInit();
                ExecuteRefresh();
            }
        }

        protected void HandleUnsubscribe()
        {
            _subscriberCount--;
            if (_subscriberCount <= 0)
            {
                _subscriberCount = 0;
                EnsureDynamicClear();
            }
        }

        protected void EnsureDynamicInit()
        {
            if (_isDynamicInitialized) return;
            _isDynamicInitialized = true;
            LocalizationSettings.AddListenerOnLocaleChange(OnLocaleChanged);
            if (m_args != null)
            {
                foreach (var arg in m_args)
                {
                    if (arg == null) continue;
                    arg.Subscribe(OnArgChanged);
                }
            }
            UpdateArgs();
        }

        protected void EnsureDynamicClear()
        {
            if (!_isDynamicInitialized) return;
            _isDynamicInitialized = false;
            LocalizationSettings.RemoveListenerOnLocaleChange(OnLocaleChanged);
            if (m_args != null)
            {
                foreach (var arg in m_args)
                {
                    if (arg == null) continue;
                    arg.Unsubscribe(OnArgChanged);
                }
            }
        }
        public virtual void Dispose() => EnsureDynamicClear();
        private void OnLocaleChanged(string locale) => ExecuteRefresh();
        private void OnArgChanged()
        {
            UpdateArgs();
            ExecuteRefresh();
        }
        protected abstract void ExecuteRefresh();

        protected void Bind<T>(ref Action<T> eventField, Action<T> callback, T cachedValue)
        {
            eventField += callback;
            if (_isDynamicInitialized && cachedValue != null) callback?.Invoke(cachedValue);
            HandleSubscribe();
        }
        protected void Unbind<TDelegate>(ref TDelegate eventField, TDelegate callback) where TDelegate : Delegate
        {
            eventField = (TDelegate)Delegate.Remove(eventField, callback);
            HandleUnsubscribe();
        }
        protected void ReleaseCachedAsset<TAsset>(ref TAsset asset)
        {
            if (asset == null) return;
            if (asset is UnityEngine.Object uObj && uObj != null)
                Addressables.Release(uObj);
            else if (asset is IEnumerable<UnityEngine.Object> uObjArray)
                foreach (var obj in uObjArray)
                    if (obj != null) Addressables.Release(obj);
            asset = default;
        }

        public void Validate()
        {
            var table = LocalizationSettings.Database.Schema.Tables.FirstOrDefault(t => t.Id == TableId);
            if (table.Id != TableId)
            {
                throw new Exception($"[LocRef] Table not found: '{TableId ?? "null"}'.");
            }
            var key = table.Keys.FirstOrDefault(k => k.Id == KeyId);
            if (key.Id != KeyId)
            {
                throw new Exception($"[LocRef] Key not found: '{KeyId ?? "null"}' in table '{TableId}'.");
            }
            if (key.IsArray != IsArray)
            {
                throw new Exception($"[LocRef] Array mismatch for '{TableId}/{KeyId}': Reference IsArray is '{IsArray}', but schema expects '{key.IsArray}'.");
            }
            int argsCount = m_args?.Length ?? 0;
            if (argsCount != key.Args.Count)
            {
                throw new Exception($"[LocRef] Argument count mismatch for '{TableId}/{KeyId}': Provided {argsCount} arguments, but schema expects exactly {key.Args.Count}.");
            }
            for (int i = 0; i < argsCount; i++)
            {
                Type expectedType = key.Args[i].ArgType;
                Type providedType = m_args[i].ArgType;
                if (expectedType != typeof(object) && expectedType != providedType)
                {
                    throw new Exception($"[LocRef] Argument type mismatch for '{TableId}/{KeyId}' at index [{i}]: Provided type '{providedType.Name}', but schema expects '{expectedType.Name}'.");
                }
            }
            Type[] providedTypes = Types.Where(t => t != typeof(object)).ToArray();
            HashSet<Type> allowedTypes = key.Types.Select(LocalizationKey.AllowedTypes.GetValueOrDefault).ToHashSet();
            if (providedTypes.Length > allowedTypes.Count)
            {
                throw new Exception($"[LocRef] Type count exceeded for '{TableId}/{KeyId}': Provided {providedTypes.Length} types, but schema supports a maximum of {allowedTypes.Count}.");
            }
            foreach (var type in providedTypes)
            {
                if (!allowedTypes.Contains(type))
                {
                    throw new Exception($"[LocRef] Unsupported type for '{TableId}/{KeyId}': '{type.Name}' is not in the allowed types list.");
                }
            }
        }
    }
    internal struct LocDataWrap<T>
    {
        public T CachedValue;
        public Action<T> OnValueChanged;
    }
    [Serializable]
    public class LocalizedReference<T1> : LocRef
    {
        public override bool IsArray => false;
        public override Type[] Types => new Type[] { typeof(T1) };


        private T1 _cached; private Action<T1> _onChanged;
        public T1 Value => _isDynamicInitialized ? _cached : LocalizationDatabase.GetValue<T1>(TableId, KeyId, GetArgs());


        public LocalizedReference(string tableId, string keyId, params ILocArg[] args) : base(tableId, keyId, args) { }



        public void Subscribe(Action<T1> cb) => Bind(ref _onChanged, cb, _cached);
        public void Unsubscribe(Action<T1> cb) => Unbind(ref _onChanged, cb);

        protected override void ExecuteRefresh()
        {
            var args = GetArgs();
            LocalizationDatabase.GetValueAsync<T1>(TableId, KeyId, args).Then(val => _onChanged?.Invoke(_cached = val));
        }
        public override void Dispose()
        {
            ReleaseCachedAsset(ref _cached);
            _onChanged = null;
            base.Dispose();
        }
    }
    [Serializable]
    public class LocalizedReference<T1, T2> : LocRef
    {
        public override bool IsArray => false;
        public override Type[] Types => new Type[] { typeof(T1), typeof(T2) };

        private T1 _cached1; private Action<T1> _onChanged1;
        private T2 _cached2; private Action<T2> _onChanged2;
        public T1 Value1 => _isDynamicInitialized ? _cached1 : LocalizationDatabase.GetValue<T1>(TableId, KeyId, GetArgs());
        public T2 Value2 => _isDynamicInitialized ? _cached2 : LocalizationDatabase.GetValue<T2>(TableId, KeyId, GetArgs());
        public LocalizedReference(string tableId, string keyId, params ILocArg[] args) : base(tableId, keyId, args) { }

        public void Subscribe(Action<T1> cb) => Bind(ref _onChanged1, cb, _cached1);
        public void Unsubscribe(Action<T1> cb) => Unbind(ref _onChanged1, cb);
        public void Subscribe(Action<T2> cb) => Bind(ref _onChanged2, cb, _cached2);
        public void Unsubscribe(Action<T2> cb) => Unbind(ref _onChanged2, cb);

        protected override void ExecuteRefresh()
        {
            var args = GetArgs();
            LocalizationDatabase.GetValueAsync<T1>(TableId, KeyId, args).Then(val => _onChanged1?.Invoke(_cached1 = val));
            LocalizationDatabase.GetValueAsync<T2>(TableId, KeyId, args).Then(val => _onChanged2?.Invoke(_cached2 = val));
        }
        public override void Dispose()
        {
            ReleaseCachedAsset(ref _cached1);
            ReleaseCachedAsset(ref _cached2);
            _onChanged1 = null;
            _onChanged2 = null;
            base.Dispose();
        }
    }
}