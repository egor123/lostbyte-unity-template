using System;
using System.Collections.Generic;
using System.Linq;
using Core.CustomEditor.Editor;
using Lostbyte.Toolkit.Common;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace Lostbyte.Toolkit.Director.Editor
{
    [NodeType(typeof(RandomNode), "Random Node")]
    public class RandomNodeView : PlayableTrackNodeView
    {
        private readonly List<Tuple<FloatField, Port>> _nodes = new();
        public RandomNodeView(PlayableTrack asset, PlayableTrackGraphView graph, PlayableTrackNode node) : base(asset, graph, node)
        {
            InputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(PlayableTrackNode));
            InputPort.portName = "In";
            inputContainer.Add(InputPort);
            outputContainer.Add(new Button(() => AddElement()) { text = "Add" });
            RefreshExpandedState();
            RefreshPorts();
        }

        private void AddElement(PlayableTrackNode node = null, float value = 1)
        {
            var element = new VisualElement() { style = { flexDirection = FlexDirection.Row } };
            var weightField = new FloatField("Weight") { value = value };
            var port = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(PlayableTrackNode));
            port.portName = "Out";
            var tuple = new Tuple<FloatField, Port>(weightField, port);
            _nodes.Add(tuple);
            element.Add(new Button(() => { outputContainer.Remove(element); _nodes.Remove(tuple); }) { text = "Remove" });
            element.Add(weightField);
            element.Add(port);
            outputContainer.Add(element);
            Graph.Connect(port, Graph.GetNodeView(node)?.InputPort);
        }

        public override void Load()
        {
            (Node as RandomNode).WeightedNodes.ForEach(node => AddElement(node.Item2, node.Item1));
        }

        public override void Save()
        {
            (Node as RandomNode).WeightedNodes = _nodes.Select(n => new SerializedTuple<float, PlayableTrackNode>(n.Item1.value, n.Item2.connected ? (n.Item2.connections.First()?.input.node as PlayableTrackNodeView).Node : null)).ToList();
        }
    }
}
