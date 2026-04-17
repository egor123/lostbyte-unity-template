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
        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            if (m_sceneAsset != null)
            {
                if (m_sceneAsset is UnityEditor.SceneAsset)
                {
                    m_scenePath = UnityEditor.AssetDatabase.GetAssetPath(m_sceneAsset);
                }
                else
                {
                    m_sceneAsset = null;
                    m_scenePath = string.Empty;
                }
            }
            else
            {
                m_scenePath = string.Empty;
            }
#endif
        }
    }
}
