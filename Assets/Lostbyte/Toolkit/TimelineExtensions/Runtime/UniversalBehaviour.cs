using UnityEngine;
using UnityEngine.Playables;

namespace Lostbyte.Toolkit.TimelineExtensions
{
    public class UniversalBehaviour : PlayableBehaviour
    {
        public BaseTimelineAction action;
        private Object boundObject;
        private bool hasStarted = false;

        
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            boundObject = playerData as Object;
            if (!hasStarted)
            {
                action?.OnStart(playable, boundObject);
                hasStarted = true;
            }
            action?.ProcessFrame(playable, info, boundObject);
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            if (hasStarted)
            {
                action?.OnStop(playable, boundObject);
                hasStarted = false;
            }
        }
    }
}
