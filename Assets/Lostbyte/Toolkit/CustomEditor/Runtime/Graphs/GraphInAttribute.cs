using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lostbyte.Toolkit.CustomEditor.Graphs
{
    public class GraphInAttribute : PropertyAttribute
    {
        public string Name;

        public GraphInAttribute(string name)
        {
            Name = name;
        }
        public GraphInAttribute()
        {
            Name = null;
        }
    }
}
