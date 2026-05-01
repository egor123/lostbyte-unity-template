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
        [SerializeField] private FactWrapper<bool> m_gamePausedFact;
        [SerializeField] private FactWrapper<bool> m_cursorLockedFact;

        private float _previousTimeScale = 1f;
        private readonly SubscriptionGroup _subscriptions = new();

        protected override void OnAwake()
        {
            _subscriptions.Subscribe(m_onGameExit, OnExit);
            _subscriptions.Subscribe(m_gamePausedFact, OnGamePaused, invokeImidiate: true);
            _subscriptions.Subscribe(m_cursorLockedFact, OnCursorLock, invokeImidiate: true);
        }

        private void OnDestroy()
        {
            _subscriptions.Dispose();
        }

        private void OnCursorLock(bool isLocked)
        {
            var targetLockState = isLocked ? CursorLockMode.Locked : CursorLockMode.None;
            var targetVisibility = !isLocked;
            if (targetLockState == Cursor.lockState && Cursor.visible == targetVisibility) return;
            DebugLogger.ManagerLog("Cursor is " + (isLocked ? "locked" : "unlocked"));
            Cursor.lockState = targetLockState;
            Cursor.visible = targetVisibility;
        }

        public void OnGamePaused(bool isPaused)
        {
            var targetTimeScale = isPaused ? 0f : _previousTimeScale;
            var targetPrevioutTimeScale = isPaused ? Time.timeScale : _previousTimeScale;
            if (targetTimeScale == Time.timeScale && _previousTimeScale == targetPrevioutTimeScale) return;
            DebugLogger.ManagerLog("Game is " + (isPaused ? "paused" : "resumed"));
            Time.timeScale = targetTimeScale;
            _previousTimeScale = targetPrevioutTimeScale;
        }

        private void OnExit()
        {
            DebugLogger.ManagerLog("Exiting Game...");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
        }
    }
}