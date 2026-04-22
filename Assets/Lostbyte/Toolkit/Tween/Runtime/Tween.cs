using System.Collections;
using UnityEngine;
using System;

namespace Lostbyte.Toolkit.Tween
{
    public abstract class Tween : ITweenGroupElement
    {
        protected Tween(MonoBehaviour initiator, float duration) { Initiator = initiator; Duration = duration; }
        protected internal AnimationCurve AnimationCurve { private get; set; }
        protected internal float? Duration { private get; set; }
        protected internal Action Callback { private get; set; }
        private MonoBehaviour Initiator { get; set; }
        private Coroutine _runningCorutine;
        public bool IsRunning { get; internal set; }

        protected internal WrapMode Loop { get; set; } = WrapMode.Clamp;
        protected internal int LoopCount { get; set; } = 1;
        protected internal bool Forward { get; set; } = true;
        bool ITweenGroupElement.IsRunning { get => IsRunning; set => IsRunning = value; }
        bool ITweenGroupElement.Forward { get => Forward; set => Forward = value; }

        protected float _progress = 0f;
        public void Play()
        {
            Forward = true;
            if (Initiator == null && !Initiator.enabled || IsRunning) return;
            IsRunning = true;
            _runningCorutine = Initiator.StartCoroutine(Enumerator());
        }
        public void PlayReverse()
        {
            Forward = false;
            if (Initiator == null && !Initiator.enabled || IsRunning) return;
            IsRunning = true;
            _runningCorutine = Initiator.StartCoroutine(Enumerator());
        }

        public void Pause()
        {
            IsRunning = false;
            if (_runningCorutine != null) Initiator.StopCoroutine(_runningCorutine);
        }
        public void Stop()
        {
            IsRunning = false;
            _progress = 0f;
            if (_runningCorutine != null) Initiator.StopCoroutine(_runningCorutine);
        }
        public void Reset()
        {
            if (_progress != 0)
            {
                Pause();
                _progress = 0f;
                DoTween(0f);
            }
        }
        public void Finish()
        {
            Pause();
            _progress = Forward ? Duration ?? 1 * MathF.Abs(LoopCount) : 0;
            DoTween(1f);
            OnFinish();
        }
        protected virtual IEnumerator Enumerator()
        {
            if (_progress == 0)
                Init();
            while (IsRunning)
            {
                Update();
                yield return null;
            }
            OnFinish();
        }
        internal void OnFinish()
        {
            Callback?.Invoke();
        }
        internal void Update()
        {
            Duration ??= 1;
            AnimationCurve ??= AnimationType.Linear.ToCurve();
            AnimationCurve.preWrapMode = Loop;
            AnimationCurve.postWrapMode = Loop;
            float maxDuration = LoopCount >= 0 ? LoopCount : float.MaxValue;
            float minDuration = LoopCount >= 0 ? 0 : float.MinValue;

            float delta = Time.deltaTime / Duration.Value;
            if (Forward) _progress += delta;
            else _progress -= delta;
            _progress = Mathf.Clamp(_progress, minDuration, maxDuration);
            var p = _progress;
            if (LoopCount != -1)
            {
                if (Forward && _progress >= LoopCount)
                {
                    if (Loop == WrapMode.Loop) AnimationCurve.postWrapMode = WrapMode.ClampForever;
                    IsRunning = false;
                }
                if (!Forward && _progress <= 0)
                {
                    if (Loop == WrapMode.Loop) AnimationCurve.preWrapMode = WrapMode.ClampForever;
                    IsRunning = false;
                }
            }
            DoTween(AnimationCurve.Evaluate(p));
        }
        internal abstract void Init();
        protected abstract void DoTween(float delta);

        void ITweenGroupElement.Update()
        {
            Update();
        }

        void ITweenGroupElement.Init()
        {
            Init();
        }

        void ITweenGroupElement.OnFinish()
        {
            OnFinish();
        }
    }
}
