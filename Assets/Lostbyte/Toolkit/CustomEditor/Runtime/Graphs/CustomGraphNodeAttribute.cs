using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lostbyte.Toolkit.CustomEditor.Graphs
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CustomGraphNodeAttribute : PropertyAttribute
    {
        public string Name;
        public CustomGraphNodeAttribute(string name)
        {
            Name = name;
        }
    }
}
