using System;

namespace Core.CustomEditor.Editor
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NodeTypeAttribute : Attribute
    {
        public Type Type { get; private set; }
        public string Name { get; private set; }
        public NodeTypeAttribute(Type type, string name) => (Type, Name) = (type, name);
    }
}
