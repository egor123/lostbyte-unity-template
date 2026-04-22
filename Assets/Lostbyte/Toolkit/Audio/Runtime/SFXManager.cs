using System.Collections;
using System.Collections.Generic;
using Lostbyte.Toolkit.Management;
using UnityEngine;

namespace Lostbyte.Toolkit.Audio
{
    public class SFXManager : Manager<SFXManager>
    {

        [SerializeField] private SFXSource m_SFXSourcePrefab;
        private readonly Queue<SFXSource> _sfxSourcePool = new();

        internal void AddToSFXPool(SFXSource source) => _sfxSourcePool.Enqueue(source);

        internal void PlaySFX(Transform parent, Vector3 position, SFXClip track, float delay = 0f)
        {
            if (!_sfxSourcePool.TryDequeue(out var source))
                source = Instantiate(m_SFXSourcePrefab, transform);
            source.Play(parent, position, track, delay);
        }
    }
}
