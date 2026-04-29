using System;
using Lostbyte.Toolkit.Common;
using Lostbyte.Toolkit.CustomEditor;
using Lostbyte.Toolkit.FactSystem;
using TMPro;
using UnityEngine;

namespace Lostbyte.Toolkit.UI
{
    [ExecuteAlways]
    [RequireComponent(typeof(TMP_InputField))]
    public class FactInputField : MonoBehaviour
    {
        [SerializeField, Autowired(isForced: true), Hide] private TMP_InputField m_input;
        [SerializeField, EditModeOnly] private KeyContainer m_key;
        [SerializeField, EditModeOnly] private FactDefinition m_fact;

        private readonly SubscriptionGroup _subscriptions = new();
        private IFactWrapper _wrapper;
        private void OnEnable()
        {
            if (Application.isPlaying)
            {
                _wrapper = m_key.GetWrapper(m_fact);
                _subscriptions.Subscribe(_wrapper, OnFactChange, invokeImidiate: true);
                _subscriptions.Subscribe(m_input.onValueChanged, OnInputChange);
            }
        }
#if UNITY_EDITOR
        private void Update()
        {
            if (!Application.isPlaying && m_key && m_fact)
                m_input.text = $"{m_key.name}[{m_fact.name}]";
        }
#endif
        private void OnDisable()
        {
            if (Application.isPlaying)
            {
                _subscriptions.Dispose();
            }
        }
        private void OnFactChange(object value)
        {
            string text = value?.ToString() ?? "";
            if (!m_input.text.Equals(text))
                m_input.text = text;
        }

        private void OnInputChange(string value)
        {
            if (_wrapper == null || _wrapper.RawValue == null) return;
            Type targetType = _wrapper.RawValue.GetType();
            object raw = null;
            try
            {
                if (targetType.IsEnum) raw = Enum.Parse(targetType, value, true);
                else raw = Convert.ChangeType(value, targetType);
            }
            catch
            {
                Debug.LogWarning("Cannot convert fact input value!");
            }
            _wrapper.RawValue = raw;
        }

    }
}
