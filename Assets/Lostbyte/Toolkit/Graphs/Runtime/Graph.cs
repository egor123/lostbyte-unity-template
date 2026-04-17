using System.Collections;
using System.Collections.Generic;
using Lostbyte.Toolkit.CustomEditor;
using UnityEngine;
using UnityEngine.Video;

namespace Lostbyte.Toolkit.Graphs
{
    public class Graph : MonoBehaviour
    {
        [field: SerializeField] public UpdateType UpdateMode { get; private set; } = UpdateType.FixedUpdate;
        [field: SerializeField, EditModeOnly] public LayerMask IncludeLayers { get; private set; } = 0;
        [SerializeField] private LayerMask m_debugMask = 0;
        [SerializeField] private List<GraphNode> m_nodes = new();
        [SerializeField] private List<GraphEdge> m_edges = new();

        private readonly Dictionary<GraphNode, LayerMask> _graphMask = new();
        private readonly Dictionary<GraphNode, int>[] _subGraphs;
        private readonly int[] _currentIndexes = new int[32];
        private readonly Dictionary<int, int> _layerToMaskIdxMap = new();
        private readonly List<IBlocking> _blocks = new();
        private LayerMask _dirtyMask;



        public enum UpdateType { ScriptOnly, Update, FixedUpdate }

        public void Register(IBlocking block) => _blocks.Add(block);
        public void Unregister(IBlocking block) => _blocks.Remove(block);

        public bool NodeIsAllowed(GraphNode node, LayerMask mask) => _graphMask.TryGetValue(node, out var m) && (m & mask) == mask;
        public void SetDirty(LayerMask mask) => _dirtyMask |= mask;

        public void Recalculate()
        {
            if (_dirtyMask == 0) return;
            foreach (var node in _graphMask.Keys) _graphMask[node] = 0;
            foreach (var block in _blocks)
            {
                if (block.Enabled)
                {
                    foreach (var node in block.Nodes)
                    {
                        _graphMask[node] |= block.Mask;
                    }
                }
            }

            _dirtyMask = 0;
        }



        private void Awake()
        {

            // for (int i = 0; i < 32; i++)
            // {
            //     if (IncludeLayers == (IncludeLayers | (1 << i)))
            //     {
            //         // hasLayers *= true;
            //     }
            // }
        }

        private void Update()
        {
            if (UpdateMode == UpdateType.Update) Recalculate();
        }

        private void FixedUpdate()
        {
            if (UpdateMode == UpdateType.FixedUpdate) Recalculate();
        }

        private void UpdateIdxs()
        {

        }





#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = new(1, 1, 1, 0.2f);
            foreach (var node in m_nodes)
            {
                // Gizmos.DrawSphere(node.transform.position, 0.3f);
                UnityEditor.Handles.Label(node.transform.position, node.name);
                node.transform.position = UnityEditor.Handles.FreeMoveHandle(node.transform.position, 0.2f, Vector3.zero, UnityEditor.Handles.RectangleHandleCap);
            }
            foreach (var edge in m_edges)
            {
                if (edge.NodeA && edge.NodeB)
                {
                    Gizmos.DrawLine(edge.NodeA.transform.position, edge.NodeB.transform.position);
                    UnityEditor.Handles.Label(Vector3.Lerp(edge.NodeA.transform.position, edge.NodeB.transform.position, 0.5f), edge.Distance.ToString("#0.00"));
                    if (edge.IsDirected)
                    {
                        var a = edge.NodeA.transform.position;
                        var b = edge.NodeB.transform.position;
                        var origin = Vector3.Lerp(a, b, 0.8f);
                        var o = Vector3.MoveTowards(origin, a, 1f);
                        var left = Vector3.Cross(Vector3.up, b - a).normalized * 0.5f;
                        Gizmos.DrawLine(origin, o + left);
                        Gizmos.DrawLine(origin, o - left);
                    }
                }
            }
        }
#endif
    }
}