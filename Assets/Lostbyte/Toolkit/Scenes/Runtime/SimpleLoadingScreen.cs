using System.Collections;
using System.Collections.Generic;
using Lostbyte.Toolkit.CustomEditor;
using UnityEngine;

namespace Lostbyte.Toolkit.Scenes
{
    public class SimpleLoadingScreen : LoadingScreenBase
    {
        [SerializeField, Autowired] private CanvasGroup m_transitionGroup;
        [SerializeField] private float m_fadeDuration = 0.5f;

        private bool _skip = false;
        public override void FadeIn() => StartCoroutine(FadeRoutine(true));
        public override void Skip() => _skip = true;
        public override void FadeOut() => StartCoroutine(FadeRoutine(false));

        private void Awake()
        {
            m_transitionGroup.alpha = 0f;
            m_transitionGroup.blocksRaycasts = false;
        }
        private IEnumerator FadeRoutine(bool fadeIn)
        {
            InTransition = true;
            float startAlpha = m_transitionGroup.alpha;
            float time = 0f;
            float targetAlpha = fadeIn ? 1f : 0f;
            while (time < m_fadeDuration && !_skip)
            {
                time += Time.deltaTime;
                m_transitionGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / m_fadeDuration);
                yield return null;
            }
            m_transitionGroup.alpha = targetAlpha;
            if (!fadeIn) m_transitionGroup.blocksRaycasts = false;
            InTransition = false;
        }

    }
}