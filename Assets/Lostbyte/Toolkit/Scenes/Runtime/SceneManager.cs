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

        public static void UpdateConstraint(string constraintId, SceneReference parent, List<SceneReference> desiredScenes, bool useLoadingScreen)
        {
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

        private async Task ApplyAllConstraintChanges()
        {
            if (_isApplyingConstraints) return;
            _isApplyingConstraints = true;
            try
            {
                while (_stateChanged)
                {
                    _stateChanged = false;
                    Dictionary<string, SceneReference> desiredScenesDict = new();
                    Dictionary<string, string> childToParentMap = new();
                    Dictionary<string, bool> desiredPathFades = new();

                    Queue<string> evalQueue = new();
                    evalQueue.Enqueue(_rootNode.ScenePath);

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
                        .Select(n => n.ScenePath)
                        .ToHashSet();

                    List<string> pathsToLoad = desiredScenesDict.Keys.Except(currentPaths).ToList();
                    List<string> pathsToUnload = currentPaths.Except(desiredScenesDict.Keys).ToList();

                    if (pathsToLoad.Count == 0 && pathsToUnload.Count == 0) continue;
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
                    try
                    {
                        if (useFades) await HandleFades(true);
                        List<Task> pendingTasks = new();
                        if (pathsToUnload.Count > 0)
                        {
                            List<AsyncOperation> ops = new();
                            foreach (var path in pathsToUnload) UnloadNode(GetNodeByPath(path), ops);
                            foreach (var op in ops) pendingTasks.Add(WaitOperation(op));
                        }
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
                            var sceneRef = desiredScenesDict[path];
                            var op = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneRef.SceneName, LoadSceneMode.Additive);
                            await WaitOperation(op);
                            Scene loadedScene = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(sceneRef.ScenePath);
                            string parentPath = childToParentMap.TryGetValue(path, out var p) ? p : _rootNode.ScenePath;
                            SceneNode parentNode = GetNodeByPath(parentPath) ?? _rootNode;
                            RegisterNewNode(loadedScene, parentNode);
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
                    finally
                    {
                        if (useFades) await HandleFades(false);
                    }
                }
            }
            finally
            {
                _isApplyingConstraints = false;
            }
        }

        private void UnloadNode(SceneNode node, List<AsyncOperation> ops)
        {
            if (node == null) return;
            var childrenCopy = new List<SceneNode>(node.Children);
            foreach (var child in childrenCopy)
                UnloadNode(child, ops);

            if (node != _rootNode)
            {
                node.Parent?.Children.Remove(node);
                var op = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(node.SceneInstance);
                if (op != null) ops?.Add(op);
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

        private async Task HandleFades(bool fadeIn)
        {
            if (LoadingScreen == null) return;
            if (fadeIn)
            {
                _loadingScreenFades++;
                if (_loadingScreenFades == 1)
                {
                    LoadingScreen.FadeIn();
                    while (LoadingScreen.InTransition) await Task.Yield();
                }
            }
            else
            {
                _loadingScreenFades = Mathf.Max(0, _loadingScreenFades - 1);
                if (_loadingScreenFades == 0)
                {
                    LoadingScreen.FadeOut();
                    while (LoadingScreen.InTransition) await Task.Yield();
                }
            }
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
                if (kvp.Value.ScenePath == path) return kvp.Value;
            return null;
        }

        public static bool SceneIsRegistered(Scene? scene) => scene.HasValue && _loadedNodes.ContainsKey(scene.Value);
    }
}