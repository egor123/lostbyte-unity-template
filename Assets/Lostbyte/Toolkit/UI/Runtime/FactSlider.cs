using System.Collections;
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
        [SerializeField] private float m_updateRate = 0.1f;

        private readonly SubscriptionGroup _subscriptions = new();

        private Coroutine _throttleCoroutine;
        private float _targetValue;

        private void OnEnable()
        {
            _subscriptions.Subscribe(m_fact, SetSliderValue, invokeImidiate: true);
            _subscriptions.Subscribe(m_slider.onValueChanged, OnSliderChange);
        }

        private void OnDisable()
        {
            _subscriptions.Dispose();
            if (_throttleCoroutine != null) StopCoroutine(_throttleCoroutine);
        }

        private void SetSliderValue(float value)
        {
            if (!Mathf.Approximately(m_slider.value, value)) m_slider.value = value;
        }

        private void OnSliderChange(float value)
        {
            _targetValue = value;
            _throttleCoroutine ??= StartCoroutine(ThrottleUpdate());
        }

        private IEnumerator ThrottleUpdate()
        {
            while (true)
            {
                float snapshotValue = _targetValue;
                m_fact.Value = snapshotValue;
                yield return new WaitForSecondsRealtime(m_updateRate);
                if (Mathf.Approximately(_targetValue, snapshotValue))
                {
                    _throttleCoroutine = null;
                    yield break;
                }
            }
        }
    }
}