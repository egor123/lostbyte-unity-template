using System;
using System.Collections;
using System.Collections.Generic;
using Lostbyte.Toolkit.Common;
using Lostbyte.Toolkit.CustomEditor;
using Lostbyte.Toolkit.FactSystem;
using Lostbyte.Toolkit.Management;
using TMPro;
using UnityEngine;

namespace Lostbyte.Toolkit.Localization
{
    [RequireComponent(typeof(TMP_Dropdown))]
    public class LocaleDropdown : MonoBehaviour
    {
        [SerializeField, Autowired(isForced: true), Hide] private TMP_Dropdown m_dropdown;
        [SerializeField] private FactWrapper<string> m_fact;
        [SerializeField] private string m_table;
        private readonly SubscriptionGroup _subscriptions = new();

        private void OnEnable()
        {
            if (Application.isPlaying) Init();
        }
        private void Init()
        {
            _subscriptions.SubscribeLocalizationChange((_) =>
            {
                m_dropdown.options.Clear();
                var locales = LocalizationSettings.Locales;
                for (int i = 0; i < locales.Length; i++)
                {
                    string locale = LocalizationDatabase.GetValue<string>(m_table, locales[i]);
                    m_dropdown.options.Add(new(locale));
                }
                m_dropdown.RefreshShownValue();
            }, invokeImidiate: true);
            _subscriptions.Subscribe(m_fact, OnFactChange, invokeImidiate: true);
            _subscriptions.Subscribe(m_dropdown.onValueChanged, OnInputChange);

        }
#if UNITY_EDITOR
        private void Update()
        {
            if (!Application.isPlaying && m_fact.Key && m_fact.Fact)
            {
                m_dropdown.options.Clear();
                m_dropdown.options.Add(new($"{m_table}/{m_fact.Key.name}[{m_fact.Fact.name}]"));
                m_dropdown.RefreshShownValue();
            }
        }
#endif
        private void OnDisable()
        {
            _subscriptions.Dispose();
        }
        private void OnFactChange(string locale)
        {
            int idx = LocalizationSettings.Locales.IndexOf(locale);
            if (idx < 0) idx = 0;
            if (!m_dropdown.value.Equals(idx)) m_dropdown.value = idx;
        }
        private void OnInputChange(int idx)
        {
            m_fact.Value = LocalizationSettings.Locales[idx];
        }
    }
}
