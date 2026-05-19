using System;
using System.Collections.Generic;
using System.Linq;
using Lostbyte.Toolkit.Common;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Lostbyte.Toolkit.CustomEditor.Editor.Graphs
{
    public class PortField : Port
    {
        private SerializedObject m_SerializedObject;
        private string m_PropertyPath;
        private bool m_IsSyncing;

        public SerializedProperty Property => m_SerializedObject?.FindProperty(m_PropertyPath);

        protected PortField(Orientation orientation, Direction direction, Capacity capacity, Type type)
            : base(orientation, direction, capacity, type) { }



        public static PortField Create(SerializedProperty property, string label, Type type, Direction direction)
        {
            var port = new PortField(Orientation.Horizontal, direction, property.isArray ? Capacity.Multi : Capacity.Single, type)
            {
                m_SerializedObject = property.serializedObject,
                m_PropertyPath = property.propertyPath,
                portName = label,
                m_EdgeConnector = new EdgeConnector<Edge>(new SimpleEdgeListener())
            };
            port.AddManipulator(port.m_EdgeConnector);
            void syncAction() => port.schedule.Execute(port.SyncConnections);
            port.TrackPropertyValue(property, _ => syncAction());
            syncAction();
            return port;
        }

        public override void Connect(Edge edge) { base.Connect(edge); SaveConnections(); }
        public override void Disconnect(Edge edge) { base.Disconnect(edge); SaveConnections(); }
        public override void DisconnectAll() { CleanupEdges(); base.DisconnectAll(); SaveConnections(); }

        public void CleanupEdges()
        {
            if (connections == null || !connections.Any()) return;
            var graph = GetFirstAncestorOfType<GraphView>();
            if (graph == null) return;
            foreach (var edge in connections.ToList())
            {
                if (edge.input != this)
                    edge.input?.Disconnect(edge);
                if (edge.output != this)
                    edge.output?.Disconnect(edge);
                graph.RemoveElement(edge);
            }
        }
        private void SaveConnections()
        {
            if (m_IsSyncing || Property == null) return;
            var targets = connections
                .Select(e => (direction == Direction.Input ? e.output : e.input)?.node?.userData as UnityEngine.Object)
                .Where(obj => obj != null)
                .ToList();

            if (Property.isArray)
            {
                Property.arraySize = targets.Count;
                for (int i = 0; i < targets.Count; i++)
                    Property.GetArrayElementAtIndex(i).objectReferenceValue = targets[i];
            }
            else
            {
                Property.objectReferenceValue = targets.FirstOrDefault();
            }
            Property.serializedObject.ApplyModifiedProperties();
        }

        private void SyncConnections()
        {
            if (direction == Direction.Input || Property == null || GetFirstAncestorOfType<GraphView>() is not { } graph)
                return;
            m_IsSyncing = true;
            try
            {
                foreach (var edge in connections.ToList())
                {
                    edge.input?.Disconnect(edge);
                    edge.output?.Disconnect(edge);
                    graph.RemoveElement(edge);
                }
                var targets = Property.isArray
                    ? Enumerable.Range(0, Property.arraySize).Select(i => Property.GetArrayElementAtIndex(i).objectReferenceValue)
                    : new[] { Property.objectReferenceValue };

                foreach (var target in targets.Where(t => t != null))
                {
                    var targetPort = graph.nodes.ToList().FirstOrDefault(n => n.userData?.Equals(target) == true)?
                        .Query<Port>().Where(p => p.direction != direction).Build().FirstOrDefault();

                    if (targetPort == null) continue;

                    var edge = new Edge { output = this, input = targetPort };
                    edge.input.Connect(edge);
                    edge.output.Connect(edge);
                    graph.AddElement(edge);
                }
            }
            finally
            {
                m_IsSyncing = false;
            }
        }

        private class SimpleEdgeListener : IEdgeConnectorListener
        {
            public void OnDropOutsidePort(Edge edge, Vector2 position) { }

            public void OnDrop(GraphView graphView, Edge edge)
            {
                var elementsToDelete = new List<GraphElement>();

                if (edge.input.capacity == Capacity.Single)
                    elementsToDelete.AddRange(edge.input.connections.Where(c => c != edge));
                if (edge.output.capacity == Capacity.Single)
                    elementsToDelete.AddRange(edge.output.connections.Where(c => c != edge));

                if (elementsToDelete.Count > 0)
                    graphView.DeleteElements(elementsToDelete);

                graphView.AddElement(edge);
                edge.input.Connect(edge);
                edge.output.Connect(edge);
            }
        }
    }
}