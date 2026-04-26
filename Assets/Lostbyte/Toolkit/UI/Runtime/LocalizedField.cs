using System.Collections;
using System.Collections.Generic;
using Lostbyte.Toolkit.Common;
using Lostbyte.Toolkit.CustomEditor;
using Lostbyte.Toolkit.FactSystem;
using Lostbyte.Toolkit.Localization;
using TMPro;
using UnityEngine;

namespace Lostbyte.Toolkit.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedField : MonoBehaviour
    {
        [SerializeField] private string m_table;
        [SerializeField] private string m_key;
        [SerializeField] private List<SerializedTuple<KeyContainer, FactDefinition>> m_args;
        private object[] _values;
        private TMP_Text _field;
        private readonly SubscriptionGroup _subscriptions = new();
        private void Awake()
        {
            _field = GetComponent<TMP_Text>();
            _values = new object[m_args.Count];
            _subscriptions.SubscribeLocalizationChange(OnLocaleChange, invokeImidiate: true);
            m_args.ForEach(a => _subscriptions.Subscribe(a.Item1, a.Item2, OnLocaleChange));
        }

        private void OnDestroy()
        {
            _subscriptions.Dispose();
        }

        private void OnLocaleChange()
        {
            for (int i = 0; i < m_args.Count; i++)
            {
                var arg = m_args[i];
                if (arg.Item1 == null || arg.Item2 == null) _values[i] = null;
                else _values[i] = arg.Item1.GetWrapper(arg.Item2).RawValue.ToString();
            }
            _field.text = LocalizationSettings.Database.GetTable(m_table).GetString(m_key, _values);
        }
    }
}
