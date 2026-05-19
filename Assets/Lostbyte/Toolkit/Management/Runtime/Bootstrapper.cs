using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lostbyte.Toolkit.Common;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lostbyte.Toolkit.Management
{
    [DefaultExecutionOrder(-1000)]
    public class Bootstrapper : MonoBehaviour
    {
        [ClearStatic] private static readonly List<IBootstrapTask> _pendingTasks = new();
        [ClearStatic] private static bool _isRunning;
        [ClearStatic] private static Bootstrapper _instance;
        [ClearStatic] private static TaskCompletionSource<bool> _runningTcs;
        public static bool IsRunning => _isRunning;
        public static Task Finished => _isRunning ? _runningTcs.Task : Task.CompletedTask;
        public static event Action OnQueueCompleted;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoCreate()
        {
            if (SceneManager.GetActiveScene().buildIndex != 0) return;
            if (FindObjectOfType<Bootstrapper>() != null) return;
            var go = new GameObject("Bootstrapper");
            go.AddComponent<Bootstrapper>();
            DontDestroyOnLoad(go);
            Print.MLog("Bootstrapper auto-created");
        }

        private void Awake()
        {
            if (_instance != null)
            {
                DestroyImmediate(gameObject);
                return;
            }
            _instance = this;
            RunAsync().Forget();
        }

        public static void RegisterTask(IBootstrapTask task)
        {
            if (!_pendingTasks.Contains(task))
            {
                _pendingTasks.Add(task);
                _pendingTasks.Sort((a, b) => a.Priority.CompareTo(b.Priority));
                RunAsync().Forget();
            }
        }


        public static async Task RunAsync()
        {
            if (_isRunning || _pendingTasks.Count == 0 || _instance == null) return;
            _isRunning = true;
            _runningTcs = new TaskCompletionSource<bool>();

            Print.MLog($"Starting Task Sequence. Queued: {_pendingTasks.Count}");

            try
            {
                while (_pendingTasks.Count > 0)
                {
                    int currentPriority = _pendingTasks[0].Priority;
                    var batch = new List<IBootstrapTask>();
                    while (_pendingTasks.Count > 0 && _pendingTasks[0].Priority == currentPriority)
                    {
                        batch.Add(_pendingTasks[0]);
                        _pendingTasks.RemoveAt(0);
                    }
                    var executionTasks = batch.Select(ExecuteTaskSafelyAsync);
                    await Task.WhenAll(executionTasks);
                }
            }
            finally
            {
                _isRunning = false;
                Print.MLog("Bootstrapper queue is empty!");
                _runningTcs.TrySetResult(true);

                OnQueueCompleted?.Invoke();
                OnQueueCompleted = null;
            }
        }

        private static async Task ExecuteTaskSafelyAsync(IBootstrapTask task)
        {
            Print.MLog($"Executing: [Priority {task.Priority}] {task.GetType().Name}");
            try
            {
                await task.Execute().ProcessResultAsync(_instance);
            }
            catch (Exception ex)
            {
                Print.MError($"Task {task.GetType().Name} failed: {ex}");
            }
        }
    }
    public interface IBootstrapTask
    {
        int Priority { get; }
        BootstrapResult Execute();
    }
    public readonly struct BootstrapResult
    {
        private readonly Task _task;
        private readonly AsyncOperation _asyncOp;
        private readonly IEnumerator _coroutine;

        private BootstrapResult(Task task, AsyncOperation asyncOp, IEnumerator coroutine)
        {
            _task = task;
            _asyncOp = asyncOp;
            _coroutine = coroutine;
        }

        public static implicit operator BootstrapResult(Task task) => new(task, null, null);
        public static implicit operator BootstrapResult(AsyncOperation asyncOp) => new(null, asyncOp, null);
        public static BootstrapResult From(IEnumerator coroutine) => new(null, null, coroutine);
        public static BootstrapResult Completed => new(null, null, null);

        public async Task ProcessResultAsync(MonoBehaviour instance)
        {
            if (_task != null)
            {
                await _task;
            }
            if (_asyncOp != null)
            {
                var tcs = new TaskCompletionSource<bool>();
                _asyncOp.completed += _ => tcs.TrySetResult(true);
                await tcs.Task;
            }
            if (_coroutine != null)
            {
                var tcs = new TaskCompletionSource<bool>();
                instance.StartCoroutine(CoroutineWrapper(instance, _coroutine, tcs));
                await tcs.Task;
            }
        }
        private IEnumerator CoroutineWrapper(MonoBehaviour instance, IEnumerator coroutine, TaskCompletionSource<bool> tcs)
        {
            yield return instance.StartCoroutine(coroutine);
            tcs.TrySetResult(true);
        }
    }
}
