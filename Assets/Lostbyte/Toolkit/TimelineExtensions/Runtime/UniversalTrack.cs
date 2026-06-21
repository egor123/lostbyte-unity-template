using System.ComponentModel;
using UnityEngine;
using UnityEngine.Timeline;

namespace Lostbyte.Toolkit.TimelineExtensions
{
    [TrackColor(0.5f, 0.5f, 0.5f)]
    [TrackBindingType(typeof(Object))]
    [TrackClipType(typeof(UniversalClip))]
    [DisplayName("Custom/Universal Track")]
    public class UniversalTrack : TrackAsset { }
}
