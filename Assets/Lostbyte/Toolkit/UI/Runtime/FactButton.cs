using System.Collections;
using System.Collections.Generic;
using Lostbyte.Toolkit.CustomEditor;
using Lostbyte.Toolkit.FactSystem;
using UnityEngine;
using UnityEngine.UI;

namespace Lostbyte.Toolkit.UI
{
    [RequireComponent(typeof(Button))]
    public class FactButton : MonoBehaviour
    {
        [SerializeField, Autowired(isForced: true), Hide] private Button m_button;
        [SerializeField] private KeyContainer m_key;
        [SerializeField] private FactDefinition m_fact;
        [SerializeField, SerializeReference] private IValueHolder m_value;

        private void OnEnable() => m_button.onClick.AddListener(Raise);
        private void OnDisable() => m_button.onClick.RemoveListener(Raise);
        private void Raise() => m_key.GetWrapper(m_fact).RawValue = m_value.RawValue;
    }
}
