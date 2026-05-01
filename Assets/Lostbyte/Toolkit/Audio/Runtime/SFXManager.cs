using System.Collections;
using System.Collections.Generic;
using Lostbyte.Toolkit.Management;
using UnityEngine;
using Lostbyte.Toolkit.Common;
using Lostbyte.Toolkit.CustomEditor;


#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Lostbyte.Toolkit.Audio
{
    public class SFXManager : Manager<SFXManager>
    {
        [Header("Settings")]
        [SerializeField, Required] private SFXSource m_SFXSourcePrefab;
        [SerializeField] private float m_MinGap = 0.1f;
        [SerializeField] private int m_MaxSources = 32;

        private readonly Queue<SFXSource> _sfxSourcePool = new();
        private readonly Dictionary<SFXClip, float> _lastPlayed = new();
        private readonly List<SFXSource> _activeSources = new();

#if UNITY_EDITOR
        private static SFXSource _editorSource;
#endif
        internal static void AddToSFXPool(SFXSource source)
        {
            if (Application.isPlaying)
            {
                Instance._sfxSourcePool.Enqueue(source);
                source.gameObject.SetActive(false);
                source.gameObject.hideFlags = HideFlags.HideInHierarchy;
                Instance._activeSources.Remove(source);
            }
        }

        internal static void PlaySFX(Transform parent, Vector3 position, SFXClip track, float delay = 0f)
        {
            if (Application.isPlaying)
            {
                if (track == null || !Instance) return;
                float currentTime = Time.unscaledTime;
                if (Instance._lastPlayed.TryGetValue(track, out float lastTime))
                    if (currentTime < lastTime + Instance.m_MinGap) return;
                Instance._lastPlayed[track] = currentTime;

                if (!Instance._sfxSourcePool.TryDequeue(out var source))
                {
                    source = Instantiate(Instance.m_SFXSourcePrefab, Instance.transform);
                }
                else
                {
                    source.gameObject.SetActive(true);
                    source.gameObject.hideFlags = HideFlags.None;
                }
                Instance._activeSources.Add(source);
                source.Play(parent, position, track, delay);

                while (Instance._activeSources.Count > Instance.m_MaxSources)
                {
                    source = Instance._activeSources[0];
                    source.ForceReturnToPool();
                    DebugLogger.ManagerLogWarning("Max sources reached. Force stop!", source);
                }
            }
#if UNITY_EDITOR
            else
            {
                var source = _editorSource;
                if (source == null)
                {
                    SFXManager instance = FindAnyObjectByType<SFXManager>();
                    source = _editorSource = (PrefabUtility.InstantiatePrefab(instance.m_SFXSourcePrefab.gameObject) as GameObject).GetComponent<SFXSource>();
                    source.gameObject.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
                    source.transform.SetParent(instance.transform);
                }
                source.Play(parent, position, track, delay);
            }
#endif
        }
    }
}
