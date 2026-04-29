using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lostbyte.Toolkit.Common;
using Lostbyte.Toolkit.FactSystem;
using Lostbyte.Toolkit.Management;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lostbyte.Toolkit.Scenes
{
    public class SceneLoader : MonoBehaviour
    {
        [Serializable]
        private struct SceneCondition
        {
            [SerializeReference] public Enum Condition;
            public SceneReference Scene;
        }

        [SerializeField] private FactWrapper<Enum> m_fact;
        [SerializeField] private LoadingScreenBase m_loadingScreen;
        [SerializeField] private List<SceneCondition> m_scenes;

        private readonly HashSet<Scene> _loaded_scenes = new();
        private readonly SubscriptionGroup _subscriptions = new();

        private bool _isTransitioning = false;
        private Enum _currentScene;

#if UNITY_EDITOR
        private void Awake()
        {
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                Scene activeScene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);

                foreach (var sceneData in m_scenes)
                {
                    if (sceneData.Scene.ScenePath == activeScene.path && activeScene.isLoaded)
                    {
                        SceneManager.Instance.RegisterChildScene(gameObject.scene, activeScene);
                        _loaded_scenes.Add(activeScene);
                        m_fact.Value = sceneData.Condition;
                        _currentScene = sceneData.Condition;
                        return;
                    }
                }
            }
        }
#endif
        private void Start()
        {
            if (m_fact.Fact != null)
            {
                ManageSceneChange(m_fact.Value);
                _subscriptions.Subscribe<Enum>(m_fact.Subscribe, m_fact.Unsubscribe, ManageSceneChange);
            }
        }

        private void ManageSceneChange(Enum e)
        {
            if (e.Equals(_currentScene)) return;
            if (_isTransitioning) return;
            StartCoroutine(TransitionRoutine(e));
        }

        private IEnumerator TransitionRoutine(Enum e)
        {
            _isTransitioning = true;
            Debug.Log($"Loading: {e}");

            if (m_loadingScreen != null)
            {
                m_loadingScreen.FadeIn();
                if (_currentScene == null) m_loadingScreen.Skip();
                while (m_loadingScreen.InTransition) yield return null;
            }

            while (!m_fact.Value.Equals(_currentScene))
            {
                Task loadingTask = HandleSceneLoading(m_fact.Value);
                yield return new WaitUntil(() => loadingTask.IsCompleted);
                if (loadingTask.IsFaulted)
                {
                    Debug.LogError($"Scene loading failed: {loadingTask.Exception}");
                    break;
                }
                // TODO await sub scene loading
            }

            if (m_loadingScreen != null)
            {
                m_loadingScreen.FadeOut();
                while (m_loadingScreen.InTransition) yield return null;
            }

            _isTransitioning = false;
            ManageSceneChange(m_fact.Value); // handle when change happened on fade out
        }

        private async Task HandleSceneLoading(Enum e)
        {
            var unloadTasks = _loaded_scenes.Select(scene => SceneManager.Instance.UnloadSceneAsync(scene));
            var loadTasks = m_scenes.Where(s => s.Condition.Equals(e))
                .Select(sceneData => SceneManager.Instance.LoadSceneAsync(sceneData.Scene, gameObject.scene));
            await Task.WhenAll(unloadTasks);
            _loaded_scenes.Clear();
            foreach (var scene in await Task.WhenAll(loadTasks))
                if (scene != null) AddLoadedScene(scene.Value);
            _currentScene = e;
        }

        private void AddLoadedScene(Scene? scene)
        {
            if (scene.HasValue) _loaded_scenes.Add(scene.Value);
        }

        private void OnDestroy()
        {
            _subscriptions.Dispose();
        }
    }
}