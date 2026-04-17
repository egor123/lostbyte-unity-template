using System;
using System.Collections.Generic;
using System.Linq;
using Core.CustomEditor.Editor;
using Lostbyte.Toolkit.Common;
using Lostbyte.Toolkit.FactSystem;
using Lostbyte.Toolkit.FactSystem.Editor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace Lostbyte.Toolkit.Director.Editor
{
    [NodeType(typeof(SwitchNode), "Switch Node")]
    public class SwitchNodeView : PlayableTrackNodeView
    {
        private readonly List<Tuple<ConditionField, Port>> _nodes = new();
        public SwitchNodeView(PlayableTrack asset, PlayableTrackGraphView graph, PlayableTrackNode node) : base(asset, graph, node)
        {
            InputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(PlayableTrackNode));
            InputPort.portName = "In";
            inputContainer.Add(InputPort);
            outputContainer.Add(new Button(() => AddElement()) { text = "Add" });
            RefreshExpandedState();
            RefreshPorts();
        }
        private void AddElement(PlayableTrackNode node = null, Condition value = null)
        {
            var element = new VisualElement() { style = { flexDirection = FlexDirection.Row } };
            var conditionField = new ConditionField() { Value = value };
            conditionField.Label.style.display = DisplayStyle.None;
            var port = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(PlayableTrackNode));
            port.portName = "Out";
            var tuple = new Tuple<ConditionField, Port>(conditionField, port);
            _nodes.Add(tuple);
            element.Add(new Button(() => { outputContainer.Remove(element); _nodes.Remove(tuple); }) { text = "Remove"});
            element.Add(conditionField);
            element.Add(port);
            outputContainer.Add(element);
            Graph.Connect(port, Graph.GetNodeView(node)?.InputPort);
        }


        public override void Load()
        {
            (Node as SwitchNode).Nodes.ForEach(node => AddElement(node.Item2, node.Item1));
        }
        public override void Save()
        {
            (Node as SwitchNode).Nodes = _nodes.Select(n => new SerializedTuple<Condition, PlayableTrackNode>(n.Item1.Value, n.Item2.connected ? (n.Item2.connections.First()?.input.node as PlayableTrackNodeView).Node : null)).ToList();
        }
    }
}