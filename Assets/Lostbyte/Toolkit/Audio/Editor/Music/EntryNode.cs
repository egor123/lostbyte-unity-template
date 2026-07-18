using System.Linq;
using Lostbyte.Toolkit.Audio.Music;
using Lostbyte.Toolkit.CustomEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Lostbyte.Toolkit.Audio.Editor
{
    public class EntryNode : MusicTrackNodeView
    {
        private Port OutputPort;
        public EntryNode(MusicTrackData asset, MusicTrackGraphView graph, MusicTrackNode node) : base(asset, graph, node)
        {
            capabilities -= Capabilities.Deletable;
            userData = asset;
        }
        public override void GenerateUI()
        {
            TitleField.value = "Start";
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
            var borderColor = new Color(0.24f, 0.52f, 0.34f);
            contentContainer.SetBorderColor(borderColor);
        }
        public override Vector2 LoadPosition() => Asset.EntryNodePosition;
        public override void SavePosition(Vector2 position) => Asset.EntryNodePosition = position;
        public override void Load()
        {
            if (Asset != null && Asset.EntrySegments != null)
            {
                foreach (var segment in Asset.EntrySegments)
                {
                    Graph.Connect(OutputPort, Graph.GetNodeView(segment)?.inputContainer.Q<Port>());
                }
            }
        }

        public override void Save()
        {
            Asset.EntrySegments = OutputPort.connections?.Select(n => ((MusicTrackNodeView)n.input.node).Node).ToArray();
        }
    }
}
