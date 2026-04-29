using System;
using Lostbyte.Toolkit.Common;
using Lostbyte.Toolkit.CustomEditor;
using Lostbyte.Toolkit.FactSystem;
using Lostbyte.Toolkit.Localization;
using TMPro;
using UnityEngine;

namespace Lostbyte.Toolkit.UI
{
    [ExecuteAlways]
    [RequireComponent(typeof(TMP_Dropdown))]

    public class EnumDropdown : MonoBehaviour
    {
        [SerializeField, Autowired(isForced: true), Hide] private TMP_Dropdown m_dropdown;
        [SerializeField, EditModeOnly] private KeyContainer m_key;
        [SerializeField, EditModeOnly] private EnumFactDefinition m_fact;
        [SerializeField, EditModeOnly] private bool m_localized;
        [SerializeField, EditModeOnly, ShowIf(nameof(m_localized))] private string m_table;

        private readonly SubscriptionGroup _subscriptions = new();
        private IFactWrapper<Enum> _wrapper;

        private void OnEnable()
        {
            if (Application.isPlaying)
            {
                _wrapper = m_key.GetWrapper(m_fact);
                if (m_localized)
                {
                    _subscriptions.SubscribeLocalizationChange((_) =>
                    {
                        m_dropdown.options.Clear();
                        Array enumValues = m_fact.EnumType.GetEnumValues();
                        for (int i = 0; i < enumValues.Length; i++)
                        {
                            object enumValue = enumValues.GetValue(i);
                            string value = LocalizationSettings.Database.GetTable(m_table).GetString(enumValue.ToString());
                            m_dropdown.options.Add(new(value));
                        }
                        m_dropdown.RefreshShownValue();
                    }, invokeImidiate: true);
                }
                else
                {
                    m_dropdown.options.Clear();
                    foreach (var enumValue in m_fact.EnumType.GetEnumValues())
                    {
                        m_dropdown.options.Add(new(enumValue.ToString()));
                    }
                }
                _subscriptions.Subscribe(_wrapper, OnFactChange, invokeImidiate: true);
                _subscriptions.Subscribe(m_dropdown.onValueChanged, OnInputChange);
            }
        }
#if UNITY_EDITOR
        private void Update()
        {
            if (!Application.isPlaying && m_key && m_fact)
            {
                m_dropdown.options.Clear();
                if (m_localized) m_dropdown.options.Add(new($"{m_table}/{m_key.name}[{m_fact.name}]"));
                else m_dropdown.options.Add(new($"{m_key.name}[{m_fact.name}]"));
                m_dropdown.RefreshShownValue();
            }
        }
#endif
        private void OnDisable()
        {
            if (Application.isPlaying)
            {
                _subscriptions.Dispose();
            }
        }
        private void OnFactChange(Enum value)
        {
            int idx = Convert.ToInt32(value);
            if (!m_dropdown.value.Equals(idx))
            {
                m_dropdown.value = idx;
            }
        }
        private void OnInputChange(int value)
        {
            if (_wrapper == null || m_fact.EnumType == null) return;
            _wrapper.RawValue = Enum.ToObject(m_fact.EnumType, value);
        }

    }
}
