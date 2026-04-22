using System.Collections;
using System.Collections.Generic;
using Lostbyte.Toolkit.CustomEditor;
using Lostbyte.Toolkit.FactSystem;
using Lostbyte.Toolkit.Common;
using UnityEngine;
using UnityEngine.UI;

namespace Lostbyte.Toolkit.UI
{
    public class FactSlider : MonoBehaviour
    {
        [SerializeField, Autowired(isForced: true), Hide] private Slider m_slider;
        [SerializeField] private FactWrapper<float> m_fact;

        private readonly SubscriptionGroup _subscriptions = new();
        private void OnEnable()
        {
            _subscriptions.Subscribe(m_fact, SetSliderValue, invokeImidiate: true);
            _subscriptions.Subscribe(m_slider.onValueChanged, OnSliderChange);
        }

        private void OnDisable() => _subscriptions.Dispose();

        private void SetSliderValue(float value)
        {
            if (m_slider.value != value) m_slider.value = value;
        }
        private void OnSliderChange(float value)
        {
            m_fact.Value = value;
        }
    }
}
