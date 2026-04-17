using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lostbyte.Toolkit.Tween
{
    internal interface ITweenGroupElement
    {
        void Play();
        void Stop();
        void Reset();
        void Update();
        bool IsRunning { get; set; }
        void Finish();
        void Init();
        void OnFinish();
        bool Forward { get; set; }
    }
    public abstract class TweenGroup : ITweenGroupElement
    {
        internal TweenGroup(MonoBehaviour initiator) => Initiator = initiator;
        internal MonoBehaviour Initiator { get; private set; }
        protected internal Action Callback { private get; set; }
        public bool IsRunning { get; protected set; }
        internal List<ITweenGroupElement> Tweens { get; private set; } = new();
        private Coroutine _runningCorutine;
        protected bool _initiated = false;
        internal bool Forward { get; set; } = true;
        bool ITweenGroupElement.IsRunning { get => IsRunning; set => IsRunning = value; }
        bool ITweenGroupElement.Forward { get; set; } = true;

        public void Play()
        {
            if (Initiator == null && !Initiator.enabled || IsRunning) return;
            IsRunning = true;
            _runningCorutine = Initiator.StartCoroutine(Enumerator());
        }
        public void Stop()
        {
            IsRunning = false;
            if (_runningCorutine != null) Initiator.StopCoroutine(_runningCorutine);
        }
        public void Reset()
        {
            Stop();
            for (int i = Tweens.Count - 1; i >= 0; i--)
                Tweens[i].Reset();
            _initiated = false;
        }
        public void Finish()
        {
            Stop();
            foreach (var tween in Tweens)
                tween.Finish();
            OnFinish();
        }
        protected virtual IEnumerator Enumerator()
        {
            if (!_initiated)
            {
                Init();
                _initiated = true;
            }
            while (IsRunning)
            {
                Update();
                yield return null;
            }
            OnFinish();
            _initiated = false;
        }
        internal void OnFinish()
        {
            Callback?.Invoke();
        }
        protected abstract void Init();
        protected abstract void Update();

        void ITweenGroupElement.Update() => Update();

        void ITweenGroupElement.Init() => Init();

        void ITweenGroupElement.OnFinish()=>OnFinish();
    }
    internal class ParallelTweenGroup : TweenGroup
    {
        public ParallelTweenGroup(MonoBehaviour initiator) : base(initiator)
        {
        }

        protected override void Init()
        {
            foreach (var tween in Tweens)
            {
                tween.Forward = true;
                tween.IsRunning = true;
                tween.Init();
            }
        }
        protected override void Update()
        {
            bool exit = true;
            foreach (var tween in Tweens)
            {
                if (tween.IsRunning)
                {
                    tween.Update();
                    if (!tween.IsRunning) tween.OnFinish();
                }
                if (tween.IsRunning) exit = false;
            }
            if (exit)
            {
                IsRunning = false;
            }
        }
    }
    internal class SequantialTweenGroup : TweenGroup
    {
        private int _current = 0;

        public SequantialTweenGroup(MonoBehaviour initiator) : base(initiator)
        {
        }

        protected override void Init()
        {
            _current = -1;
            StartNext();
        }
        protected override void Update()
        {
            if (_current >= Tweens.Count) IsRunning = false;
            else
            {
                Tweens[_current].Update();
                while (_current < Tweens.Count && !Tweens[_current].IsRunning)
                    StartNext();
            }
        }
        private void StartNext()
        {
            if (_current >= 0 && _current < Tweens.Count)
                Tweens[_current].OnFinish();
            _current++;
            if (_current >= Tweens.Count) IsRunning = false;
            else
            {
                var tween = Tweens[_current];
                tween.Forward = true;
                tween.IsRunning = true;
                tween.Init();
            }
        }
    }
}
