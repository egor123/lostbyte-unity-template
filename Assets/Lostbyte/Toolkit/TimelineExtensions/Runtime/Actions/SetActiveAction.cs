using UnityEngine;
using UnityEngine.Playables;

namespace Lostbyte.Toolkit.TimelineExtensions
{
    [TimelineExtension(Name = "GameObject/Set Active", BindingType = typeof(GameObject), ColorHex = "#e0e0e0")]
    public class SetActiveAction : BaseTimelineAction
    {
        public bool Value = true;

        public override void OnStart(Playable playable, Object boundObject)
        {
            if(boundObject is GameObject gameObject)
                gameObject.SetActive(Value);
        }
    }
}
