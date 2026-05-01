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
        [SerializeField] private FactWrapper<Enum> m_fact;
        [SerializeField, EditModeOnly] private bool m_localized;
        [SerializeField, EditModeOnly, ShowIf(nameof(m_localized))] private string m_table;

        private readonly SubscriptionGroup _subscriptions = new();

        private void OnEnable()
        {
            if (Application.isPlaying)
            {
                Type enumType = (m_fact.Fact as EnumFactDefinition).EnumType;
                if (m_localized)
                {
                    _subscriptions.SubscribeLocalizationChange((_) =>
                    {
                        m_dropdown.options.Clear();
                        Array enumValues = enumType.GetEnumValues();
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
                    foreach (var enumValue in enumType.GetEnumValues())
                    {
                        m_dropdown.options.Add(new(enumValue.ToString()));
                    }
                }
                _subscriptions.Subscribe(m_fact, OnFactChange, invokeImidiate: true);
                _subscriptions.Subscribe(m_dropdown.onValueChanged, OnInputChange);
            }
        }
#if UNITY_EDITOR
        private void Update()
        {
            if (!Application.isPlaying && m_fact.Key && m_fact.Fact)
            {
                m_dropdown.options.Clear();
                if (m_localized) m_dropdown.options.Add(new($"{m_table}/{m_fact.Key.name}[{m_fact.Fact.name}]"));
                else m_dropdown.options.Add(new($"{m_fact.Key.name}[{m_fact.Fact.name}]"));
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
            if (m_fact.Key == null || m_fact.Fact == null) return;
            Type enumType = (m_fact.Fact as EnumFactDefinition).EnumType;
            m_fact.RawValue = Enum.ToObject(enumType, value);
        }

    }
}
