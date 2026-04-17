using System;
using System.Collections;
using System.Collections.Generic;
using Lostbyte.Toolkit.CustomEditor;
using UnityEngine;

namespace Lostbyte.Toolkit.Graphs
{
    [Serializable]
    public struct GraphEdge
    {
        public bool IsDirected;
        public GraphNode NodeA;
        public GraphNode NodeB;
        [SerializeReference, UniqeReference] public List<IDistanceOverride> Overrides;

        public interface IDistanceOverride
        {
            float Override(float distance);
        }
        public struct FixedDistanceOverride : IDistanceOverride
        {
            public float Distance;
            public readonly float Override(float distance) => Distance;
        }

        public struct MultiplyerOverride : IDistanceOverride
        {
            public float Multiplyer;
            public readonly float Override(float distance) => distance * Multiplyer;
        }

        private bool _initiated;
        private bool _static;
        private float _distance;

        public float Distance
        {
            get
            {
                if (_initiated)
                {
                    _initiated = true;
                    _static = NodeA.gameObject.isStatic && NodeB.gameObject.isStatic;
                    _distance = Vector3.Distance(NodeA.transform.position, NodeB.transform.position);
                }
                if (!_static)
                    _distance = Vector3.Distance(NodeA.transform.position, NodeB.transform.position);
                float d = _distance;
                foreach (var o in Overrides)
                    if (o != null)
                        d = o.Override(d);
                return d;
            }
        }
        public override readonly int GetHashCode()
        {
            int e = IsDirected.GetHashCode();
            int a = NodeA.GetHashCode();
            int b = NodeB.GetHashCode();
            if (!IsDirected)
                return HashCode.Combine(e, a, b);
            if (a.CompareTo(b) > 0)
                return HashCode.Combine(e, a, b);
            return HashCode.Combine(e, b, a);
        }
        public override readonly bool Equals(object obj)
        {
            if (obj is GraphEdge edge)
            {
                if (IsDirected != edge.IsDirected)
                    return false;
                if (IsDirected)
                    return NodeA == edge.NodeA && NodeB == edge.NodeB;
                else
                    return (NodeA == edge.NodeA && NodeB == edge.NodeB) || (NodeA == edge.NodeB && NodeB == edge.NodeA);
            }
            return false;
        }
    }
}
