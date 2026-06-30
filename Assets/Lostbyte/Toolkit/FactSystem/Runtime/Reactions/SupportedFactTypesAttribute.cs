using System;

namespace Lostbyte.Toolkit.FactSystem
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class SupportedFactTypesAttribute : Attribute
    {
        public Type[] ValidTypes { get; }
        public SupportedFactTypesAttribute(params Type[] types) => ValidTypes = types;
        public bool IsTypeSupported(Type targetType)
        {
            if (ValidTypes == null || ValidTypes.Length == 0) return true;
            foreach (var t in ValidTypes)
            {
                if (t == targetType) return true;
                if (t == typeof(Enum) && targetType.IsSubclassOf(typeof(Enum))) return true;
            }
            return false;
        }
    }
}