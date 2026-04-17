using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Lostbyte.Toolkit.Director.Editor
{
    public class StartNodeView : PlayableTrackNodeView
    {
        private readonly EnumField _priorityField, _behaviourField;
        public StartNodeView(PlayableTrack asset, PlayableTrackGraphView graph, PlayableTrackNode node) : base(asset, graph, node)
        {
            capabilities -= Capabilities.Deletable;
            contentContainer.Add(_priorityField = new("Priority", Priority.Default));
            contentContainer.Add(_behaviourField = new("OnContinue", OnContinueBehaviour.Schedule));


            TitleField.value = "Start";
            TitleField.isReadOnly = true;
            titleContainer.style.backgroundColor = Color.green;

            OutputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(PlayableTrackNode));
            OutputPort.portName = "Out";
            outputContainer.Add(OutputPort);

            RefreshExpandedState();
            RefreshPorts();
        }

        public override Vector2 LoadPosition() => Asset.StartNodePosition;
        public override void SavePosition(Vector2 position) => Asset.StartNodePosition = position;
        public override void Load()
        {
            _priorityField.value = Asset.Priority;
            _behaviourField.value = Asset.OnContinueBehaviour;
            if (Asset != null) Graph.Connect(OutputPort, Graph.GetNodeView(Asset.StartNode)?.InputPort);
        }

        public override void Save()
        {
            Asset.Priority = (Priority) _priorityField.value;
            Asset.OnContinueBehaviour = (OnContinueBehaviour)_behaviourField.value;
            Asset.StartNode = OutputPort.connected ? ((PlayableTrackNodeView)OutputPort.connections.First()?.input.node).Node : null;
        }
    }
}
