using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Lostbyte.Toolkit.CustomEditor.Editor.Graphs
{
    public class CustomNodeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private EditorWindow _window;
        private GraphView _graphView;

        private NodeInfo[] _nodeInfos;
        private Action<Type, string, Vector2> _onSelectCallback;

        public void Initialize(EditorWindow window, GraphView graphView, NodeInfo[] nodeInfos, Action<Type, string, Vector2> onSelectCallback) =>
            (_window, _graphView, _nodeInfos, _onSelectCallback) = (window, graphView, nodeInfos, onSelectCallback);

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> tree = new() { new SearchTreeGroupEntry(new GUIContent("Create Node"), 0) };
            Dictionary<string, SearchTreeGroupEntry> groupDict = new();
            foreach (var info in _nodeInfos)
            {
                var pathParts = info.Name.Split('/');
                SearchTreeGroupEntry currentGroup = null;
                string currentPath = "";

                for (int i = 0; i < pathParts.Length - 1; i++)
                {
                    currentPath = string.IsNullOrEmpty(currentPath) ? pathParts[i] : $"{currentPath}/{pathParts[i]}";
                    if (!groupDict.TryGetValue(currentPath, out currentGroup))
                    {
                        currentGroup = new SearchTreeGroupEntry(new GUIContent(pathParts[i]), i + 1);
                        groupDict[currentPath] = currentGroup;
                        tree.Add(currentGroup);
                    }
                }
                tree.Add(new SearchTreeEntry(new GUIContent(pathParts[^1])) { level = pathParts.Length, userData = info });
            }
            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
        {
            var worldPos = _window.rootVisualElement.ChangeCoordinatesTo(_window.rootVisualElement.parent, context.screenMousePosition - _window.position.position);
            var localPos = _graphView.contentViewContainer.WorldToLocal(worldPos);

            if (entry.userData is NodeInfo info && info.NodeType != null)
            {
                _onSelectCallback?.Invoke(info.NodeType, info.Name.Split("/")[^1], localPos);
                return true;
            }
            return false;
        }
    }
}