using System.Collections;
using System.Collections.Generic;
using Lostbyte.Toolkit.CustomEditor;
using UnityEngine;

namespace Lostbyte.Toolkit.Graphs
{
    public interface IBlocking
    {
        GraphNode[] Nodes { get; }
        bool Enabled { get; }
        LayerMask Mask { get; }
    }
    public class GraphBlock : MonoBehaviour, IBlocking
    {
        [field: SerializeField, Autowired(Autowired.Type.Parent), EditModeOnly] public Graph Graph { get; private set; }
        [field: SerializeField, Autowired, EditModeOnly] public GraphNode[] Nodes { get; private set; }
        [field: SerializeField] public LayerMask Mask { get; private set; } = 0; // FIXME
        [SerializeField] private bool _enabled;
        public bool Enabled
        {
            get => enabled; set
            {
                if (_enabled == value) return;
                _enabled = value;
                if (Graph) Graph.SetDirty(Mask);
            }
        } // FIXME

        private void Awake()
        {
            if (Graph) Graph.Register(this);
        }

        private void OnDestroy()
        {
            if (Graph) Graph.Unregister(this);
        }
    }
}
