using UnityEngine;
using UnityEngine.Playables;

namespace Lostbyte.Toolkit.TimelineExtensions
{
    public class UniversalBehaviour : PlayableBehaviour
    {
        public BaseTimelineAction action;
        private GameObject boundObject;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            boundObject = playerData as GameObject;
            action?.ProcessFrame(playable, info, boundObject);
        }

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            if (boundObject != null) action?.OnStart(playable, boundObject);
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            if (boundObject != null) action?.OnStop(playable, boundObject);
        }
    }
}
