using System;

namespace Lostbyte.Toolkit.TimelineExtensions
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TimelineExtensionAttribute : Attribute
    {
        public Type BindingType { get; set; } = typeof(UnityEngine.GameObject);
        public string Name { get; set; } = "Custom Action";
        public string ColorHex { get; set; } = "#FFFFFF";
    }
}
