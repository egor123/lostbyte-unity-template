using System.Collections;
using System.Collections.Generic;
using Lostbyte.Toolkit.Common;
using Lostbyte.Toolkit.CustomEditor;
using Lostbyte.Toolkit.FactSystem;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Lostbyte.Toolkit.Management
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField, EditModeOnly] private InputActionReference m_pauseAction;
        [SerializeField] private FactWrapper<bool> m_gamePausedFact;
        [SerializeField] private FactWrapper<bool> m_cursorLockedFact;

        [SerializeField] private GameObject m_menu;

        private readonly SubscriptionGroup _subscriptions = new();

        private void Awake()
        {
            if (m_pauseAction)
            {
                m_pauseAction.action.Enable();
                m_pauseAction.action.performed += OnPausePerformed;
            }
            _subscriptions.Subscribe(m_gamePausedFact, OnGamePaused, invokeImidiate: true);

        }

        private void OnDestroy()
        {
            if (m_pauseAction)
            {
                m_pauseAction.action.performed -= OnPausePerformed;
                m_pauseAction.action.Disable();
            }
            _subscriptions.Dispose();
            m_cursorLockedFact.Value = false;
            m_gamePausedFact.Value = false;
        }

        public void TogglePause() => m_gamePausedFact.Value = !m_gamePausedFact.Value;
        private void OnPausePerformed(InputAction.CallbackContext context) => TogglePause();
        private void OnGamePaused(bool isPaused)
        {
            m_menu.SetActive(isPaused);
            m_cursorLockedFact.Value = !isPaused;
        }
    }
}
