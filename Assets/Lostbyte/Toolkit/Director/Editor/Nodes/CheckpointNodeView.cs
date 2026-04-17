using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.CustomEditor.Editor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Lostbyte.Toolkit.Director.Editor
{
    [NodeType(typeof(CheckpointNode), "Checkpoint Node")]
    public class CheckpointNodeView : PlayableTrackNodeView
    {
        public Port OnContinuePort;
        private readonly EnumField _behaviourField;
        public CheckpointNodeView(PlayableTrack asset, PlayableTrackGraphView graph, PlayableTrackNode node) : base(asset, graph, node)
        {
            contentContainer.Add(_behaviourField = new("OnContinueBehaviour", CheckpointBehaviour.Continue));

            InputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(PlayableTrackNode));
            InputPort.portName = "In";
            inputContainer.Add(InputPort);

            OutputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(PlayableTrackNode));
            OutputPort.portName = "Out";
            outputContainer.Add(OutputPort);

            OnContinuePort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(PlayableTrackNode));
            OnContinuePort.portName = "OnContinue";
            outputContainer.Add(OnContinuePort);

            RefreshExpandedState();
            RefreshPorts();
        }

        public override void Load()
        {
            _behaviourField.value = (Node as CheckpointNode).CheckpointBehaviour;
            Graph.Connect(OutputPort, Graph.GetNodeView((Node as CheckpointNode).NextNode)?.InputPort);
            Graph.Connect(OnContinuePort, Graph.GetNodeView((Node as CheckpointNode).OnContinueNode)?.InputPort);
        }

        public override void Save()
        {
            (Node as CheckpointNode).CheckpointBehaviour = (CheckpointBehaviour)_behaviourField.value;
            (Node as CheckpointNode).NextNode = OutputPort.connected ? ((PlayableTrackNodeView)OutputPort.connections.First()?.input.node).Node : null;
            (Node as CheckpointNode).OnContinueNode = OnContinuePort.connected ? ((PlayableTrackNodeView)OnContinuePort.connections.First()?.input.node).Node : null;
        }
    }
}
