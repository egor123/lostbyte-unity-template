using System;
using System.Collections.Generic;
using System.Linq;
using Lostbyte.Toolkit.CustomEditor;
using UnityEngine;

namespace Lostbyte.Toolkit.Director
{
    public class Director : MonoBehaviour
    {
        public static Director Instance { get; private set; }
        [field: SerializeField, ReadOnly] public bool IsPlaying { get; private set; }
        [field: SerializeField, ReadOnly] public Priority CurrentPriority { get; private set; } = Priority.Low;
        private readonly Dictionary<Priority, Queue<IPlayableClipBehaviour>> _tracks = new();
        private static Priority[] _priorities;
        internal static Priority[] Priorities => _priorities ??= Enum.GetValues(typeof(Priority)).Cast<Priority>().ToArray();
        private void Awake()
        {
            Instance = this;
        }
        public void Schedule(IPlayableData data, Priority priority = Priority.Default) => Schedule(data.GetClip(), priority);
        public void Schedule(IPlayableClipBehaviour clip, Priority priority = Priority.Default)
        {
            if (!_tracks.TryGetValue(priority, out var queue))
                queue = _tracks[priority] = new();
            if (IsPlaying)
            {
                if (priority > CurrentPriority)
                {
                    InteruptClip(CurrentPriority);
                    CurrentPriority = priority;
                    queue.Enqueue(clip);
                    clip.OnStart();
                }
                else if (clip.SchedulingBehaviour == OnContinueBehaviour.Schedule)
                {
                    queue.Enqueue(clip);
                }
            }
            else
            {
                CurrentPriority = priority;
                clip.Time = 0;
                queue.Enqueue(clip);
                clip.OnStart();
                IsPlaying = true;
            }
        }
        private void InteruptClip(Priority priority)
        {
            IPlayableClipBehaviour clip = _tracks[priority].Peek();
            switch (clip.InteruptBehaviour)
            {
                case InteruptBehaviour.Restart:
                    clip.OnEnd();
                    clip.Time = 0;
                    break;
                case InteruptBehaviour.Skip:
                    clip.OnEnd();
                    _tracks[priority].Dequeue();
                    break;
                case InteruptBehaviour.Continue:
                    clip.OnPause();
                    break;
            }
        }
        private Priority StartClip()
        {
            for (int i = Priorities.Length - 1; i >= 0; i--)
            {
                Priority priority = Priorities[i];
                if (_tracks.ContainsKey(priority) && _tracks[priority].TryPeek(out var clip))
                {
                    if (clip.Time == 0) clip.OnStart();
                    else clip.OnContinue();
                    return priority;
                }
            }
            IsPlaying = false;
            return Priority.Low;
        }
        private void Update()
        {
            if (!IsPlaying) return;
            IPlayableClipBehaviour clip = _tracks[CurrentPriority].Peek();
            if (!clip.IsFinished())
            {
                clip.Time += Time.deltaTime;
                clip.OnUpdate();
            }
            else
            {
                clip.OnEnd();
                _tracks[CurrentPriority].Dequeue();
                CurrentPriority = StartClip();
            }
        }
        private void OnDestroy()
        {
            if(IsPlaying)_tracks[CurrentPriority].Peek().OnEnd();
        }
    }
    public enum Priority : int { Low, Default, Important, Vital }

    public enum InteruptBehaviour { Restart, Continue, Skip }
    public enum OnContinueBehaviour { Schedule, Drop }
    public interface IPlayableClipBehaviour
    {
        InteruptBehaviour InteruptBehaviour { get; }
        OnContinueBehaviour SchedulingBehaviour { get; }
        float Time { get; set; }
        bool IsFinished();
        void OnStart();
        void OnPause();
        void OnContinue();
        void OnUpdate();
        void OnEnd();
    }
    public interface IPlayableData
    {
        InteruptBehaviour InteruptBehaviour { get; }
        OnContinueBehaviour OnContinueBehaviour { get; }
        IPlayableClipBehaviour GetClip();
    }
    public abstract class PlayableBehaviour<T> : IPlayableClipBehaviour where T : IPlayableData
    {
        public T Data { get; private set; }
        public PlayableBehaviour(T data) => Data = data;
        public InteruptBehaviour InteruptBehaviour => Data.InteruptBehaviour;
        public OnContinueBehaviour SchedulingBehaviour => Data.OnContinueBehaviour;
        public float Time { get; set; }

        public abstract bool IsFinished();
        public abstract void OnStart();
        public abstract void OnEnd();
        public abstract void OnPause();
        public abstract void OnContinue();
        public abstract void OnUpdate();
    }
}