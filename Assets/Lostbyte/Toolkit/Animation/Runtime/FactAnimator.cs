using Lostbyte.Toolkit.CustomEditor;
using Lostbyte.Toolkit.FactSystem;
using UnityEngine;

namespace Lostbyte.Toolkit.Animation
{
    [RequireComponent(typeof(Animator))]
    public class FactAnimator : MonoBehaviour
    {
        [SerializeField, Autowired(Autowired.Type.Parent, isForced: true), Hide] private KeyReference m_key;
        [SerializeField, Autowired(Autowired.Type.Self, isForced: true)] private Animator m_animator;
        [SerializeField] private FactAnimatorSettings m_settings;
        private FactAnimatorRunner _runner;
        private FactAnimatorRunner Runner => _runner ??= m_settings.GetInstance(m_animator, m_key.Key);
        private void OnEnable() => Runner.Enable();
        private void OnDisable() => Runner.Disable();
        private void Update() => Runner.Update();
    }
}
