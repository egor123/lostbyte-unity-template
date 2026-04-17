using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Core.CustomEditor.Editor;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Lostbyte.Toolkit.CustomEditor.Editor.Graphs
{
    public abstract class CustomGraphView<TGraph, TAsset, TNodeView, TNodeBase> : GraphView
        where TGraph : CustomGraphView<TGraph, TAsset, TNodeView, TNodeBase>, new()
        where TAsset : ScriptableObject
        where TNodeView : CustomGraphNode<TGraph, TAsset, TNodeView, TNodeBase>
        where TNodeBase : ScriptableObject
    {
        public TAsset Asset { get; private set; }
        private static Dictionary<Type, NodeInfo> _nodeTypes;

        public CustomGraphView()
        {
            OnGraphInit();
        }

        protected virtual void OnGraphInit()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();
            RegisterSearchWindow();
        }
        protected virtual void RegisterSearchWindow()
        {
            CustomNodeSearchWindow searchWindow = ScriptableObject.CreateInstance<CustomNodeSearchWindow>();
            searchWindow.Initialize(GetNodeInfos(), CreateNode);
            nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
        }

        protected virtual void ClearGraph()
        {
            graphElements.ForEach(RemoveElement);
        }

        private static Dictionary<Type, NodeInfo> GetNodeTypeDict() => _nodeTypes ??= UniqeReferenceAttribute
            .GetSubClasses(typeof(TNodeView))
            .Where(t => t.GetCustomAttribute<NodeTypeAttribute>() != null)
            .ToDictionary(t => t.GetCustomAttribute<NodeTypeAttribute>().Type, t => new NodeInfo()
            {
                Name = t.GetCustomAttribute<NodeTypeAttribute>().Name,
                NodeType = t.GetCustomAttribute<NodeTypeAttribute>().Type,
                ViewType = t
            });
        public NodeInfo[] GetNodeInfos() => GetNodeTypeDict().Values.ToArray();



        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new();
            foreach (var port in ports)
                if (startPort != port && startPort.node != port.node)
                    if (startPort.portType == port.portType 
                        || startPort.portType == typeof(Unsafe) || port.portType == typeof(Unsafe)
                        || (port.direction == Direction.Input && port.portType == typeof(object)) || (startPort.direction == Direction.Input && startPort.portType == typeof(object)))
                        compatiblePorts.Add(port);
            return compatiblePorts;
        }

        public virtual void Load(TAsset asset)
        {
            ClearGraph();
            Asset = asset;
            if (asset == null) return;
            List<TNodeView> nodes = new();
            var loadedNodes = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(asset)).OfType<TNodeBase>();
            foreach (var node in loadedNodes)
            {
                try { nodes.Add(GetNodeView(node)); }
                catch (Exception exception) { Debug.LogError(exception); }
            }
            foreach (var node in nodes)
            {
                try { node.Load(); }
                catch (Exception exception) { Debug.LogError(exception); }
            }
        }
        public virtual Vector2 GetDefaultNodeSize() => new(200, 150);

        public virtual TNodeView GetNodeView(TNodeBase node)
        {
            if (node == null) return null;
            if (nodes.FirstOrDefault(v => v is TNodeView dv && dv.Node == node) is TNodeView view && view != null) return view;
            if (!GetNodeTypeDict().TryGetValue(node.GetType(), out var info)) return null;
            view = Activator.CreateInstance(info.ViewType, new object[] { Asset, this, node }) as TNodeView;
            view.title = node.name;
            view.SetPosition(new Rect(view.LoadPosition(), GetDefaultNodeSize()));
            AddElement(view);
            return view;
        }
        public virtual void CreateNode(Type nodeType, string name, Vector2 position = default)
        {
            TNodeBase newNode = ScriptableObject.CreateInstance(nodeType) as TNodeBase;
            newNode.name = name;
            GetNodeView(newNode);
        }
        public void Connect(Port outputPort, Port inputPort)
        {
            if (outputPort == null || inputPort == null) return;
            AddElement(outputPort.ConnectTo(inputPort));
        }
        public void Save(TAsset asset)
        {
            if (asset == null) return;
            string assetPath = AssetDatabase.GetAssetPath(asset);
            UnityEngine.Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            foreach (var subAsset in subAssets)
            {
                if (subAsset != Asset)
                {
                    AssetDatabase.RemoveObjectFromAsset(subAsset);
                }
            }
            SaveAsset(asset);
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            Load(asset); //FIXME DLETEME???
        }
        protected virtual void SaveAsset(TAsset asset)
        {
            foreach (var node in nodes)
            {
                try
                {
                    if (node is TNodeView tNode && tNode != null)
                    {
                        tNode.SavePosition(tNode.GetPosition().position);
                        tNode.Save();
                        if (tNode.Node != null)
                        {
                            tNode.Node.name = tNode.title;
                            AssetDatabase.AddObjectToAsset(tNode.Node, asset);
                        }
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogError(exception);
                }

            }
        }
    }
    public struct NodeInfo
    {
        public string Name;
        public Type NodeType;
        public Type ViewType;
    }
    public class Unsafe { }
}
