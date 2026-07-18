using System;
using Lostbyte.Toolkit.CustomEditor.Graphs;
using UnityEngine;

namespace Lostbyte.Toolkit.Audio.Music
{
    [Serializable, GraphElement]
    public struct StemData
    {
        [GraphField] public AudioClip Clip;
        [GraphField, Range(0f, 1f)] public float DefaultVolume;
    }
}
