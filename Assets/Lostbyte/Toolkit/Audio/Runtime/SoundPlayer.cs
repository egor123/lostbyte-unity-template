using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lostbyte.Toolkit.Audio
{
    public class SoundPlayer : MonoBehaviour
    {
        [SerializeField] private SFXClip m_track;
        public void Play() => m_track.Play(transform.position);
    }
}
