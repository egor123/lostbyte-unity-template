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
    public class UIElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, IPointerUpHandler

    {
        [Header("Selection Settings")]
        [SerializeField, EditModeOnly] private bool m_selectOnHover = true;


        [Header("Tween Settings")]
        [SerializeField, EditModeOnly] private bool m_enableHoverAnim = false;
        [SerializeField, EditModeOnly] private bool m_enableSelectedAnim = false;
        [SerializeField, EditModeOnly, ShowIf(nameof(m_enableHoverAnim), nameof(m_enableSelectedAnim))] private AnimationType m_animType = AnimationType.EaseInOut; // only if m_enableHoverAnim == true
        [SerializeField, EditModeOnly, ShowIf(nameof(m_enableHoverAnim), nameof(m_enableSelectedAnim))] private float m_animDuration = 0.1f; // only if m_enableHoverAnim == true
        [SerializeField, EditModeOnly, ShowIf(nameof(m_enableHoverAnim), nameof(m_enableSelectedAnim))] private float m_animScale = 1.1f; // only if m_enableHoverAnim == true

        [Header("Audio Settings")]
        [SerializeField] private SFXClip m_onSelectClip;
        [SerializeField] private SFXClip m_onClickClip;
        [SerializeField, ShowIf(typeof(Slider), typeof(TMP_Dropdown), typeof(TMP_InputField))] private SFXClip m_onChangeClip;
        [SerializeField, ShowIf(typeof(TMP_InputField))] private SFXClip m_onSubmitClip;

        private bool _isSelected, _isHovered;
        private TransformTween _tween;
        private int _onEnableFrame;
        private readonly SubscriptionGroup _subscriptions = new();

        private void Start()
        {
            if (m_enableHoverAnim || m_enableSelectedAnim)
            {
                _tween = this.Tween(transform).Scale(m_animScale).SetAnimation(m_animType).SetDuration(m_animDuration).SetDeltaType(TimeDeltaType.Unscaled);
                if (EventSystem.current.currentSelectedGameObject == gameObject) OnSelect(default);
            }
            Bind<Slider>(i => _subscriptions.Subscribe(i.onValueChanged, PlayOnChange));
            Bind<TMP_Dropdown>(i => _subscriptions.Subscribe(i.onValueChanged, PlayOnChange));
            Bind<TMP_InputField>(i => _subscriptions.Subscribe(i.onValueChanged, PlayOnChange));
            Bind<TMP_InputField>(i => _subscriptions.Subscribe(i.onSubmit, PlayOnSubmit));
        }
        private void OnEnable()
        {
            _onEnableFrame = Time.frameCount;
        }
        private void Bind<T>(System.Action<T> binder) where T : Component
        {
            if (TryGetComponent<T>(out var component)) binder.Invoke(component);
        }
        private void PlayOnSubmit()
        {
            if (_onEnableFrame + 1 < Time.frameCount && m_onSubmitClip) m_onSubmitClip.Play();
        }
        private void PlayOnClick()
        {
            if (_onEnableFrame + 1 < Time.frameCount && m_onClickClip) m_onClickClip.Play();
        }
        private void PlayOnChange()
        {
            if (_onEnableFrame + 1 < Time.frameCount && m_onChangeClip) m_onChangeClip.Play();
        }
        private void PlayOnSelect()
        {
            if (_onEnableFrame + 1 < Time.frameCount && m_onSelectClip) m_onSelectClip.Play();
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
            PlayOnSelect();
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

        public void OnPointerUp(PointerEventData eventData)
        {
            PlayOnClick();
        }
    }
}
