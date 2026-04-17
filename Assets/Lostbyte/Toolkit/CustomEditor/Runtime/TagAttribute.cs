using System;

namespace Lostbyte.Toolkit.CustomEditor
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TagAttribute : Attribute
    {
        public string Tag;

        public TagAttribute(string tag) { Tag = tag; }
    }
}