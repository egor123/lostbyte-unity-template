using System;
using System.Collections;
using System.Collections.Generic;
using Lostbyte.Toolkit.Common;
using Lostbyte.Toolkit.CustomEditor;
using Lostbyte.Toolkit.FactSystem;
using UnityEngine;

namespace Lostbyte.Toolkit.Management
{
    public class GameManager : Manager<GameManager>
    {
        [SerializeField, Autowired, Hide] private KeyReference m_key;
        [SerializeField] private EventWrapper m_onGameExit;

        [SerializeField] private Condition m_cursorLockCondition;

        public event Action OnGamePaused;
        public event Action OnGameResumed;

        public bool IsPaused { get; private set; }

        private float m_previousTimeScale = 1f;
        private readonly SubscriptionGroup _subscriptions = new();
        protected override void OnAwake()
        {
            _subscriptions.Subscribe(m_onGameExit, OnExit);
            _subscriptions.Subscribe(m_cursorLockCondition, m_key, OnCursorLock);
        }

        private void OnDestroy()
        {
            _subscriptions.Dispose();
        }

        private void OnCursorLock(bool isLocked)
        {
            Debug.Log("[GameManager] Cursor is " + (isLocked ? "locked" : "unlocked"));
            Cursor.lockState = isLocked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !isLocked;
        }

        public void TogglePause()
        {
            if (IsPaused) ResumeGame();
            else PauseGame();
        }
        public void PauseGame()
        {
            if (IsPaused) return;
            Debug.Log("[GameManager] Game is paused");
            IsPaused = true;
            m_previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            OnGamePaused?.Invoke();
        }

        public void ResumeGame()
        {
            if (!IsPaused) return;
            Debug.Log("[GameManager] Game is resumed");
            IsPaused = false;
            Time.timeScale = m_previousTimeScale;
            OnGameResumed?.Invoke();
        }

        public void QuitGame()
        {
            OnExit();
        }

        private void OnExit()
        {
            Debug.Log("[GameManager] Exiting Game...");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
        }
    }
}