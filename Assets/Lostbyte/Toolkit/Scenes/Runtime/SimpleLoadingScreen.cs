using System.Threading;
using System.Threading.Tasks;
using Lostbyte.Toolkit.CustomEditor;
using UnityEngine;

namespace Lostbyte.Toolkit.Scenes
{
    public class SimpleLoadingScreen : LoadingScreenBase
    {
        [SerializeField, Autowired] private CanvasGroup m_transitionGroup;
        [SerializeField] private float m_fadeDuration = 0.5f;

        private bool _skip = false;
        private bool _fadeIn = false;
        private bool _inTransition = false;
        private CancellationTokenSource _fadeCts;

        public override bool InTransition => _inTransition;
        public float Progress => m_transitionGroup.alpha;

        public override Task FadeIn() => StartFade(true);
        public override Task FadeOut() => StartFade(false);
        public override void Skip() => _skip = true;

        private void Awake() => ApplyAlpha(0f);

        private void OnDestroy() => StopCurrentFade();

        private async Task StartFade(bool fadeIn)
        {
            if (!_inTransition && _fadeIn == fadeIn) return;
            StopCurrentFade();

            _fadeCts = new CancellationTokenSource();
            await FadeRoutine(fadeIn, _fadeCts.Token);
        }

        private async Task FadeRoutine(bool fadeIn, CancellationToken token)
        {
            _fadeIn = fadeIn;
            _inTransition = true;
            _skip = false;

            float startAlpha = m_transitionGroup.alpha;
            float targetAlpha = _fadeIn ? 1f : 0f;
            float time = 0f;

            float distance = Mathf.Abs(targetAlpha - startAlpha);
            float actualDuration = m_fadeDuration * distance;

            if (actualDuration <= 0.001f || _skip)
            {
                FinishFade(targetAlpha);
                return;
            }

            ApplyAlpha(startAlpha);

            try
            {
                while (time < actualDuration && !_skip)
                {
                    if (token.IsCancellationRequested || m_transitionGroup == null) return;

                    time += Time.unscaledDeltaTime;
                    var progress = Mathf.Lerp(startAlpha, targetAlpha, time / actualDuration);
                    ApplyAlpha(progress);

                    await Task.Yield();
                }

                if (!token.IsCancellationRequested && m_transitionGroup != null)
                {
                    FinishFade(targetAlpha);
                }
            }
            catch (TaskCanceledException) { }
        }

        private void FinishFade(float targetAlpha)
        {
            ApplyAlpha(targetAlpha);
            _skip = false;
            _inTransition = false;

            _fadeCts?.Dispose();
            _fadeCts = null;
        }

        public override void SetFadeIn(float progress)
        {
            StopCurrentFade();

            _fadeIn = true;
            _inTransition = progress < 1f;
            ApplyAlpha(progress);
        }

        public override void SetFadeOut(float progress)
        {
            StopCurrentFade();

            _fadeIn = false;
            _inTransition = progress < 1f;
            ApplyAlpha(1f - progress);
        }

        private void StopCurrentFade()
        {
            if (_fadeCts != null)
            {
                _fadeCts.Cancel();
                _fadeCts.Dispose();
                _fadeCts = null;
            }
            _inTransition = false;
        }

        private void ApplyAlpha(float alpha)
        {
            if (m_transitionGroup == null) return;

            alpha = Mathf.Clamp01(alpha);
            m_transitionGroup.alpha = alpha;
            m_transitionGroup.blocksRaycasts = alpha > 0f;
        }
    }
}