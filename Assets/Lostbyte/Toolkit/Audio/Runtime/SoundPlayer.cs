using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lostbyte.Toolkit.Audio
{
    public class SoundPlayer : MonoBehaviour
    {
        [SerializeField] private SFXTrack m_track;
        public void Play() => m_track.Play(transform.position);
    }
}
