using System.Reflection;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Timeline;

namespace Lostbyte.Toolkit.TimelineExtensions.Editor
{
    [CustomTimelineEditor(typeof(Object))]
    public class UniversalClipEditor : ClipEditor
    {
        public override ClipDrawOptions GetClipOptions(TimelineClip clip)
        {
            var options = base.GetClipOptions(clip);
            var asset = clip.asset as UniversalClip;

            if (asset != null && asset.action != null)
            {
                var attr = asset.action.GetType().GetCustomAttribute<TimelineExtensionAttribute>();
                if (attr != null && ColorUtility.TryParseHtmlString(attr.ColorHex, out Color color))
                    options.highlightColor = color;
            }
            return options;
        }

        public override void OnCreate(TimelineClip clip, TrackAsset track, TimelineClip clonedFrom)
        {
            var asset = clip.asset as UniversalClip;
            if (asset != null && asset.action != null)
            {
                var attr = asset.action.GetType().GetCustomAttribute<TimelineExtensionAttribute>();
                clip.displayName = attr != null ? attr.Name : asset.action.GetType().Name;
            }
        }
    }
}
