using System.Linq;
using Lostbyte.Toolkit.Audio.Music;
using Lostbyte.Toolkit.CustomEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Lostbyte.Toolkit.Audio.Editor
{
    public class ExitNode : MusicTrackNodeView
    {
        private Port OutputPort;
        public ExitNode(MusicTrackData asset, MusicTrackGraphView graph, MusicTrackNode node) : base(asset, graph, node)
        {
            capabilities -= Capabilities.Deletable;
            userData = asset;
        }
        public override void GenerateUI()
        {
            TitleField.value = "Exit";
            TitleField.isReadOnly = true;

            OutputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(MusicTrackNode));
            OutputPort.portName = "Out";
            outputContainer.Add(OutputPort);

            RefreshExpandedState();
            RefreshPorts();
        }
        public override void UpdateStyles()
        {
            base.UpdateStyles();
            var borderColor = new Color(0.7f, 0.5f, 0.2f);
            contentContainer.SetBorderColor(borderColor);
        }
        public override Vector2 LoadPosition() => Asset.ExitNodePosition;
        public override void SavePosition(Vector2 position) => Asset.ExitNodePosition = position;
        public override void Load()
        {
            if (Asset != null && Asset.ExitSegments != null)
            {
                foreach (var segment in Asset.ExitSegments)
                {
                    Graph.Connect(OutputPort, Graph.GetNodeView(segment)?.inputContainer.Q<Port>());
                }
            }
        }

        public override void Save()
        {
            Asset.ExitSegments = OutputPort.connections?.Select(n => ((MusicTrackNodeView)n.input.node).Node).ToArray();
        }
    }
}
