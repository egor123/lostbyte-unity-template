using System.Linq;
using Core.CustomEditor.Editor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace Lostbyte.Toolkit.Director.Editor
{
    [NodeType(typeof(WaitNode), "Wait Node")]
    public class WaitNodeView : PlayableTrackNodeView
    {
        private readonly FloatField _floatField;
        public WaitNodeView(PlayableTrack asset, PlayableTrackGraphView graph, PlayableTrackNode node) : base(asset, graph, node)
        {
            contentContainer.Add(_floatField = new());
            style.width = 100;

            InputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(PlayableTrackNode));
            InputPort.portName = "In";
            inputContainer.Add(InputPort);

            OutputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(PlayableTrackNode));
            OutputPort.portName = "Out";
            outputContainer.Add(OutputPort);

            RefreshExpandedState();
            RefreshPorts();
        }
        public override void Load()
        {
            _floatField.value = (Node as WaitNode).Time;
            Graph.Connect(OutputPort, Graph.GetNodeView((Node as WaitNode).NextNode)?.InputPort);
        }
        public override void Save()
        {
            (Node as WaitNode).Time = _floatField.value;
            (Node as WaitNode).NextNode = OutputPort.connected ? ((PlayableTrackNodeView)OutputPort.connections.First()?.input.node).Node : null;
        }
    }
}
