using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        public CancellationTokenSource Cts;
    }

    [DefaultExecutionOrder(-100)]
    public class SceneManager : Manager<SceneManager>
    {
        public LoadingScreenBase LoadingScreen;

        [ClearStatic] private static readonly Dictionary<Scene, SceneNode> _loadedNodes = new();
        [ClearStatic] private static readonly Dictionary<string, SceneConstraint> _constraints = new();

        private SceneNode _rootNode;
        private int _loadingScreenFades = 0;

        protected override void OnAwake()
        {
            _rootNode = new(gameObject.scene, null);
            _loadedNodes[_rootNode.SceneInstance] = _rootNode;
        }
#if UNITY_EDITOR
        private void Start()
        {
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

            foreach (var constraint in _constraints.Values)
                ApplyConstraintAsync(constraint, constraint.Cts.Token).Forget();
        }
#endif
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
                    UseLoadingScreen = useLoadingScreen,
                    Cts = new CancellationTokenSource()
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
            constraint.Cts?.Cancel();
            constraint.Cts?.Dispose();
            constraint.Cts = new CancellationTokenSource();

            constraint.ParentRef = parent;
            constraint.DesiredScenes = desiredScenes ?? new List<SceneReference>();
            constraint.UseLoadingScreen = useLoadingScreen;

            constraint.ActiveScenes.RemoveAll(s => !s.IsValid() || !s.isLoaded);

            if (Instance == null) return;
            if (!parent.IsValid || GetNodeByPath(parent.ScenePath) != null)
                Instance.ApplyConstraintAsync(constraint, constraint.Cts.Token).Forget();
        }

        private async Task ApplyConstraintAsync(SceneConstraint constraint, CancellationToken token)
        {
            var currentPaths = constraint.ActiveScenes.Select(s => s.path).ToList();
            var desiredPaths = constraint.DesiredScenes.Select(s => s.ScenePath).ToList();

            var scenesToUnload = constraint.ActiveScenes.Where(s => !desiredPaths.Contains(s.path)).ToList();
            var scenesToLoad = constraint.DesiredScenes.Where(s => !currentPaths.Contains(s.ScenePath)).ToList();

            if (scenesToUnload.Count == 0 && scenesToLoad.Count == 0) return;

            bool useFades = constraint.UseLoadingScreen;
            try
            {
                if (useFades) await HandleFades(true);

                if (scenesToUnload.Count > 0)
                {
                    List<Task> unloadTasks = new();
                    foreach (var scene in scenesToUnload)
                    {
                        if (_loadedNodes.TryGetValue(scene, out SceneNode node))
                        {
                            List<AsyncOperation> ops = new();
                            UnloadNode(node, ops);
                            foreach (var op in ops) unloadTasks.Add(WaitOperation(op));
                        }
                        constraint.ActiveScenes.Remove(scene);
                    }
                    await Task.WhenAll(unloadTasks);
                }

                if (token.IsCancellationRequested) return;

                if (scenesToLoad.Count > 0)
                {
                    SceneNode parentNode = constraint.ParentRef.IsValid ? GetNodeByPath(constraint.ParentRef.ScenePath) : _rootNode;

                    foreach (var sceneRef in scenesToLoad)
                    {
                        if (!sceneRef.IsValid) continue;

                        var op = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneRef.SceneName, LoadSceneMode.Additive);
                        await WaitOperation(op);

                        Scene loadedScene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(UnityEngine.SceneManagement.SceneManager.sceneCount - 1);
                        var newNode = RegisterNewNode(loadedScene, parentNode);
                        constraint.ActiveScenes.Add(loadedScene);

                        if (token.IsCancellationRequested) return;

                        await EvaluateConstraintsForParent(newNode);
                    }
                }
            }
            catch (Exception ex)
            {
                Print.MError($"Constraint application failed: {ex.Message}");
            }
            finally
            {
                if (useFades) await HandleFades(false);
            }
        }

        private async Task EvaluateConstraintsForParent(SceneNode parentNode)
        {
            var constraintsToTrigger = _constraints.Values
                .Where(c => c.ParentRef.IsValid && c.ParentRef.ScenePath == parentNode.ScenePath)
                .ToList();

            foreach (var constraint in constraintsToTrigger)
            {
                await ApplyConstraintAsync(constraint, constraint.Cts.Token);
            }
        }

        private void UnloadNode(SceneNode node, List<AsyncOperation> ops)
        {
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