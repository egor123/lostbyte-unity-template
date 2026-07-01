using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Lostbyte.Toolkit.Common;
using Lostbyte.Toolkit.CustomEditor.Graphs;

namespace Lostbyte.Toolkit.CustomEditor.Editor.Graphs
{
    public abstract class CustomGraphNode<TGraph, TAsset, TNodeView, TNodeBase> : Node
        where TGraph : CustomGraphView<TGraph, TAsset, TNodeView, TNodeBase>, new()
        where TAsset : ScriptableObject
        where TNodeView : CustomGraphNode<TGraph, TAsset, TNodeView, TNodeBase>
        where TNodeBase : ScriptableObject
    {
        public TNodeBase Node { get; protected set; }
        public TNodeBase WorkingNode { get; protected set; }
        public SerializedObject WorkingSO { get; protected set; }

        public TGraph Graph { get; protected set; }
        public TAsset Asset { get; protected set; }
        public TextField TitleField;

        public CustomGraphNode(TAsset asset, TGraph graph, TNodeBase node)
        {
            (Asset, Graph, Node, userData) = (asset, graph, node, node);

            TitleField = new TextField { value = node?.name ?? nameof(TNodeBase), style = { flexGrow = 1, width = StyleKeyword.Auto } };
            TitleField.RegisterValueChangedCallback(e => title = e.newValue);

            titleContainer.Clear();
            titleContainer.Add(TitleField);

            if (Node != null)
            {
                WorkingNode = UnityEngine.Object.Instantiate(Node);
                WorkingNode.name = Node.name;
                WorkingSO = new SerializedObject(WorkingNode);
            }

            UpdateStyles();
            GenerateUI();
        }

        public virtual void UpdateStyles()
        {
            contentContainer.SetBorderWidth(2).SetBorderRadius(10).SetBorderColor(Color.gray).SetPadding(0);
            contentContainer.style.backgroundColor = new StyleColor(new Color(0.25f, 0.25f, 0.25f));
        }

        public virtual void GenerateUI()
        {
            var prop = WorkingSO.GetIterator();
            bool deep = true;

            while (prop.NextVisible(deep))
            {
                deep = false;
                if (prop.name == "m_Script") continue;
                try { ProcessProperty(prop.Copy(), contentContainer, outputContainer, inputContainer); }
                catch (Exception e) { Print.MError(e); }
            }

            RefreshExpandedState();
            RefreshPorts();
        }

        private void ProcessProperty(SerializedProperty prop, VisualElement fields, VisualElement outPorts, VisualElement inPorts)
        {
            if (prop.GetTargetField() is not { } field) return;

            var inAttr = field.GetCustomAttribute<GraphInAttribute>();
            var outAttr = field.GetCustomAttribute<GraphOutAttribute>();
            var fieldAttr = field.GetCustomAttribute<GraphFieldAttribute>();

            if (inPorts != null && inAttr != null)
                inPorts.Add(PortField.Create(prop, inAttr.Name ?? prop.displayName, typeof(TNodeBase), Direction.Input));

            if (outAttr != null)
                outPorts.Add(PortField.Create(prop, outAttr.Name ?? prop.displayName, typeof(TNodeBase), Direction.Output));

            if (fieldAttr != null)
            {
                if (prop.isArray && prop.propertyType != SerializedPropertyType.String)
                    fields.Add(CreateListLayout(prop, outAttr != null));
                else
                {
                    var propField = new PropertyField(prop);
                    if (fieldAttr.Name != null) propField.label = fieldAttr.Name;
                    propField.BindProperty(prop);
                    fields.Add(propField);
                }
            }
        }

        private VisualElement CreateListLayout(SerializedProperty arrayProp, bool requiresOutPort)
        {
            var container = new VisualElement { style = { backgroundColor = new StyleColor(new Color(0, 0, 0, 0.15f)) } };
            var listContainer = new VisualElement();
            container.Add(listContainer);

            void RebuildList()
            {
                listContainer.Query<PortField>().ForEach(p => p.CleanupEdges());
                listContainer.Clear();
                for (int i = 0; i < arrayProp.arraySize; i++)
                {
                    int index = i;
                    var elementProp = arrayProp.GetArrayElementAtIndex(index);
                    var row = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center } };
                    var removeBtn = new Button(() =>
                    {
                        int size = arrayProp.arraySize;
                        arrayProp.DeleteArrayElementAtIndex(index);
                        if (arrayProp.arraySize == size) arrayProp.DeleteArrayElementAtIndex(index);
                        arrayProp.serializedObject.ApplyModifiedProperties();
                        RebuildList();
                    })
                    { text = "X", style = { width = 20, height = 20 } };

                    var fields = new VisualElement { style = { flexGrow = 1, marginLeft = 4, marginRight = 4, justifyContent = Justify.Center } };
                    var ports = new VisualElement { style = { flexDirection = FlexDirection.Column, justifyContent = Justify.Center } };

                    row.Add(removeBtn); row.Add(fields); row.Add(ports);

                    if (elementProp.propertyType == SerializedPropertyType.Generic && elementProp.hasVisibleChildren)
                    {
                        var child = elementProp.Copy();
                        var end = elementProp.GetEndProperty();
                        bool enter = true;

                        while (child.NextVisible(enter) && !SerializedProperty.EqualContents(child, end))
                        {
                            enter = false;
                            try { ProcessProperty(child.Copy(), fields, ports, null); }
                            catch (Exception e) { Print.MError(e); }
                        }
                    }
                    else
                    {
                        var propField = new PropertyField(elementProp, " ");
                        propField.BindProperty(elementProp);
                        fields.Add(propField);

                        if (requiresOutPort)
                            ports.Add(PortField.Create(elementProp, "", typeof(TNodeBase), Direction.Output));
                    }
                    listContainer.Add(row);
                }
            }

            container.Add(new Button(() =>
            {
                arrayProp.arraySize++;
                arrayProp.serializedObject.ApplyModifiedProperties();
                RebuildList();
            })
            { text = "Add Element", style = { marginTop = 4 } });

            RebuildList();
            return container;
        }

        public abstract Vector2 LoadPosition();
        public abstract void SavePosition(Vector2 position);

        public virtual void Load()
        {
            if (!Node || !WorkingNode) return;
            EditorUtility.CopySerialized(Node, WorkingNode);
            WorkingSO.Update();
        }

        public virtual void Save()
        {
            if (!Node || !WorkingNode) return;
            WorkingSO.ApplyModifiedProperties();
            EditorUtility.CopySerialized(WorkingNode, Node);
            EditorUtility.SetDirty(Node);
        }
    }
}