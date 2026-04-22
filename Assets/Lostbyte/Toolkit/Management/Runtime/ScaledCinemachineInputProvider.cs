using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Lostbyte.Toolkit.FactSystem;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Lostbyte.Toolkit.Management
{
    public class ScaledCinemachineInputProvider : MonoBehaviour, AxisState.IInputAxisProvider
    {
        [SerializeField] private InputActionReference m_lookAction;
        [SerializeField] private FactWrapper<float> m_sensetivityFact;
        [SerializeField] private FactWrapper<bool> m_gamePausedFact;

        private void OnEnable()
        {
            if (m_lookAction) m_lookAction.action.Enable();
            m_gamePausedFact.Subscribe(OnPaused);
        }

        private void OnDisable()
        {
            if (m_lookAction) m_lookAction.action.Disable();
            m_gamePausedFact.Unsubscribe(OnPaused);

        }

        private void OnPaused(bool isPaused)
        {
            if (TryGetComponent<CinemachineVirtualCamera>(out var cam)) cam.enabled = !isPaused;
        }

        public float GetAxisValue(int axis)
        {
            if (m_lookAction == null) return 0f;

            Vector2 lookInput = m_lookAction.action.ReadValue<Vector2>();

            return axis switch
            {
                // X Axis (Pan)
                0 => lookInput.x * m_sensetivityFact.Value,
                // Y Axis (Tilt)
                1 => lookInput.y * m_sensetivityFact.Value,
                // Z Axis (Usually Zoom/Scroll)
                2 => 0f,
                _ => 0f,
            };
        }
    }
}
