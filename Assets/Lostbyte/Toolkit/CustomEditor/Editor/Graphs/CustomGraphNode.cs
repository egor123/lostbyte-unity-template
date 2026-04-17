using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Lostbyte.Toolkit.CustomEditor.Editor.Graphs
{
    public abstract class CustomGraphNode<TGraph, TAsset, TNodeView, TNodeBase> : Node
        where TGraph : CustomGraphView<TGraph, TAsset, TNodeView, TNodeBase>, new()
        where TAsset : ScriptableObject
        where TNodeView : CustomGraphNode<TGraph, TAsset, TNodeView, TNodeBase>
        where TNodeBase : ScriptableObject
    {
        public TNodeBase Node { get; protected set; }
        public TGraph Graph { get; protected set; }
        public TAsset Asset { get; protected set; }
        public TextField TitleField;
        public CustomGraphNode(TAsset asset, TGraph graph, TNodeBase node)
        {
            (Asset, Graph, Node) = (asset, graph, node);
            TitleField = new TextField() { value = node != null ? node.name : nameof(TNodeBase) };
            TitleField.style.flexGrow = 1;
            TitleField.style.width = StyleKeyword.Auto;
            TitleField.RegisterValueChangedCallback(evt => { title = evt.newValue; });
            titleContainer.Clear();
            titleContainer.Add(TitleField);
        }
        public abstract Vector2 LoadPosition();
        public abstract void SavePosition(Vector2 position);
        public abstract void Load();
        public abstract void Save();
    }
}
