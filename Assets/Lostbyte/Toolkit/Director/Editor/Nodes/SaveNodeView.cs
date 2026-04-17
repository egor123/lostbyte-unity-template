using System.Linq;
using Core.CustomEditor.Editor;
using UnityEditor.Experimental.GraphView;

namespace Lostbyte.Toolkit.Director.Editor
{
    [NodeType(typeof(SaveNode), "Game Save Node")]
    public class SaveNodeView : PlayableTrackNodeView
    {        public SaveNodeView(PlayableTrack asset, PlayableTrackGraphView graph, PlayableTrackNode node) : base(asset, graph, node)
        {
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
            Graph.Connect(OutputPort, Graph.GetNodeView((Node as SaveNode).NextNode)?.InputPort);
        }

        public override void Save()
        {
            (Node as SaveNode).NextNode = OutputPort.connected ? ((PlayableTrackNodeView)OutputPort.connections.First()?.input.node).Node : null;
        }
    }
}
