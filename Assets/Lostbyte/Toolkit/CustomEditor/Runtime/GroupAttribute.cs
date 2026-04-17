using System;

namespace Lostbyte.Toolkit.CustomEditor
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class GroupAttribute : Attribute
    {
        public string GroupName;
        public bool LookNext;

        public GroupAttribute(string groupName, bool lookNext = false)
        {
            GroupName = groupName;
            LookNext = lookNext;
        }
    }
}