using System.Collections;
using System.Collections.Generic;
using Lostbyte.Toolkit.CustomEditor;
using Lostbyte.Toolkit.FactSystem;
using UnityEngine;
using UnityEngine.UI;

namespace Lostbyte.Toolkit.UI
{
    public class EventButton : MonoBehaviour
    {
        [SerializeField, Autowired(isForced: true), Hide] private Button m_button;
        [SerializeField] private KeyContainer m_key;
        [SerializeField] private EventDefinition m_event;

        private void OnEnable() => m_button.onClick.AddListener(Raise);
        private void OnDisable() => m_button.onClick.RemoveListener(Raise);
        private void Raise() => m_key.Raise(m_event);
    }
}
