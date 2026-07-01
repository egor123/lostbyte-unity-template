using System;
using System.Collections.Generic;
using System.Linq;
using Lostbyte.Toolkit.Common;
using Lostbyte.Toolkit.CustomEditor;
using Lostbyte.Toolkit.FactSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lostbyte.Toolkit.Scenes
{
    [Tag("Scene Management")]
    [SupportedFactTypes(typeof(Enum))]
    public class SceneLoadReaction : FactReaction
    {
        [Serializable]
        public struct SceneCondition
        {
            [SerializeReference] public Enum Condition;
            public SceneReference Scene;
        }

        public SceneReference ParentScene;
        public bool UseLoadingScreen = true;
        public List<SceneCondition> Scenes = new();

        private Enum _currentScene;
        private string _constraintId;

        public override FactReaction Copy() => new SceneLoadReaction()
        {
            ParentScene = ParentScene,
            UseLoadingScreen = UseLoadingScreen,
            Scenes = new List<SceneCondition>(Scenes)
        };

        public override void Initialize(KeyContainer key, FactDefinition fact)
        {
            _constraintId = Guid.NewGuid().ToString();
            bool adoptedEditorScenes = false;
            _currentScene = null;
#if UNITY_EDITOR
            Dispose();
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                Scene activeScene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                foreach (var sceneData in Scenes)
                {
                    if (sceneData.Scene.ScenePath == activeScene.path && activeScene.isLoaded)
                    {
                        if (SceneManager.TryRegisterEditorConstraint(_constraintId, ParentScene, sceneData.Scene, activeScene, UseLoadingScreen))
                        {
                            _currentScene = sceneData.Condition;
                            key.GetWrapper(fact).RawValue = sceneData.Condition;
                            adoptedEditorScenes = true;
                            break;
                        }
                    }
                }
                if (adoptedEditorScenes) break;
            }
#endif
            base.Initialize(key, fact);
            if (!adoptedEditorScenes) OnValueChanged(Wrapper.RawValue);
        }

        protected override void OnValueChanged(object newValue)
        {
            var newCondition = (Enum)newValue;
            if (newCondition.Equals(_currentScene)) return;

            _currentScene = newCondition;

            var desiredScenes = Scenes
                .Where(s => s.Condition.Equals(newCondition) && s.Scene.IsValid)
                .Select(s => s.Scene)
                .ToList();

            SceneManager.UpdateConstraint(_constraintId, ParentScene, desiredScenes, UseLoadingScreen);
        }
    }
}