using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lostbyte.Toolkit.CustomEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem;

namespace Lostbyte.Toolkit.UI
{
    public class UIMenuTab : MonoBehaviour
    {
        [SerializeField] private GameObject m_selectFirst;

        [SerializeField] private bool m_overrideNavigation;
        [SerializeField, ShowIf(nameof(m_overrideNavigation))] private bool m_loop = true;

        private void OnEnable()
        {
            StartCoroutine(InitUI());
        }
        private IEnumerator InitUI()
        {
            TryConfigureNavigation();
            yield return TryForceSelection();
            if (enabled)
            {
                Subscribe();
            }
        }

        private void OnDisable()
        {
            Unsubscribe();
        }
        private void TryConfigureNavigation()
        {
            if (!m_overrideNavigation) return;
            var selectables = GetComponentsInChildren<Selectable>()
                .Where(s => s.enabled && s.IsInteractable())
                .OrderByDescending(s => ((RectTransform)s.transform).anchoredPosition.y)
                .ToList();

            for (int i = 0; i < selectables.Count; i++)
            {
                Selectable next = i > 0 ? selectables[i - 1] : (m_loop ? selectables[^1] : null);
                Selectable prev = i < selectables.Count - 1 ? selectables[i + 1] : (m_loop ? selectables[0] : null);
                Navigation nav = new()
                {
                    mode = Navigation.Mode.Explicit,
                    selectOnUp = next,
                    selectOnDown = prev,
                    selectOnLeft = null,
                    selectOnRight = null
                };
                selectables[i].navigation = nav;
            }
        }
        private void Subscribe()
        {
            var module = EventSystem.current?.currentInputModule as InputSystemUIInputModule;
            if (module == null) return;
            if (module.move.action != null) module.move.action.performed += OnNavigate;
            if (module.submit.action != null) module.submit.action.performed += OnNavigate;
        }
        private void Unsubscribe()
        {
            var module = EventSystem.current?.currentInputModule as InputSystemUIInputModule;
            if (module == null) return;
            if (module.move.action != null) module.move.action.performed -= OnNavigate;
            if (module.submit.action != null) module.submit.action.performed -= OnNavigate;
        }
        private void OnNavigate(InputAction.CallbackContext ctx)
        {
            if (ctx.phase != InputActionPhase.Performed) return;
            if (EventSystem.current.currentSelectedGameObject != null) return;
            StartCoroutine(TryForceSelection());
        }
        private IEnumerator TryForceSelection()
        {
            yield return null;
            if (m_selectFirst != null && enabled)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(m_selectFirst);
            }
        }

    }
}
