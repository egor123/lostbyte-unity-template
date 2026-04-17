using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lostbyte.Toolkit.Management;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lostbyte.Toolkit.Scenes
{
    [DefaultExecutionOrder(-100)]
    public class SceneManager : Manager<SceneManager>
    {
        private readonly Dictionary<Scene, SceneNode> _loadedNodes = new();
        private SceneNode _rootNode;

        protected override void OnAwake()
        {
            _rootNode = new(gameObject.scene, null);
            _loadedNodes[_rootNode.SceneInstance] = _rootNode;
        }

        public Scene? LoadScene(SceneReference scene, Scene? parent = null)
        {
            if (!scene.IsValid)
            {
                Debug.LogError($"Scene is not valid!");
                return null;
            }
            SceneNode parentNode = parent.HasValue ? _loadedNodes.GetValueOrDefault(parent.Value, null) : null;
            UnityEngine.SceneManagement.SceneManager.LoadScene(scene.SceneName, LoadSceneMode.Additive);
            int index = UnityEngine.SceneManagement.SceneManager.sceneCount - 1;
            Scene loadedScene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(index);
            RegisterNewNode(loadedScene, parentNode);
            return loadedScene;
        }
        public async Task<Scene?> LoadSceneAsync(SceneReference scene, Scene? parent = null)
        {
            if (!scene.IsValid)
            {
                Debug.LogError($"Scene is not valid!");
                return null;
            }

            SceneNode parentNode = parent.HasValue ? _loadedNodes.GetValueOrDefault(parent.Value, null) : null;
            var op = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(scene.SceneName, LoadSceneMode.Additive);

            while (!op.isDone)
                await Task.Yield();

            Scene loadedScene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(UnityEngine.SceneManagement.SceneManager.sceneCount - 1);
            RegisterNewNode(loadedScene, parentNode);
            if (parent.HasValue && !_loadedNodes.ContainsKey(parent.Value))
            {
                Debug.LogWarning($"Parent scene was unloaded while loading '{scene.SceneName}' scene");
                UnloadSceneAsync(loadedScene);
                return null;
            }
            return loadedScene;
        }


        public void UnloadSceneAsync(Scene scene)
        {
            if (_loadedNodes.TryGetValue(scene, out SceneNode node))
                UnloadNode(node);
            else
                Debug.LogWarning($"Scene '{scene.name}' is not loaded!");
        }

        public IEnumerator UnloadSceneRoutine(Scene scene)
        {
            if (_loadedNodes.TryGetValue(scene, out SceneNode node))
            {
                List<AsyncOperation> ops = new();
                UnloadNode(node, ops);
                foreach (var op in ops)
                    yield return op;
            }
        }

        private void UnloadNode(SceneNode node, List<AsyncOperation> ops = null)
        {
            var childrenCopy = new List<SceneNode>(node.Children);
            foreach (var child in childrenCopy)
                UnloadNode(child);

            if (node != _rootNode)
            {
                node.Parent?.Children.Remove(node);
                var op = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(node.ScenePath);
                ops?.Add(op);
                _loadedNodes.Remove(node.SceneInstance);
            }
        }

        private void RegisterNewNode(Scene scene, SceneNode parent)
        {
            _loadedNodes[scene] = new(scene, parent);
        }
    }
}
