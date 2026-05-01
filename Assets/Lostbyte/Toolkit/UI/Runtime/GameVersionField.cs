using System;
using System.Collections;
using System.Collections.Generic;
using Lostbyte.Toolkit.CustomEditor;
using TMPro;
using UnityEngine;

namespace Lostbyte.Toolkit.UI
{
    [ExecuteAlways]
    [RequireComponent(typeof(TMP_Text))]
    public class GameVersionField : MonoBehaviour
    {
        [SerializeField, Autowired(isForced: true), Hide] private TMP_Text m_text;

        private void Start()
        {
            if (m_text != null) m_text.text = $"v{Application.version}";
        }
#if UNITY_EDITOR
        private void Update()
        {
            if (m_text != null) m_text.text = $"v{Application.version}";
        }
#endif
    }
}
