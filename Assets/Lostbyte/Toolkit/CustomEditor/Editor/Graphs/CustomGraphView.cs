using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Lostbyte.Toolkit.Common;
using Lostbyte.Toolkit.CustomEditor.Graphs;
using Core.CustomEditor.Editor;

namespace Lostbyte.Toolkit.CustomEditor.Editor.Graphs
{
    public abstract class CustomGraphView<TGraph, TAsset, TNodeView, TNodeBase> : GraphView
        where TGraph : CustomGraphView<TGraph, TAsset, TNodeView, TNodeBase>, new()
        where TAsset : ScriptableObject
        where TNodeView : CustomGraphNode<TGraph, TAsset, TNodeView, TNodeBase>
        where TNodeBase : ScriptableObject
    {
        public TAsset Asset { get; private set; }
        public Action OnGraphModified;
        private bool _isClearing;
        private bool _isLoading;

        private static Dictionary<Type, NodeInfo> _nodeTypes;
        protected EditorWindow _window;

        public void Initialize(EditorWindow window)
        {
            _window = window;
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            Insert(0, new GridBackground());

            InitStyles();
            InitMinimap();
            RegisterSearchWindow();

            graphViewChanged += OnGraphViewChanged;

            RegisterNodeChangeEvents();

            RegisterCallback<AttachToPanelEvent>(e => Undo.undoRedoPerformed += OnUndoRedo);
            RegisterCallback<DetachFromPanelEvent>(e => Undo.undoRedoPerformed -= OnUndoRedo);
        }

        protected virtual void InitMinimap()
        {
            var miniMap = new MiniMap { windowed = false };
            miniMap.style.position = Position.Absolute;
            miniMap.style.bottom = 20;
            miniMap.style.right = 20;
            miniMap.style.width = 200;
            miniMap.style.height = 140;
            miniMap.style.backgroundColor = new StyleColor(new Color(0.15f, 0.15f, 0.15f, 0.8f));
            Add(miniMap);
        }

        protected virtual void InitStyles()
        {
            if (EditorGUIUtility.Load("Assets/Lostbyte/Toolkit/CustomEditor/Editor/Graphs/GraphViewStyles.uss") is StyleSheet styleSheet)
                styleSheets.Add(styleSheet);
        }

        protected virtual void RegisterSearchWindow()
        {
            var searchWindow = ScriptableObject.CreateInstance<CustomNodeSearchWindow>();
            searchWindow.Initialize(_window, this, GetNodeInfos(), CreateNode);
            nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
        }

        private void RegisterNodeChangeEvents()
        {
            RegisterCallback<ChangeEvent<int>>(OnNodeFieldChanged);
            RegisterCallback<ChangeEvent<bool>>(OnNodeFieldChanged);
            RegisterCallback<ChangeEvent<float>>(OnNodeFieldChanged);
            RegisterCallback<ChangeEvent<string>>(OnNodeFieldChanged);
            RegisterCallback<ChangeEvent<Color>>(OnNodeFieldChanged);
            RegisterCallback<ChangeEvent<UnityEngine.Object>>(OnNodeFieldChanged);
            RegisterCallback<ChangeEvent<Enum>>(OnNodeFieldChanged);
            RegisterCallback<ChangeEvent<Vector2>>(OnNodeFieldChanged);
            RegisterCallback<ChangeEvent<Vector3>>(OnNodeFieldChanged);
            RegisterCallback<ChangeEvent<Vector4>>(OnNodeFieldChanged);
            RegisterCallback<ChangeEvent<Rect>>(OnNodeFieldChanged);
            RegisterCallback<ChangeEvent<AnimationCurve>>(OnNodeFieldChanged);
            RegisterCallback<ChangeEvent<Bounds>>(OnNodeFieldChanged);
            RegisterCallback<ChangeEvent<Gradient>>(OnNodeFieldChanged);
        }
        private void OnNodeFieldChanged<T>(ChangeEvent<T> evt)
        {
            if (_isLoading || _isClearing) return;
            if (evt.target is VisualElement targetElement)
            {
                if (targetElement is GraphElement) return;
                var parentNode = targetElement.GetFirstAncestorOfType<TNodeView>();
                if (parentNode != null) OnGraphModified?.Invoke();
            }
        }

        private void OnUndoRedo()
        {
            if (_isLoading || _isClearing) return;
            OnGraphModified?.Invoke();
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange changes)
        {
            if (_isClearing || _isLoading) return changes;
            bool modified = false;
            if (changes.elementsToRemove != null)
            {
                foreach (var element in changes.elementsToRemove)
                {
                    if (element is TNodeView nodeView && nodeView.Node != null)
                    {
                        Undo.DestroyObjectImmediate(nodeView.Node);
                        modified = true;
                    }
                }
            }
            if (changes.movedElements != null || changes.edgesToCreate != null)
                modified = true;

            if (modified) OnGraphModified?.Invoke();
            return changes;
        }
        public virtual void ClearGraph()
        {
            _isClearing = true;
            foreach (var nodeView in nodes.ToList().OfType<TNodeView>())
                if (nodeView.WorkingNode)
                    UnityEngine.Object.DestroyImmediate(nodeView.WorkingNode);
            DeleteElements(graphElements.ToList());
            _isClearing = false;
        }

        private static Dictionary<Type, NodeInfo> GetNodeTypeDict()
        {
            if (_nodeTypes != null) return _nodeTypes;

            var views = TypeCache.GetTypesDerivedFrom<TNodeView>()
                .Select(t => new { Type = t, Attr = t.GetCustomAttribute<NodeTypeAttribute>() })
                .Where(x => x.Attr != null)
                .ToDictionary(x => x.Attr.Type, x => new NodeInfo { Name = x.Attr.Name, NodeType = x.Attr.Type, ViewType = x.Type });

            var models = TypeCache.GetTypesDerivedFrom<TNodeBase>()
                .Select(t => new { Type = t, Attr = t.GetCustomAttribute<CustomGraphNodeAttribute>() })
                .Where(x => x.Attr != null && !views.ContainsKey(x.Type))
                .ToDictionary(x => x.Type, x => new NodeInfo { Name = x.Attr.Name, NodeType = x.Type, ViewType = typeof(TNodeView) });

            return _nodeTypes = views.Concat(models).ToDictionary(k => k.Key, v => v.Value);
        }

        public NodeInfo[] GetNodeInfos() => GetNodeTypeDict().Values.ToArray();

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.ToList().Where(port =>
                startPort != port &&
                startPort.node != port.node &&
                (startPort.portType == port.portType ||
                 startPort.portType == typeof(Unsafe) || port.portType == typeof(Unsafe) ||
                 (port.direction == Direction.Input && port.portType == typeof(object)) ||
                 (startPort.direction == Direction.Input && startPort.portType == typeof(object)))
            ).ToList();
        }

        public void Connect(Port outputPort, Port inputPort)
        {
            if (outputPort == null || inputPort == null) return;
            Edge edge = outputPort.ConnectTo(inputPort);
            AddElement(edge);
        }

        public void Disconnect(Port port)
        {
            if (port == null) return;
            var ports = port.connections.ToArray();
            foreach (var edge in ports)
            {
                edge.input?.Disconnect(edge);
                edge.output?.Disconnect(edge);
                RemoveElement(edge);
            }
        }

        public virtual void Load(TAsset asset)
        {
            _isLoading = true;
            ClearGraph();
            Asset = asset;
            if (asset == null) return;
            var loadedNodes = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(asset)).OfType<TNodeBase>();
            foreach (var node in loadedNodes)
            {
                var view = GetNodeView(node);
                view?.Load();
            }
            schedule.Execute(() => schedule.Execute(() => { _isLoading = false; }));
        }

        public virtual Vector2 GetDefaultNodeSize() => new(200, 150);

        public virtual TNodeView GetNodeView(TNodeBase node, Vector2? position = null)
        {
            if (node == null) return null;
            if (nodes.FirstOrDefault(v => v is TNodeView dv && dv.Node == node) is TNodeView view) return view;

            if (!GetNodeTypeDict().TryGetValue(node.GetType(), out var info)) return null;

            view = Activator.CreateInstance(info.ViewType, Asset, this, node) as TNodeView;
            if (view == null) return null;

            view.title = node.name;
            view.SetPosition(new Rect(position ?? view.LoadPosition(), GetDefaultNodeSize()));
            AddElement(view);
            return view;
        }

        public virtual void CreateNode(Type nodeType, string name, Vector2 position = default)
        {
            var newNode = ScriptableObject.CreateInstance(nodeType) as TNodeBase;
            newNode.name = name;
            Undo.RegisterCreatedObjectUndo(newNode, "Create Graph Node");
            GetNodeView(newNode, position);
            OnGraphModified?.Invoke();
        }

        public void Save(TAsset asset)
        {
            if (!asset) return;
            string assetPath = AssetDatabase.GetAssetPath(asset);
            try
            {
                var activeNodes = new HashSet<TNodeBase>();
                foreach (var nodeView in nodes.ToList().OfType<TNodeView>())
                {
                    if (nodeView == null) continue;
                    nodeView.Save();
                    nodeView.SavePosition(nodeView.GetPosition().position);
                    if (!nodeView.Node) continue;
                    nodeView.Node.name = nodeView.title;
                    activeNodes.Add(nodeView.Node);
                    if (!AssetDatabase.IsSubAsset(nodeView.Node))
                        AssetDatabase.AddObjectToAsset(nodeView.Node, asset);
                }
                var allSubAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
                foreach (var subAsset in allSubAssets)
                {
                    if (subAsset != asset && subAsset is TNodeBase nodeBase && !activeNodes.Contains(nodeBase))
                    {
                        AssetDatabase.RemoveObjectFromAsset(subAsset);
                        UnityEngine.Object.DestroyImmediate(subAsset, true);
                    }
                }
                EditorUtility.SetDirty(asset);
                AssetDatabase.SaveAssets();
            }
            catch (Exception ex)
            {
                Print.MError($"Failed to save graph to prevent data corruption. Error: {ex.Message}");
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