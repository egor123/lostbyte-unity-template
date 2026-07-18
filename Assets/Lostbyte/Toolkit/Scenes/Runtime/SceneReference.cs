using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lostbyte.Toolkit.Scenes
{
    [Serializable]
    public struct SceneReference
    {
        [SerializeField] private UnityEngine.Object m_sceneAsset;
        [SerializeField] private string m_scenePath;
        public readonly bool IsValid => !string.IsNullOrEmpty(m_scenePath);
        public readonly string ScenePath => m_scenePath;
        public string SceneName
        {
            get
            {
                if (!IsValid) return string.Empty;
                int slash = m_scenePath.LastIndexOf('/');
                int dot = m_scenePath.LastIndexOf('.');
                return m_scenePath.Substring(slash + 1, dot - slash - 1);
            }
        }
        public SceneReference(string path)
        {
            m_scenePath = path;
            m_sceneAsset = null;
        }
        public override readonly bool Equals(object obj) => obj is SceneReference scene && scene.ScenePath == m_scenePath;
        public override readonly int GetHashCode() => m_scenePath.GetHashCode();
    }
}
