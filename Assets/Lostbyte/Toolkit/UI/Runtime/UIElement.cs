using Lostbyte.Toolkit.Audio;
using Lostbyte.Toolkit.Common;
using Lostbyte.Toolkit.CustomEditor;
using Lostbyte.Toolkit.Tween;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Lostbyte.Toolkit.UI
{
    [System.Flags]
    public enum PlatformConstraints
    {
        None = 0,
        Editor = 1 << 0,
        Web = 1 << 1,
        Mobile = 1 << 2,
        Desktop = 1 << 3,
        Console = 1 << 4,
        All = ~0
    }
    public class UIElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private PlatformConstraints m_disableOnPlatforms = PlatformConstraints.None;

        [SerializeField, EditModeOnly] private bool m_enableHoverAnim = false;
        [SerializeField, EditModeOnly] private AnimationType m_hoverAnimType = AnimationType.EaseInOut; // only if m_enableHoverAnim == true
        [SerializeField, EditModeOnly] private float m_hoverAnimDuration = 0.1f; // only if m_enableHoverAnim == true
        [SerializeField, EditModeOnly] private float m_hoverScale = 1.2f; // only if m_enableHoverAnim == true

        [SerializeField] private SFXClip m_onSelectClip; // only if has TMP_InputField
        [SerializeField] private SFXClip m_onDeselectClip; // only if has TMP_InputField
        [SerializeField] private SFXClip m_onChangeClip; // only if has TMP_InputField || Slider
        [SerializeField] private SFXClip m_onSubmitClip; // only if has TMP_InputField || Button

        private TransformTween _tween;
        private readonly SubscriptionGroup _subscriptions = new();
        private void Awake()
        {
            if (ShouldDisableOnCurrentPlatform())
            {
                gameObject.SetActive(false);
                return;
            }
            if (TryGetComponent<Button>(out var button))
            {
                _subscriptions.Subscribe(button.onClick, PlayOnSubmit);
            }
            if (TryGetComponent<Slider>(out var slider))
            {
                _subscriptions.Subscribe(slider.onValueChanged, PlayOnChange);
            }
            if (TryGetComponent<TMP_InputField>(out var input))
            {
                _subscriptions.Subscribe(input.onSelect, PlayOnSelect);
                _subscriptions.Subscribe(input.onDeselect, PlayOnDeselect);
                _subscriptions.Subscribe(input.onValueChanged, PlayOnChange);
                _subscriptions.Subscribe(input.onEndEdit, PlayOnSubmit);
            }
            if (m_enableHoverAnim)
            {
                _tween = this.Tween(transform).Scale(m_hoverScale).SetAnimation(m_hoverAnimType).SetDuration(m_hoverAnimDuration).SetDeltaType(TimeDeltaType.Unscaled);
            }
        }
        private bool ShouldDisableOnCurrentPlatform()
        {
            if (m_disableOnPlatforms == PlatformConstraints.None) return false;

            // Editor Check
            if (Application.isEditor && m_disableOnPlatforms.HasFlag(PlatformConstraints.Editor)) return true;

            // WebGL Check
            if (Application.platform == RuntimePlatform.WebGLPlayer && m_disableOnPlatforms.HasFlag(PlatformConstraints.Web)) return true;

            // Mobile Check
            if ((Application.platform == RuntimePlatform.Android ||
                 Application.platform == RuntimePlatform.IPhonePlayer) &&
                 m_disableOnPlatforms.HasFlag(PlatformConstraints.Mobile)) return true;

            // Desktop Check
            if ((Application.platform == RuntimePlatform.WindowsPlayer ||
                 Application.platform == RuntimePlatform.OSXPlayer ||
                 Application.platform == RuntimePlatform.LinuxPlayer) &&
                 m_disableOnPlatforms.HasFlag(PlatformConstraints.Desktop)) return true;

            // Console Check
            if ((Application.platform == RuntimePlatform.PS4 ||
                 Application.platform == RuntimePlatform.PS5 ||
                 Application.platform == RuntimePlatform.XboxOne ||
                 Application.platform == RuntimePlatform.Switch) &&
                 m_disableOnPlatforms.HasFlag(PlatformConstraints.Console)) return true;

            return false;
        }

        private void PlayOnSubmit()
        {
            if (m_onSubmitClip) m_onSubmitClip.Play(Vector3.zero);
        }

        private void PlayOnChange()
        {
            if (m_onSubmitClip) m_onChangeClip.Play(Vector3.zero);
        }
        private void PlayOnSelect()
        {
            if (m_onSubmitClip) m_onSelectClip.Play(Vector3.zero);
        }
        private void PlayOnDeselect()
        {
            if (m_onSubmitClip) m_onDeselectClip.Play(Vector3.zero);
        }
        private void OnDestroy()
        {
            _subscriptions.Dispose();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (m_enableHoverAnim) _tween.Play();
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            if (m_enableHoverAnim) _tween.PlayReverse();
        }
    }
}
