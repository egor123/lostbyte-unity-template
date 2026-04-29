using System;
using Lostbyte.Toolkit.Common;
using Lostbyte.Toolkit.FactSystem;
using UnityEngine;

namespace Lostbyte.Toolkit.Localization
{
    [Serializable]
    public class LocalizedString : IDisposable, ISerializationCallbackReceiver
    {
        [SerializeField] private string m_table;
        [SerializeField] private string m_key;
        [SerializeField] private SerializedTuple<KeyContainer, FactDefinition>[] m_facts;

        private IFactWrapper[] _facts;
        private object[] _values;
        private int _factCount;

        private event Action<string> OnChange;
        private string _value;
        private bool _isSubscribedToLocale;
        private bool _requiresRuntimeInitialization = false;
        public Enum Locale { get; private set; } = null;

        public string Value
        {
            get
            {
                RuntimeInitialization();
                if (!ValueIsValid) OnLocaleChange(LocalizationSettings.Locale);
                return _value;
            }
            private set => _value = value;
        }

        private bool ValueIsValid => _isSubscribedToLocale || (_value != null && LocalizationSettings.Locale.Equals(Locale));
        private bool NeedsLocaleSubscription => OnChange != null || _factCount > 0;

        public LocalizedString(string key, string table, params object[] args)
        {
            m_key = key;
            m_table = table;
            SetArgs(args);
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            if (m_facts != null && m_facts.Length > 0)
            {
                _requiresRuntimeInitialization = true;
            }
        }

        private void RuntimeInitialization()
        {
            if (!_requiresRuntimeInitialization) return;
            if (!Application.isPlaying) return;
            _requiresRuntimeInitialization = false;
            _values = new object[m_facts.Length];
            _facts = new IFactWrapper[m_facts.Length];
            for (int i = 0; i < m_facts.Length; i++)
            {
                var fact = m_facts[i];
                if (fact != null && fact.Item1 && fact.Item2)
                {
                    SetArg(i, fact.Item1.GetWrapper(fact.Item2));
                }
            }
        }
        public void SetArgs(params object[] args)
        {
            if (args == null || args.Length == 0)
            {
                _values = Array.Empty<object>();
                _facts = Array.Empty<IFactWrapper>();
                return;
            }

            _values = new object[args.Length];
            _facts = new IFactWrapper[args.Length];

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] is IFactWrapper fact) SetArg(i, fact);
                else SetArg(i, args[i]);
            }
        }

        public void SetArg(int idx, object arg)
        {
            ClearFactAt(idx);
            _values[idx] = arg;
            UpdateValue();
        }

        public void SetArg(int idx, IFactWrapper fact)
        {
            ClearFactAt(idx);

            _facts[idx] = fact;
            _values[idx] = fact; // Ensure the format array has a reference to the fact

            if (fact != null)
            {
                _factCount++;
                fact.Subscribe(UpdateValue);
            }

            UpdateLocaleSubscription();
            UpdateValue();
        }

        private void ClearFactAt(int idx)
        {
            if (_facts != null && idx < _facts.Length && _facts[idx] != null)
            {
                _facts[idx].Unsubscribe(UpdateValue);
                _facts[idx] = null;
                _factCount--;
            }
        }

        private void UpdateLocaleSubscription()
        {
            bool needsSub = NeedsLocaleSubscription;

            if (needsSub && !_isSubscribedToLocale)
            {
                LocalizationSettings.AddListenerOnLocaleChange(OnLocaleChange);
                _isSubscribedToLocale = true;
                UpdateValue();
            }
            else if (!needsSub && _isSubscribedToLocale)
            {
                LocalizationSettings.RemoveListenerOnLocaleChange(OnLocaleChange);
                _isSubscribedToLocale = false;
            }
        }

        private void OnLocaleChange(Enum locale)
        {
            Locale = locale;
            UpdateValue();
        }

        private void UpdateValue()
        {
            if (_facts != null)
            {
                for (int i = 0; i < _facts.Length; i++)
                {
                    IFactWrapper fact = _facts[i];
                    if (fact != null) _values[i] = fact.RawValue;
                }
            }
            _value = LocalizationSettings.Database.GetTable(m_table).GetString(m_key, _values);
            OnChange?.Invoke(_value);
        }

        public void Subscribe(Action<string> callback)
        {
            RuntimeInitialization();
            OnChange += callback;
            UpdateLocaleSubscription();
            callback?.Invoke(Value);
        }

        public void Unsubscribe(Action<string> callback)
        {
            OnChange -= callback;
            UpdateLocaleSubscription();
        }

        public void Dispose()
        {
            if (_facts != null)
            {
                for (int i = 0; i < _facts.Length; i++)
                {
                    ClearFactAt(i);
                }
            }
            OnChange = null;
            UpdateLocaleSubscription();
            GC.SuppressFinalize(this);
        }

        public override string ToString() => Value;
    }
}
