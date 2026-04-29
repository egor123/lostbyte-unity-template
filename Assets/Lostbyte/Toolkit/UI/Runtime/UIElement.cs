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
    public class UIElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler

    {
        [SerializeField, EditModeOnly] private bool m_selectOnHover = true;

        [SerializeField, EditModeOnly] private bool m_enableHoverAnim = false;
        [SerializeField, EditModeOnly] private bool m_enableSelectedAnim = false; // TODO

        [SerializeField, EditModeOnly] private AnimationType m_animType = AnimationType.EaseInOut; // only if m_enableHoverAnim == true
        [SerializeField, EditModeOnly] private float m_animDuration = 0.1f; // only if m_enableHoverAnim == true
        [SerializeField, EditModeOnly] private float m_animScale = 1.1f; // only if m_enableHoverAnim == true

        [SerializeField] private SFXClip m_onSelectClip; // only if has TMP_InputField
        [SerializeField] private SFXClip m_onDeselectClip; // only if has TMP_InputField
        [SerializeField] private SFXClip m_onChangeClip; // only if has TMP_InputField || Slider
        [SerializeField] private SFXClip m_onSubmitClip; // only if has TMP_InputField || Button

        private bool _isSelected, _isHovered;
        private TransformTween _tween;
        private readonly SubscriptionGroup _subscriptions = new();

        private void Awake()
        {
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
            if (m_enableHoverAnim || m_enableSelectedAnim)
            {
                _tween = this.Tween(transform).Scale(m_animScale).SetAnimation(m_animType).SetDuration(m_animDuration).SetDeltaType(TimeDeltaType.Unscaled);
                if (EventSystem.current.currentSelectedGameObject == gameObject) OnSelect(default);
            }
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
            if (m_selectOnHover) EventSystem.current.SetSelectedGameObject(gameObject);
            _isSelected = true;
            TryPlayTween();
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            _isSelected = false;
            TryPlayTweenReversed();
        }

        public void OnSelect(BaseEventData eventData)
        {
            _isHovered = true;
            TryPlayTween();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            _isHovered = false;
            TryPlayTweenReversed();
        }
        private void TryPlayTween()
        {
            if ((m_enableHoverAnim && _isHovered) || (m_enableSelectedAnim && _isSelected)) _tween.Play();
        }
        private void TryPlayTweenReversed()
        {
            if (m_enableHoverAnim && m_enableSelectedAnim && !_isHovered && !_isSelected) _tween.PlayReverse();
            else if (m_enableHoverAnim && !m_enableSelectedAnim && !_isHovered) _tween.PlayReverse();
            else if (!m_enableHoverAnim && m_enableSelectedAnim && !_isSelected) _tween.PlayReverse();
        }
    }
}
