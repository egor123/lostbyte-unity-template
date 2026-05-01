using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Lostbyte.Toolkit.CustomEditor.Graphs;
using UnityEditor;
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

        public Tuple<FieldPath, Port> InputPort;
        public Dictionary<FieldPath, Port> OutputPorts = new();
        public Dictionary<FieldPath, BindableElement> DynamicFields = new();

        public CustomGraphNode(TAsset asset, TGraph graph, TNodeBase node)
        {
            (Asset, Graph, Node) = (asset, graph, node);
            TitleField = new TextField() { value = node != null ? node.name : nameof(TNodeBase) };
            TitleField.style.flexGrow = 1;
            TitleField.style.width = StyleKeyword.Auto;
            TitleField.RegisterValueChangedCallback(evt => { title = evt.newValue; });
            titleContainer.Clear();
            titleContainer.Add(TitleField);

            UpdateStyles();
            GenerateUI();
        }

        public virtual void UpdateStyles()
        {
            Color backgroundColor = new(.25f, .25f, .25f);
            Color borderColor = Color.gray;
            int borderRadius = 10;
            int borderWidth = 2;

            contentContainer.style.backgroundColor = new StyleColor(backgroundColor);
            contentContainer.style.borderBottomWidth = borderWidth;
            contentContainer.style.borderLeftWidth = borderWidth;
            contentContainer.style.borderRightWidth = borderWidth;
            contentContainer.style.borderTopWidth = borderWidth;

            contentContainer.style.borderBottomColor = new StyleColor(borderColor);
            contentContainer.style.borderTopColor = new StyleColor(borderColor);
            contentContainer.style.borderLeftColor = new StyleColor(borderColor);
            contentContainer.style.borderRightColor = new StyleColor(borderColor);

            contentContainer.style.borderTopLeftRadius = new StyleLength(borderRadius);
            contentContainer.style.borderTopRightRadius = new StyleLength(borderRadius);
            contentContainer.style.borderBottomLeftRadius = new StyleLength(borderRadius);
            contentContainer.style.borderBottomRightRadius = new StyleLength(borderRadius);
            contentContainer.style.paddingTop = 0;
            contentContainer.style.paddingLeft = 0;
            contentContainer.style.paddingRight = 0;
        }
        public virtual void GenerateUI()
        {
            if (Node == null) return;
            SerializedObject so = new(Node);
            Type type = Node.GetType();
            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (FieldInfo field in fields)
            {
                bool isCollection = typeof(IList).IsAssignableFrom(field.FieldType) && field.FieldType != typeof(string);

                var inAttr = field.GetCustomAttribute<GraphInAttribute>();
                if (inAttr != null)
                {
                    Port port = AddPort(field, inAttr.Name, Direction.Input);
                    InputPort = new(new(field), port);
                    inputContainer.Add(port);
                }

                var outAttr = field.GetCustomAttribute<GraphOutAttribute>();
                if (outAttr != null)
                {
                    Port port = AddPort(field, outAttr.Name, Direction.Output);
                    OutputPorts.Add(new(field), port);
                    outputContainer.Add(port);
                }

                if (Attribute.IsDefined(field, typeof(GraphFieldAttribute)))
                {
                    if (isCollection)
                    {
                        VisualElement listContainer = GenerateListUI(new(field));
                        contentContainer.Add(listContainer);
                    }
                    else
                    {
                        BindableElement element = AddField(field);
                        DynamicFields.Add(new(field), element);
                        contentContainer.Add(element);
                    }
                }
            }

            RefreshExpandedState();
            RefreshPorts();
        }
        private Port AddPort(FieldInfo field, string name, Direction dirrection, int elementIdx = -1)
        {
            bool isCollection = typeof(IList).IsAssignableFrom(field.FieldType) && field.FieldType != typeof(string);
            Port.Capacity capacity = isCollection ? Port.Capacity.Multi : Port.Capacity.Single;
            Type portType = isCollection && field.FieldType.IsGenericType
                ? field.FieldType.GetGenericArguments()[0]
                : (field.FieldType.IsArray ? field.FieldType.GetElementType() : field.FieldType);

            Port port = InstantiatePort(Orientation.Horizontal, dirrection, capacity, portType);
            port.portName = name ?? field.Name;
            return port;
        }
        private BindableElement AddField(FieldInfo field)
        {
            BindableElement propField = EditorUtils.CreatePropertyField(field.FieldType, field.Name);

            propField.Query<Label>().ForEach(label =>
            {
                label.style.minWidth = 0;
                label.style.width = StyleKeyword.Auto;
                label.style.marginRight = 5;
            });

            return propField;
        }

        private VisualElement GenerateListUI(FieldPath listField)
        {
            Type elementType = listField.FieldType.IsGenericType
                ? listField.FieldType.GetGenericArguments()[0]
                : listField.FieldType.GetElementType();

            Foldout foldout = new() { text = listField.Name };
            int idx = 0;

            Dictionary<Port, VisualElement> portOriginalContainers = new();
            Toggle toggle = foldout.Q<Toggle>();

            toggle.Add(new VisualElement { style = { flexGrow = 1 } });

            Port summaryPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, elementType);
            summaryPort.portName = "Out";
            summaryPort.SetEnabled(false);
            summaryPort.style.display = DisplayStyle.None;
            toggle.Add(summaryPort);

            VisualElement summaryConnector = summaryPort.Q("connector");

            foldout.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue)
                {
                    summaryPort.style.display = DisplayStyle.None;
                    var portsToMove = summaryConnector.Children().Where(c => c is Port).Cast<Port>().ToList();

                    foreach (var port in portsToMove)
                    {
                        if (portOriginalContainers.TryGetValue(port, out VisualElement originalContainer))
                        {
                            port.style.position = StyleKeyword.Null;
                            port.style.opacity = StyleKeyword.Null;
                            originalContainer.Add(port);
                        }
                    }
                }
                else
                {
                    summaryPort.style.display = DisplayStyle.Flex;
                    var portsToMove = foldout.contentContainer.Query<Port>().ToList();

                    foreach (var port in portsToMove)
                    {
                        port.style.position = Position.Absolute;
                        port.style.opacity = 0f;
                        summaryConnector.Add(port);
                    }
                }
            });

            if (Node != null)
            {
                object collectionObj = listField.GetValue(Node);
                if (collectionObj is IList existingList)
                {
                    for (int i = 0; i < existingList.Count; i++)
                    {
                        VisualElement existingRow = GenerateListItemRow(portOriginalContainers, elementType, listField, foldout, idx++);
                        foldout.Add(existingRow);
                    }
                }
            }

            Button addButton = new(() =>
            {
                VisualElement newRow = GenerateListItemRow(portOriginalContainers, elementType, listField, foldout, idx++);
                foldout.Insert(foldout.childCount - 1, newRow);
            })
            { text = "+ Add Item" };

            foldout.Add(addButton);

            return foldout;
        }
        private VisualElement GenerateListItemRow(Dictionary<Port, VisualElement> portOriginalContainers, Type elementType, FieldPath listPath, Foldout parentFoldout, int initialIndex)
        {
            FieldPath rowPath = listPath.WithIndex(initialIndex);
            FieldInfo[] fields = elementType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            VisualElement rowContainer = new();
            rowContainer.style.flexDirection = FlexDirection.Row;
            rowContainer.style.alignItems = Align.Center;
            rowContainer.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
            rowContainer.style.marginBottom = 5;

            VisualElement fieldsContainer = new();
            fieldsContainer.style.flexGrow = 1;

            VisualElement portsContainer = new();
            portsContainer.style.flexDirection = FlexDirection.Row;

            Button removeButton = new(() =>
            {
                foreach (var element in rowContainer.Query<BindableElement>().Build())
                {
                    if (element.userData is FieldPath fp) DynamicFields.Remove(fp);
                }

                foreach (var port in rowContainer.Query<Port>().Build())
                {
                    if (port.userData is FieldPath fp)
                    {
                        var edgesToDelete = port.connections.ToArray();
                        foreach (var edge in edgesToDelete)
                        {
                            edge.input?.Disconnect(edge);
                            edge.output?.Disconnect(edge);
                            Graph.RemoveElement(edge);
                        }

                        port.RemoveFromHierarchy();
                        portOriginalContainers.Remove(port);
                        OutputPorts.Remove(fp);
                    }
                }

                rowContainer.RemoveFromHierarchy();

                int currentIndex = 0;
                int listDepth = listPath.Depth - 1;

                foreach (var child in parentFoldout.Children())
                {
                    if (child is Button) continue;

                    VisualElement remainingRow = child;

                    foreach (var element in remainingRow.Query<BindableElement>().Build())
                    {
                        if (element.userData is FieldPath oldPath)
                        {
                            FieldPath newPath = oldPath.SetIndexAtDepth(listDepth, currentIndex);
                            if (DynamicFields.Remove(oldPath)) DynamicFields.Add(newPath, element);
                            element.userData = newPath;
                        }
                    }

                    foreach (var port in remainingRow.Query<Port>().Build())
                    {
                        if (port.userData is FieldPath oldPath)
                        {
                            FieldPath newPath = oldPath.SetIndexAtDepth(listDepth, currentIndex);
                            if (OutputPorts.Remove(oldPath)) OutputPorts.Add(newPath, port);
                            port.userData = newPath;
                        }
                    }
                    currentIndex++;
                }
            })
            { text = "x" };

            rowContainer.Add(removeButton);
            rowContainer.Add(fieldsContainer);
            rowContainer.Add(portsContainer);

            foreach (var field in fields)
            {
                if (Attribute.IsDefined(field, typeof(GraphFieldAttribute)))
                {
                    FieldPath newPath = rowPath.Append(field);
                    BindableElement element = AddField(field);
                    element.userData = newPath;

                    DynamicFields.Add(newPath, element);
                    fieldsContainer.Add(element);
                }
                var outAttr = field.GetCustomAttribute<GraphOutAttribute>();
                if (outAttr != null)
                {
                    FieldPath newPath = rowPath.Append(field);
                    Port port = AddPort(field, outAttr.Name, Direction.Output);
                    port.userData = newPath;

                    OutputPorts.Add(newPath, port);
                    portsContainer.Add(port);

                    portOriginalContainers[port] = portsContainer;
                }
            }

            return rowContainer;
        }
        public abstract Vector2 LoadPosition();
        public abstract void SavePosition(Vector2 position);
        public virtual void Load()
        {
            if (Node == null) return;
            foreach ((FieldPath field, BindableElement element) in DynamicFields)
            {
                PropertyInfo prop = element.GetType().GetProperty("value");
                prop?.SetValue(element, field.GetValue(Node));
            }
            foreach ((FieldPath field, Port port) in OutputPorts)
            {
                bool isCollection = typeof(IList).IsAssignableFrom(field.FieldType) && field.FieldType != typeof(string);
                object fieldValue = field.GetValue(Node);
                if (fieldValue == null) continue;
                if (isCollection)
                {
                    if (fieldValue is IList connectedNodes)
                    {
                        foreach (var connectedNode in connectedNodes)
                        {
                            if (connectedNode == null) continue;
                            TNodeView view = Graph.GetNodeView(connectedNode as TNodeBase);
                            Graph.Connect(port, view?.inputContainer.Q<Port>());
                        }
                    }
                }
                else
                {
                    TNodeView view = Graph.GetNodeView(fieldValue as TNodeBase);
                    Graph.Connect(port, view?.inputContainer.Q<Port>());
                    // if (view != null) Graph.Connect(port, view.InputPort.Item2);
                }
            }
        }
        public virtual void Save()
        {
            if (Node == null) return;
            ClearGraphCollections(Node);
            foreach ((FieldPath field, BindableElement element) in DynamicFields)
            {
                PropertyInfo prop = element.GetType().GetProperty("value");
                if (prop != null)
                {
                    object value = prop.GetValue(element);
                    field.SetValue(Node, value);
                }
            }
            foreach ((FieldPath field, Port port) in OutputPorts)
                SavePortConnection(field, port, Direction.Output);
            SavePortConnection(InputPort.Item1, InputPort.Item2, Direction.Input);
            EditorUtility.SetDirty(Node);
        }
        private void ClearGraphCollections(object target)
        {
            if (target == null) return;

            FieldInfo[] fields = target.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (FieldInfo field in fields)
            {
                if (Attribute.IsDefined(field, typeof(GraphFieldAttribute)) ||
                    Attribute.IsDefined(field, typeof(GraphOutAttribute)) ||
                    Attribute.IsDefined(field, typeof(GraphInAttribute)))
                {
                    bool isCollection = typeof(IList).IsAssignableFrom(field.FieldType) && field.FieldType != typeof(string);

                    if (isCollection)
                    {
                        field.SetValue(target, null);
                    }
                    else if (!field.FieldType.IsPrimitive && !field.FieldType.IsEnum && field.FieldType != typeof(string))
                    {
                        object child = field.GetValue(target);
                        if (child != null)
                        {
                            ClearGraphCollections(child);
                            if (field.FieldType.IsValueType)
                            {
                                field.SetValue(target, child);
                            }
                        }
                    }
                }
            }
        }
        private void SavePortConnection(FieldPath field, Port port, Direction direction)
        {

            bool isCollection = typeof(IList).IsAssignableFrom(field.FieldType) && field.FieldType != typeof(string);
            Type portType = isCollection && field.FieldType.IsGenericType
                ? field.FieldType.GetGenericArguments()[0]
                : (field.FieldType.IsArray ? field.FieldType.GetElementType() : field.FieldType);

            var connectedNodes = port.connections
                        .Select(e => (direction == Direction.Output ? e.input : e.output).node)
                        .OfType<TNodeView>()
                        .Select(n => n.Node)
                        .ToArray();

            if (isCollection)
            {
                if (field.FieldType.IsArray)
                {
                    field.SetValue(Node, connectedNodes);
                }
                else
                {
                    var listType = typeof(List<>).MakeGenericType(portType);
                    IList listInstance = (IList)Activator.CreateInstance(listType);
                    foreach (var node in connectedNodes)
                    {
                        listInstance.Add(node);
                    }
                    field.SetValue(Node, listInstance);
                }
            }
            else
            {
                field.SetValue(Node, connectedNodes.FirstOrDefault());
            }
        }
    }
}
