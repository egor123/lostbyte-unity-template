using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lostbyte.Toolkit.CustomEditor.Graphs
{
    public class GraphFieldAttribute : PropertyAttribute
    {
        public string Name;
        public GraphFieldAttribute() => Name = null;
        public GraphFieldAttribute(string name) => Name = name;
    }
}
