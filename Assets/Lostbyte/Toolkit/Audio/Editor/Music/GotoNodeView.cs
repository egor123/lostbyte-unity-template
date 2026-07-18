using System.Collections.Generic;
using System.Linq;
using Core.CustomEditor.Editor;
using Lostbyte.Toolkit.Audio.Music;
using Lostbyte.Toolkit.Common;
using UnityEditor;
using UnityEngine.UIElements;

namespace Lostbyte.Toolkit.Audio.Editor
{
    [NodeType(typeof(GotoNode), "Logic/GOTO")]
    public class GotoNodeView : MusicTrackNodeView
    {
        private DropdownField _dropdown;
        private List<(MusicTrackNodeView view, string name)> _nodes = new();
        public GotoNodeView(MusicTrackData asset, MusicTrackGraphView graph, MusicTrackNode node) : base(asset, graph, node) { }

        public override void GenerateUI()
        {
            base.GenerateUI();
            _dropdown = new DropdownField();
            _dropdown.RegisterCallback<PointerDownEvent>(RefreshDropdownChoices, TrickleDown.TrickleDown);
            contentContainer.Add(_dropdown);
        }

        private void RefreshDropdownChoices(PointerDownEvent evt)
        {
            if (Graph == null || Graph.nodes == null) return;
            _nodes = Graph.nodes.Cast<MusicTrackNodeView>()
                .Where(n => n != this && n.Node != null)
                .WhereNotNull()
                .Select(n => (n, n.Name))
                .OrderBy(n => n.Name)
                .ToList();
            var availableNodeNames = _nodes.Select(n => n.name).ToList();
            if (!availableNodeNames.Contains(string.Empty))
                availableNodeNames.Insert(0, string.Empty);
            _dropdown.choices = availableNodeNames;
        }

        public override void Load()
        {
            base.Load();
            _dropdown.value = (Node as GotoNode).Next != null ? (Node as GotoNode).Next.name : null;
        }

        public override void Save()
        {
            base.Save();
            RefreshDropdownChoices(null);
            int idx = _dropdown.index - 1;
            if (idx >= 0 && idx < _nodes.Count)
                (Node as GotoNode).Next = _nodes[_dropdown.index - 1].view.Node;
            else
                (Node as GotoNode).Next = null;
            EditorUtility.SetDirty(Node);
        }
    }
}