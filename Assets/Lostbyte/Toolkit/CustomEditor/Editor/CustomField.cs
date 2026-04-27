using System;

namespace Lostbyte.Toolkit.CustomEditor.Editor
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CustomFieldAttribute : Attribute
    {
        public Type TargetType { get; }

        public CustomFieldAttribute(Type targetType)
        {
            TargetType = targetType;
        }

    }
}