using System;
using System.Collections.Generic;
using Lostbyte.Toolkit.Common;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Lostbyte.Toolkit.Localization
{
    public abstract class LocRef : IDisposable
    {
        [field: SerializeField] public string TableId { get; protected set; }
        [field: SerializeField] public string KeyId { get; protected set; }
        [SerializeField, SerializeReference] protected ILocArg[] m_args;
        private object[] _cachedArgs;
        protected bool _isDynamicInitialized;
        protected int _subscriberCount;

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
    }

    internal struct LocDataWrap<T>
    {
        public T CachedValue;
        public Action<T> OnValueChanged;
    }
    [Serializable]
    public class LocalizedReference<T1> : LocRef
    {
        private T1 _cached1; private Action<T1> _onChanged1;
        public T1 Value1 => _isDynamicInitialized ? _cached1 : LocalizationDatabase.GetValue<T1>(TableId, KeyId, GetArgs());
        public LocalizedReference(string tableId, string keyId, params ILocArg[] args) : base(tableId, keyId, args) { }

        public T1 Value => _isDynamicInitialized ? _cached1 : LocalizationDatabase.GetValue<T1>(TableId, KeyId, GetArgs());


        public void Subscribe(Action<T1> cb) => Bind(ref _onChanged1, cb, _cached1);
        public void Unsubscribe(Action<T1> cb) => Unbind(ref _onChanged1, cb);

        protected override void ExecuteRefresh()
        {
            var args = GetArgs();
            LocalizationDatabase.GetValueAsync<T1>(TableId, KeyId, args).Then(val => _onChanged1?.Invoke(_cached1 = val));
        }
        public override void Dispose()
        {
            ReleaseCachedAsset(ref _cached1);
            _onChanged1 = null;
            base.Dispose();
        }
    }
    [Serializable]
    public class LocalizedReference<T1, T2> : LocRef
    {
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
