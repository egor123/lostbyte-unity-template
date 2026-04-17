using System;
using UnityEngine;

namespace Lostbyte.Toolkit.Localization
{
    [Serializable]
    public struct LocalizedString
    {
        [SerializeField] private string m_key;
        [SerializeField] private object[] m_args;
        private Action<string> onChange;
        private string _value;
        private string _locale;
        public string Value
        {
            get
            {
                if (_value == null || _locale != LocalizationSettings.Locale)
                {
                    _locale = LocalizationSettings.Locale;
                    if (m_args != null && m_args.Length > 0) _value = LocalizationSettings.Database.CurrentTable.GetString(m_key, m_args);
                    _value = LocalizationSettings.Database.CurrentTable.GetString(m_key);
                }
                return _value;
            }
        }
        public LocalizedString(string key, params object[] args)
        {
            m_key = key;
            m_args = args;
            _value = null;
            _locale = null;
            onChange = null;
        }
        public LocalizedString(string id)
        {
            m_key = id;
            m_args = null;
            _value = null;
            _locale = null;
            onChange = null;
        }
        public void AddListener(Action<string> callback)
        {
            if (onChange?.GetInvocationList().Length == 0)
                LocalizationSettings.AddListenerOnLocaleChange(OnLocaleChange);
            onChange += callback;
            callback?.Invoke(Value);
        }
        public void RemoveListener(Action<string> callback)
        {
            onChange -= callback;
            if (onChange?.GetInvocationList().Length == 0)
                LocalizationSettings.RemoveListenerOnLocaleChange(OnLocaleChange);

        }
        private void OnLocaleChange(string locale) => onChange?.Invoke(Value);
        public override string ToString() => Value;
    }
}
