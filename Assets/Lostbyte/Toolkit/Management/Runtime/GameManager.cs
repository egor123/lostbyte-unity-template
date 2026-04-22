using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
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
            Debug.Log("[GameManager] Cursor is " + (isLocked ? "locked" : "unlocked"));
            Cursor.lockState = isLocked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !isLocked;
        }

        public void OnGamePaused(bool isPaused)
        {
            Debug.Log("[GameManager] Game is " + (isPaused ? "paused" : "resumed"));
            if (isPaused) _previousTimeScale = Time.timeScale;
            Time.timeScale = isPaused ? 0f : _previousTimeScale;
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