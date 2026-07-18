using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lostbyte.Toolkit.Common;
using Lostbyte.Toolkit.Management;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lostbyte.Toolkit.Scenes
{
    public class SceneConstraint
    {
        public string Id;
        public SceneReference ParentRef;
        public List<SceneReference> DesiredScenes = new();
        public List<Scene> ActiveScenes = new();
        public bool UseLoadingScreen;
    }

    [DefaultExecutionOrder(-100)]
    public class SceneManager : Manager<SceneManager>
    {
        public LoadingScreenBase LoadingScreen;

        [ClearStatic] private static readonly Dictionary<Scene, SceneNode> _loadedNodes = new();
        [ClearStatic] private static readonly Dictionary<string, SceneConstraint> _constraints = new();

        private readonly Dictionary<string, Action> _beforeSceneLoadsCallback = new();
        private readonly Dictionary<string, Action> _afterSceneLoadsCallback = new();
        private readonly Dictionary<string, Action> _beforeSceneUnloadsCallback = new();
        private readonly Dictionary<string, Action> _afterSceneUnloadsCallback = new();


        private SceneNode _rootNode;
        private int _loadingScreenFades = 0;
        private bool _initialized = false;

        private bool _isApplyingConstraints = false;
        private bool _stateChanged = false;

        protected override void OnAwake()
        {
            _rootNode = new(gameObject.scene, null);
            _loadedNodes[_rootNode.SceneInstance] = _rootNode;
        }

        private void Start()
        {
#if UNITY_EDITOR
            foreach ((var key, var constraint) in _constraints)
            {
                var parentScene = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(constraint.ParentRef.ScenePath);
                if (!TryGetNode(parentScene, out var parentNode)) parentNode = RegisterNewNode(parentScene, null);
                foreach (var scene in constraint.ActiveScenes)
                {
                    if (TryGetNode(scene, out var node)) node.SetParent(parentNode);
                    else RegisterNewNode(scene, parentNode);
                }
            }

            var orphanedNodes = _loadedNodes.Values.ToList();
            var rootQueue = new Queue<SceneNode>();
            rootQueue.Enqueue(_rootNode);
            while (rootQueue.TryDequeue(out var node))
            {
                orphanedNodes.Remove(node);
                node.Children.ForEach(rootQueue.Enqueue);
                TriggerBeforeSceneLoad(node.Path);
                TriggerAfterSceneLoad(node.Path);

            }
            foreach (var node in orphanedNodes)
            {
                _loadedNodes.Remove(node.SceneInstance);
            }

            UnityEngine.SceneManagement.SceneManager.sceneCount.ToStream()
                .Select(UnityEngine.SceneManagement.SceneManager.GetSceneAt)
                .Where(scene => !_loadedNodes.ContainsKey(scene))
                .ForEach(scene => UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(scene));
#endif
            _initialized = true;
            _stateChanged = true;
            ApplyAllConstraintChanges().Forget();
        }

        public static bool TryRegisterEditorConstraint(string id, SceneReference parent, SceneReference adoptedRef, Scene activeScene, bool useLoadingScreen)
        {
            if (Instance != null) return false;
            bool isParentLoaded = !parent.IsValid || UnityEngine.SceneManagement.SceneManager.GetSceneByPath(parent.ScenePath).isLoaded;
            if (!isParentLoaded) return false;

            if (!_constraints.TryGetValue(id, out var constraint))
            {
                constraint = new SceneConstraint
                {
                    Id = id,
                    ParentRef = parent,
                    UseLoadingScreen = useLoadingScreen
                };
                _constraints[id] = constraint;
            }

            if (!constraint.DesiredScenes.Any(s => s.ScenePath == adoptedRef.ScenePath))
                constraint.DesiredScenes.Add(adoptedRef);

            if (!constraint.ActiveScenes.Contains(activeScene))
                constraint.ActiveScenes.Add(activeScene);

            return true;
        }

        public SceneNode LoadScene(SceneReference scene, Scene parent)
        {
            if (!TryGetNode(parent, out var parentNode)) return null;
            TriggerBeforeSceneLoad(scene.ScenePath);
            UnityEngine.SceneManagement.SceneManager.LoadScene(scene.SceneName, LoadSceneMode.Additive);
            Scene loadedScene = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(scene.ScenePath);
            var node = RegisterNewNode(loadedScene, parentNode);
            TriggerAfterSceneLoad(scene.ScenePath);
            return node;
        }

        public async Task<SceneNode> LoadSceneAsync(SceneReference scene, Scene parent)
        {
            if (!TryGetNode(parent, out var parentNode)) return null;
            TriggerBeforeSceneLoad(scene.ScenePath);
            await WaitOperation(UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(scene.SceneName));
            Scene loadedScene = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(scene.ScenePath);
            var node = RegisterNewNode(loadedScene, parentNode);
            TriggerAfterSceneLoad(scene.ScenePath);
            if (!TryGetNode(parent, out _))
            {
                UnloadScene(node);
                return null;
            }
            return node;
        }

        public void UnloadScene(SceneNode scene) => UnloadNode(scene, null);

        public static void UpdateConstraint(string constraintId, SceneReference parent, List<SceneReference> desiredScenes, bool useLoadingScreen)
        {
            Print.MLog($"Update constraint [{constraintId}]: {parent.SceneName} -> {string.Join(", ", desiredScenes.Select(s => s.SceneName))}");
            if (!_constraints.TryGetValue(constraintId, out var constraint))
            {
                constraint = new SceneConstraint { Id = constraintId };
                _constraints[constraintId] = constraint;
            }

            constraint.ParentRef = parent;
            constraint.DesiredScenes = desiredScenes ?? new List<SceneReference>();
            constraint.UseLoadingScreen = useLoadingScreen;

            constraint.ActiveScenes.RemoveAll(s => !s.IsValid() || !s.isLoaded);

            if (Instance == null || !Instance._initialized) return;

            Instance._stateChanged = true;
            Instance.ApplyAllConstraintChanges().Forget();
        }
        public void AddBeforeSceneLoadedCallback(SceneReference scene, Action callback)
        {
            _beforeSceneLoadsCallback.TryGetValue(scene.ScenePath, out var existingAction);
            _beforeSceneLoadsCallback[scene.ScenePath] = existingAction + callback;
        }

        public void AddAfterSceneLoadedCallback(SceneReference scene, Action callback)
        {
            _afterSceneLoadsCallback.TryGetValue(scene.ScenePath, out var existingAction);
            _afterSceneLoadsCallback[scene.ScenePath] = existingAction + callback;
        }

        public void AddBeforeSceneUnloadedCallback(SceneReference scene, Action callback)
        {
            _beforeSceneUnloadsCallback.TryGetValue(scene.ScenePath, out var existingAction);
            _beforeSceneUnloadsCallback[scene.ScenePath] = existingAction + callback;
        }

        public void AddAfterSceneUnloadedCallback(SceneReference scene, Action callback)
        {
            _beforeSceneUnloadsCallback.TryGetValue(scene.ScenePath, out var existingAction);
            _beforeSceneUnloadsCallback[scene.ScenePath] = existingAction + callback;
        }

        public void RemoveBeforeSceneLoadedCallback(SceneReference scene, Action callback)
        {
            if (!_beforeSceneLoadsCallback.TryGetValue(scene.ScenePath, out var existingAction)) return;
            existingAction -= callback;
            if (existingAction == null) _beforeSceneLoadsCallback.Remove(scene.ScenePath);
            else _beforeSceneLoadsCallback[scene.ScenePath] = existingAction;
        }

        public void RemoveAfterSceneLoadedCallback(SceneReference scene, Action callback)
        {
            if (!_afterSceneLoadsCallback.TryGetValue(scene.ScenePath, out var existingAction)) return;
            existingAction -= callback;
            if (existingAction == null) _afterSceneLoadsCallback.Remove(scene.ScenePath);
            else _afterSceneLoadsCallback[scene.ScenePath] = existingAction;
        }

        public void RemoveBeforeSceneUnloadedCallback(SceneReference scene, Action callback)
        {
            if (!_beforeSceneUnloadsCallback.TryGetValue(scene.ScenePath, out var existingAction)) return;
            existingAction -= callback;
            if (existingAction == null) _beforeSceneUnloadsCallback.Remove(scene.ScenePath);
            else _beforeSceneUnloadsCallback[scene.ScenePath] = existingAction;
        }

        public void RemoveAfterSceneUnloadedCallback(SceneReference scene, Action callback)
        {
            if (!_afterSceneUnloadsCallback.TryGetValue(scene.ScenePath, out var existingAction)) return;
            existingAction -= callback;
            if (existingAction == null) _afterSceneUnloadsCallback.Remove(scene.ScenePath);
            else _afterSceneUnloadsCallback[scene.ScenePath] = existingAction;
        }

        private void TriggerBeforeSceneLoad(string scene)
        {
            if (_beforeSceneLoadsCallback.TryGetValue(scene, out var callbacks)) callbacks?.Invoke();
        }

        private void TriggerAfterSceneLoad(string scene)
        {
            if (_afterSceneLoadsCallback.TryGetValue(scene, out var callbacks)) callbacks?.Invoke();
        }

        private void TriggerBeforeSceneUnload(string scene)
        {
            if (_beforeSceneUnloadsCallback.TryGetValue(scene, out var callbacks)) callbacks?.Invoke();
        }

        private void TriggerAfterSceneUnload(string scene)
        {
            if (_afterSceneUnloadsCallback.TryGetValue(scene, out var callbacks)) callbacks?.Invoke();
        }


        private async Task ApplyAllConstraintChanges()
        {
            if (_isApplyingConstraints) return;
            _isApplyingConstraints = true;
            bool _fadedIn = false;
            try
            {
                while (_stateChanged)
                {
                    _stateChanged = false;
                    Dictionary<string, SceneReference> desiredScenesDict = new();
                    Dictionary<string, string> childToParentMap = new();
                    Dictionary<string, bool> desiredPathFades = new();

                    Queue<string> evalQueue = new();
                    evalQueue.Enqueue(_rootNode.Path);

                    while (evalQueue.TryDequeue(out var currentPath))
                    {
                        foreach (var constraint in _constraints.Values)
                        {
                            if (constraint.ParentRef.IsValid && constraint.ParentRef.ScenePath == currentPath)
                            {
                                foreach (var desiredRef in constraint.DesiredScenes)
                                {
                                    if (!desiredRef.IsValid) continue;
                                    if (desiredPathFades.TryGetValue(desiredRef.ScenePath, out bool existingFade))
                                        desiredPathFades[desiredRef.ScenePath] = existingFade || constraint.UseLoadingScreen;
                                    else
                                        desiredPathFades[desiredRef.ScenePath] = constraint.UseLoadingScreen;

                                    if (desiredScenesDict.TryAdd(desiredRef.ScenePath, desiredRef))
                                    {
                                        evalQueue.Enqueue(desiredRef.ScenePath);
                                        childToParentMap[desiredRef.ScenePath] = currentPath;
                                    }
                                }
                            }
                        }
                    }

                    Dictionary<string, bool> activePathFades = new();
                    foreach (var constraint in _constraints.Values)
                    {
                        foreach (var activeScene in constraint.ActiveScenes)
                        {
                            if (!activeScene.IsValid()) continue;
                            if (activePathFades.TryGetValue(activeScene.path, out bool existingFade))
                                activePathFades[activeScene.path] = existingFade || constraint.UseLoadingScreen;
                            else
                                activePathFades[activeScene.path] = constraint.UseLoadingScreen;
                        }
                    }

                    HashSet<string> currentPaths = _loadedNodes.Values
                        .Where(n => n != _rootNode)
                        .Select(n => n.Path)
                        .ToHashSet();

                    List<string> pathsToLoad = desiredScenesDict.Keys.Except(currentPaths).ToList();
                    List<string> pathsToUnload = currentPaths.Except(desiredScenesDict.Keys).ToList();

                    if (pathsToLoad.Count == 0 && pathsToUnload.Count == 0 && !_fadedIn) continue;
                    bool useFades = false;
                    foreach (var path in pathsToLoad)
                    {
                        if (desiredPathFades.TryGetValue(path, out bool requiresFade) && requiresFade)
                        {
                            useFades = true;
                            break;
                        }
                    }
                    if (!useFades)
                    {
                        foreach (var path in pathsToUnload)
                        {
                            if (activePathFades.TryGetValue(path, out bool requiresFade) && requiresFade)
                            {
                                useFades = true;
                                break;
                            }
                        }
                    }
                    if (useFades && !_fadedIn)
                    {
                        _fadedIn = true;
                        if (LoadingScreen != null)
                            await LoadingScreen.FadeIn();
                    }
                    try
                    {
                        List<Task> pendingTasks = new();
                        if (pathsToUnload.Count > 0)
                            foreach (var path in pathsToUnload) UnloadNode(GetNodeByPath(path), pendingTasks);

                        if (pathsToLoad.Count > 0)
                        {
                            foreach (var path in pathsToLoad)
                            {
                                if (_stateChanged) break;
                                pendingTasks.Add(LoadAndRegisterSceneAsync(path));
                            }
                        }
                        if (pendingTasks.Count > 0) await Task.WhenAll(pendingTasks);

                        async Task LoadAndRegisterSceneAsync(string path)
                        {
                            TriggerBeforeSceneLoad(path);
                            var op = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(path, LoadSceneMode.Additive);
                            await WaitOperation(op);
                            Scene loadedScene = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(path);
                            string parentPath = childToParentMap.TryGetValue(path, out var p) ? p : _rootNode.Path;
                            SceneNode parentNode = GetNodeByPath(parentPath) ?? _rootNode;
                            RegisterNewNode(loadedScene, parentNode);
                            TriggerAfterSceneLoad(path);
                            foreach (var constraint in _constraints.Values)
                                if (constraint.ParentRef.ScenePath == parentPath && constraint.DesiredScenes.Any(s => s.ScenePath == path))
                                    if (!constraint.ActiveScenes.Contains(loadedScene))
                                        constraint.ActiveScenes.Add(loadedScene);
                        }
                    }
                    catch (Exception ex)
                    {
                        Print.MError($"Constraint application failed during execution: {ex.Message}");
                    }
                    if (_stateChanged) continue;
                    await Task.Yield();
                    if (_stateChanged) continue;
                    await Task.Yield();
                    if (_stateChanged) continue;

                    if (_fadedIn)
                    {
                        if (LoadingScreen != null)
                            await LoadingScreen.FadeOut();
                        _fadedIn = false;
                    }
                }
                Print.MAssert(!_fadedIn, "Fade Out has not been applied!");
            }
            finally
            {
                _isApplyingConstraints = false;
            }
        }

        private void UnloadNode(SceneNode node, List<Task> ops = null)
        {
            if (node == null || node.SceneInstance == null || node.Path == null) return;
            var path = node.Path;
            var childrenCopy = new List<SceneNode>(node.Children);
            foreach (var child in childrenCopy)
                UnloadNode(child, ops);

            if (node != _rootNode)
            {
                node.Parent?.Children.Remove(node);
                TriggerBeforeSceneUnload(path);
                var op = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(node.SceneInstance);
                if (op != null)
                    ops?.Add(WaitOperation(op).Then(() => TriggerAfterSceneUnload(path)));
                else
                    TriggerAfterSceneUnload(path);
                _loadedNodes.Remove(node.SceneInstance);

                foreach (var constraint in _constraints.Values)
                    constraint.ActiveScenes.Remove(node.SceneInstance);
            }
        }

        private async Task WaitOperation(AsyncOperation op)
        {
            if (op == null) return;
            while (!op.isDone) await Task.Yield();
        }

        public static bool TryGetNode(Scene scene, out SceneNode node)
        {
            return _loadedNodes.TryGetValue(scene, out node);
        }

        public static SceneNode RegisterNewNode(Scene scene, SceneNode parent)
        {
            var node = new SceneNode(scene, parent);
            _loadedNodes[scene] = node;
            return node;
        }

        private static SceneNode GetNodeByPath(string path)
        {
            foreach (var kvp in _loadedNodes)
                if (kvp.Value.Path == path) return kvp.Value;
            return null;
        }

        public static bool SceneIsRegistered(Scene? scene) => scene.HasValue && _loadedNodes.ContainsKey(scene.Value);
    }
}