using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lostbyte.Toolkit.CustomEditor.Graphs
{
    public class GraphOutAttribute : PropertyAttribute
    {
        public string Name;

        public GraphOutAttribute(string name)
        {
            Name = name;
        }
        public GraphOutAttribute()
        {
            Name = null;
        }
    }
}
