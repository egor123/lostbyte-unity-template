using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lostbyte.Toolkit.Scenes
{
    [Serializable]
    public class SceneReference
    {
        [SerializeField] private UnityEngine.Object m_sceneAsset;
        [SerializeField] private string m_scenePath;
        public bool IsValid => !string.IsNullOrEmpty(m_scenePath);
        public string ScenePath => m_scenePath;
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
    }
}
