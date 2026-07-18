using Lostbyte.Toolkit.CustomEditor;
using UnityEngine;

namespace Lostbyte.Toolkit.Audio.Music
{
    public abstract class MusicTrackNode : ScriptableObject
    {
        [Hide] public Vector2 Position;
        public abstract MusicSegmentData GetCurrentSegment();
        public abstract MusicSegmentData GetNextSegment();
    }
}