using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Lostbyte.Toolkit.TimelineExtensions
{
    [Serializable]
    public abstract class BaseTimelineAction
    {
        public virtual void OnStart(Playable playable, UnityEngine.Object boundObject) { }
        public virtual void ProcessFrame(Playable playable, FrameData info, UnityEngine.Object boundObject) { }
        public virtual void OnStop(Playable playable, UnityEngine.Object boundObject) { }
    }
}
