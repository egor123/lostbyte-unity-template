using System;
using System.Collections.Generic;
using System.Linq;
using Lostbyte.Toolkit.Common;
using Lostbyte.Toolkit.CustomEditor;
using UnityEngine;
using UnityEngine.Playables;

namespace Lostbyte.Toolkit.Director
{
    [DefaultExecutionOrder(-50)]
    public class Director : MonoBehaviour
    {
        public static Director Instance { get; private set; }
        [field: SerializeField, Autowired, Required] public PlayableDirector Player { get; private set; }
        [field: SerializeField, ReadOnly] public bool IsPlaying { get; private set; }
        [field: SerializeField, ReadOnly] public Priority CurrentPriority { get; private set; } = Priority.Low;
        private Queue<IPlayableClipBehaviour>[] _tracks;
        [ClearStatic] private static readonly Queue<(IPlayableClipBehaviour clip, Priority priority)> _queue = new();
        private static Priority[] _priorities;
        internal static Priority[] Priorities => _priorities ??= Enum.GetValues(typeof(Priority)).Cast<Priority>().ToArray();
        private void Awake()
        {
            Instance = this;
            _tracks = new Queue<IPlayableClipBehaviour>[Priorities.Length];
            for (int i = 0; i < Priorities.Length; i++)
            {
                _tracks[i] = new Queue<IPlayableClipBehaviour>();
            }
            while (_queue.TryDequeue(out var data)) HandleQueue(data.clip, data.priority);
        }
        public static void Schedule(IPlayableData data, Priority priority = Priority.Default) => Schedule(data.GetClip(), priority);
        public static void Schedule(IPlayableClipBehaviour clip, Priority priority = Priority.Default)
        {
            if (Instance == null) _queue.Enqueue((clip, priority));
            else Instance.HandleQueue(clip, priority);
        }
        public void ClearQueue()
        {
            _queue.Clear();

            for (int i = 0; i < _tracks.Length; i++)
            {
                if (IsPlaying && i == (int)CurrentPriority)
                {
                    if (_tracks[i].TryPeek(out var activeClip))
                    {
                        _tracks[i].Clear();
                        _tracks[i].Enqueue(activeClip);
                        continue;
                    }
                }
                _tracks[i].Clear();
            }
        }
        private void HandleQueue(IPlayableClipBehaviour clip, Priority priority)
        {
            var queue = _tracks[(int)priority];

            if (IsPlaying)
            {
                if (priority > CurrentPriority)
                {
                    InteruptClip(CurrentPriority);
                    CurrentPriority = priority;
                    queue.Enqueue(clip);
                    SafeExecute(clip.OnStart);
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
                SafeExecute(clip.OnStart);
                IsPlaying = true;
            }
        }
        private void SafeExecute(Action action)
        {
            try { action?.Invoke(); }
            catch (Exception e) { Print.MError($"Clip execution failed: {e}"); }
        }
        private void InteruptClip(Priority priority)
        {
            var queue = _tracks[(int)priority];
            if (!queue.TryPeek(out var clip)) return;

            switch (clip.InteruptBehaviour)
            {
                case InteruptBehaviour.Restart:
                    SafeExecute(clip.OnEnd);
                    clip.Time = 0;
                    break;
                case InteruptBehaviour.Skip:
                    SafeExecute(clip.OnEnd);
                    queue.Dequeue();
                    break;
                case InteruptBehaviour.Continue:
                    SafeExecute(clip.OnPause);
                    break;
            }
        }
        private Priority StartClip()
        {
            for (int i = Priorities.Length - 1; i >= 0; i--)
            {
                Priority priority = Priorities[i];
                var queue = _tracks[(int)priority];

                if (queue.TryPeek(out var clip))
                {
                    if (clip.Time == 0) SafeExecute(clip.OnStart);
                    else SafeExecute(clip.OnContinue);

                    return priority;
                }
            }
            IsPlaying = false;
            return Priority.Low;
        }
        private void Update()
        {
            if (!IsPlaying) return;

            var queue = _tracks[(int)CurrentPriority];
            if (queue.TryPeek(out var clip))
            {
                if (!clip.IsFinished())
                {
                    clip.Time += Time.deltaTime;
                    SafeExecute(clip.OnUpdate);
                }
                else
                {
                    SafeExecute(clip.OnEnd);
                    queue.Dequeue();
                    CurrentPriority = StartClip();
                }
            }
        }

        private void OnDestroy()
        {
            if (IsPlaying && _tracks[(int)CurrentPriority].TryPeek(out var clip))
            {
                clip.OnEnd();
            }
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