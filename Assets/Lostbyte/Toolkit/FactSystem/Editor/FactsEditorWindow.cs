using System;
using System.Collections.Generic;
using System.Linq;
using Lostbyte.Toolkit.Common;
using Lostbyte.Toolkit.CustomEditor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Lostbyte.Toolkit.FactSystem.Editor
{
    public class FactsEditorWindow : EditorWindow
    {
        [SerializeField] private VisualTreeAsset m_VisualTreeAsset = default;
        [SerializeField] private StyleSheet m_StyleSheet = default;
        private TreeView _treeView;
        private int _lastAssetHash = 0;
        private string _filter = "";
        private int _view = 0;
        private Vector2 _lastMousePosition;

        [MenuItem("Window/Facts/FactsEditorWindow")]
        public static void ShowFactsEditorWindow()
        {
            var wnd = GetWindow<FactsEditorWindow>();
            wnd.titleContent = new GUIContent("FactsEditorWindow");
        }

        public void CreateGUI()
        {
            if (m_VisualTreeAsset == null)
                m_VisualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.lostbyte.toolkit/FactSystem/Editor/FactsEditorWindow.uxml");
            if (m_StyleSheet == null)
                m_StyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.lostbyte.toolkit/FactSystem/Editor/FactsEditorWindow.uss");

            VisualElement root = rootVisualElement;
            m_VisualTreeAsset.CloneTree(root);
            root.styleSheets.Add(m_StyleSheet);

            _treeView = root.Q<TreeView>("tree-view");

            root.Q<Button>("add-btn").clicked += OnAddButtonClicked;

            root.Q<Button>("compile-btn").clicked += () =>
            {
                if (Application.isPlaying) Print.Warn("Cannot compile when playing!");
                else FactCodeGenerator.Generate(FactEditorUtils.Database);
            };

            root.Q<ToolbarSearchField>("search-bar").RegisterValueChangedCallback(evt =>
            {
                _filter = evt.newValue;
                SaveTreeViewState();
                BuildTreeView();
            });

            var inspectorPanel = root.Q<ScrollView>("inspector");
            _treeView.selectionChanged += (selectedItems) => UpdateInspectorPanel(inspectorPanel, selectedItems);

            var dropdown = root.Q<DropdownField>("tab-selector");
            _view = dropdown.index;
            dropdown.RegisterValueChangedCallback(evt =>
            {
                _view = dropdown.index;
                SaveTreeViewState();
                BuildTreeView();
            });

            BuildTreeView();

            EditorApplication.update += CheckForDatabaseChanges;
            EditorApplication.playModeStateChanged += OnGameStateChange;
        }

        private void OnAddButtonClicked()
        {
            switch (_view)
            {
                case 0: FactEditorUtils.ShowAddNewKeyPopup(null, "", _lastMousePosition); break;
                case 1: FactEditorUtils.ShowAddNewFactPopup(null, "", true, _lastMousePosition); break;
                case 2: FactEditorUtils.ShowAddNewEventPopup(null, "", true, _lastMousePosition); break;
                default: Print.Warn("Unknown view"); break;
            }
        }

        private void UpdateInspectorPanel(ScrollView inspectorPanel, IEnumerable<object> selectedItems)
        {
            inspectorPanel.Clear();
            var selectedItem = selectedItems.FirstOrDefault();
            if (selectedItem == null) return;

            if (selectedItem is ScriptableObject item)
            {
                var serializedObject = new SerializedObject(item);
                var iterator = serializedObject.GetIterator();
                bool enterChildren = true;

                while (iterator.NextVisible(enterChildren))
                {
                    enterChildren = false;
                    if (iterator.name == "m_Script") continue;

                    var field = new PropertyField(iterator.Copy());
                    field.Bind(serializedObject);
                    inspectorPanel.Add(field);
                }

                if (item is FactDefinition fact)
                {
                    var parentKey = GetParentKey(_treeView.selectedIndex);
                    if (parentKey != null)
                    {
                        var keySO = new SerializedObject(parentKey);
                        var regsProp = keySO.FindProperty($"<{nameof(KeyContainer.FactRegistrations)}>k__BackingField")
                                    ?? keySO.FindProperty(nameof(KeyContainer.FactRegistrations));

                        if (regsProp != null)
                        {
                            for (int i = 0; i < regsProp.arraySize; i++)
                            {
                                var regProp = regsProp.GetArrayElementAtIndex(i);
                                var factProp = regProp.FindPropertyRelative(nameof(FactRegistration.Fact));

                                if (factProp != null && factProp.objectReferenceValue == fact)
                                {
                                    inspectorPanel.Add(new VisualElement()
                                        .SetBackgroundColor(new Color(0.15f, 0.15f, 0.15f))
                                        .SetMargin(10, 0));

                                    var regField = new PropertyField(regProp);
                                    regField.BindProperty(regProp);
                                    inspectorPanel.Add(regField);
                                    break;
                                }
                            }
                        }
                    }
                }
                if (item is EventDefinition evt)
                {
                    var parentKey = GetParentKey(_treeView.selectedIndex);
                    if (parentKey != null)
                    {
                        var keySO = new SerializedObject(parentKey);
                        var regsProp = keySO.FindProperty($"<{nameof(KeyContainer.EventRegistrations)}>k__BackingField")
                                    ?? keySO.FindProperty(nameof(KeyContainer.EventRegistrations));

                        if (regsProp != null)
                        {
                            for (int i = 0; i < regsProp.arraySize; i++)
                            {
                                var regProp = regsProp.GetArrayElementAtIndex(i);
                                var eventProp = regProp.FindPropertyRelative(nameof(EventRegistration.Event));

                                if (eventProp != null && eventProp.objectReferenceValue == evt)
                                {
                                    inspectorPanel.Add(new VisualElement()
                                        .SetBackgroundColor(new Color(0.15f, 0.15f, 0.15f))
                                        .SetMargin(10, 0));

                                    var regField = new PropertyField(regProp);
                                    regField.BindProperty(regProp);
                                    inspectorPanel.Add(regField);
                                    break;
                                }
                            }
                        }
                    }
                }

            }
        }

        private void BuildTreeView()
        {
            var db = FactEditorUtils.Database;
            if (db == null || _treeView == null) return;

            EnsureGuidsExist(db.FactStorage);
            EnsureGuidsExist(FactEditorUtils.GetAllKeys());
            EnsureGuidsExist(db.EventStorage);

            var treeItems = new List<TreeViewItemData<object>>();
            int id = 0;

            switch (_view)
            {
                case 0:
                    foreach (var rootKey in db.RootKeys)
                    {
                        var item = FilterKeyView(rootKey, _filter, ref id);
                        if (item.HasValue) treeItems.Add(item.Value);
                    }
                    break;
                case 1:
                    FilterFactView(db, _filter, ref id).ForEach(treeItems.Add);
                    break;
                case 2:
                    FilterEventView(db, _filter, ref id).ForEach(treeItems.Add);
                    break;
                default:
                    Print.Warn("Unknown view");
                    break;
            }

            _treeView.makeItem = () => new VisualElement();
            _treeView.bindItem = (element, i) =>
            {
                element.Clear();
                var item = _treeView.GetItemDataForIndex<object>(i);

                if (item is ScriptableObject obj)
                {
                    element.name = obj.name;
                    VisualElement row = null;

                    if (item is KeyContainer key)
                    {
                        row = new KeyRow(key);
                        row.AddContextualMenu(evt =>
                        {
                            _lastMousePosition = Event.current.mousePosition;
                            evt.menu.AppendAction("Move Up", (e) => FactEditorUtils.MoveUp(GetParentKey(i), GetItemByIndex(i)));
                            evt.menu.AppendAction("Move Down", (e) => FactEditorUtils.MoveDown(GetParentKey(i), GetItemByIndex(i)));
                            if (!Application.isPlaying)
                            {
                                evt.menu.AppendAction("Add New Key", (e) => FactEditorUtils.ShowAddNewKeyPopup(key, "", _lastMousePosition));
                                evt.menu.AppendAction("Add New Fact", (e) => FactEditorUtils.ShowAddNewFactPopup(key, "", true, _lastMousePosition));
                            }
                            evt.menu.AppendAction("Add Existing Fact", (e) => FactEditorUtils.ShowAddExistingFactPopup(key, "", _lastMousePosition));
                            if (!Application.isPlaying)
                                evt.menu.AppendAction("Add New Event", (e) => FactEditorUtils.ShowAddNewEventPopup(key, "", true, _lastMousePosition));
                            evt.menu.AppendAction("Add Existing Event", (e) => FactEditorUtils.ShowAddExistingEventPopup(key, "", _lastMousePosition));
                            if (!Application.isPlaying)
                                evt.menu.AppendAction("Delete", (e) => FactEditorUtils.ShowDeleteKeyModal(key));
                        });
                    }
                    else if (item is FactDefinition fact)
                    {
                        row = new FactRow(fact, GetParentKey(i));
                        row.AddContextualMenu(evt =>
                        {
                            _lastMousePosition = Event.current.mousePosition;
                            if (!Application.isPlaying || _view == 1)
                            {
                                evt.menu.AppendAction("Move Up", (e) => FactEditorUtils.MoveUp(GetParentKey(i), GetItemByIndex(i)));
                                evt.menu.AppendAction("Move Down", (e) => FactEditorUtils.MoveDown(GetParentKey(i), GetItemByIndex(i)));
                            }
                            if (!Application.isPlaying)
                            {
                                if (_view == 0) evt.menu.AppendAction("Remove", (e) => FactEditorUtils.RemoveFact(GetParentKey(i), fact));
                                evt.menu.AppendAction("Delete", (e) => FactEditorUtils.ShowDeleteFactModal(fact));
                            }
                        });
                    }
                    else if (item is EventDefinition @event)
                    {
                        row = new EventRow(@event, GetParentKey(i));
                        row.AddContextualMenu(evt =>
                        {
                            _lastMousePosition = Event.current.mousePosition;
                            if (!Application.isPlaying || _view == 2)
                            {
                                evt.menu.AppendAction("Move Up", (e) => FactEditorUtils.MoveUp(GetParentKey(i), GetItemByIndex(i)));
                                evt.menu.AppendAction("Move Down", (e) => FactEditorUtils.MoveDown(GetParentKey(i), GetItemByIndex(i)));
                            }
                            if (!Application.isPlaying)
                            {
                                if (_view == 0) evt.menu.AppendAction("Remove", (e) => FactEditorUtils.RemoveEvent(GetParentKey(i), @event));
                                evt.menu.AppendAction("Delete", (e) => FactEditorUtils.ShowDeleteEventModal(@event));
                            }
                        });
                    }

                    if (row != null)
                    {
                        element.focusable = false;
                        element.pickingMode = PickingMode.Ignore;
                        element.Add(row);
                    }
                }
            };

            _treeView.SetRootItems(treeItems);
            _treeView.Rebuild();
            LoadTreeViewState();
        }

        private void EnsureGuidsExist<T>(IEnumerable<T> items) where T : Definition
        {
            foreach (var item in items)
            {
                if (string.IsNullOrWhiteSpace(item.Guid))
                {
                    item.Guid = FactEditorUtils.GenerateGuid(item.name);
                    EditorUtility.SetDirty(item);
                }
            }
        }

        private void EnsureGuidsExist(IEnumerable<KeyContainer> items)
        {
            foreach (var item in items)
            {
                if (string.IsNullOrWhiteSpace(item.Guid))
                {
                    item.Guid = FactEditorUtils.GenerateGuid(item.name);
                    EditorUtility.SetDirty(item);
                }
            }
        }

        private bool MatchesFilter(string value, Type type, string filter)
        {
            if (string.IsNullOrEmpty(filter)) return true;
            value = value.ToLowerInvariant();

            foreach (var f in filter.Split('+', StringSplitOptions.RemoveEmptyEntries))
            {
                var token = f.Trim().ToLowerInvariant();
                if (token.Length < 2 || token[1] != ':')
                {
                    if (value.Contains(token)) return true;
                    continue;
                }

                var prefix = token[0];
                var search = token[2..];

                if (value.Contains(search) &&
                    ((prefix == 'f' && type == typeof(FactDefinition)) ||
                     (prefix == 'k' && type == typeof(KeyContainer)) ||
                     (prefix == 'e' && type == typeof(EventDefinition))))
                    return true;
            }
            return false;
        }

        private ScriptableObject GetItemByIndex(int index) => _treeView.GetItemDataForIndex<object>(index) as ScriptableObject;

        private TreeViewItemData<object>? FilterKeyView(KeyContainer key, string filter, ref int id)
        {
            var children = new List<TreeViewItemData<object>>();
            bool addAll = string.IsNullOrEmpty(filter);
            bool matchesSelf = MatchesFilter(key.name, typeof(KeyContainer), filter);
            bool matchesChild = false;

            if (addAll || matchesSelf) filter = null;

            foreach (var fact in key.DefinedFacts)
            {
                if (MatchesFilter(fact.name, typeof(FactDefinition), filter))
                {
                    children.Add(new TreeViewItemData<object>(id++, fact));
                    matchesChild = true;
                }
            }

            foreach (var @event in key.DefinedEvents)
            {
                if (MatchesFilter(@event.name, typeof(EventDefinition), filter))
                {
                    children.Add(new TreeViewItemData<object>(id++, @event));
                    matchesChild = true;
                }
            }

            foreach (var childKey in key.Children)
            {
                var childItem = FilterKeyView(childKey, filter, ref id);
                if (childItem.HasValue)
                {
                    children.Add(childItem.Value);
                    matchesChild = true;
                }
            }

            return (addAll || matchesSelf || matchesChild) ? new TreeViewItemData<object>(id++, key, children) : null;
        }

        private List<TreeViewItemData<object>> FilterFactView(FactDatabase db, string filter, ref int id)
        {
            List<TreeViewItemData<object>> items = new();
            foreach (var fact in db.FactStorage)
                if (MatchesFilter(fact.name, typeof(FactDefinition), filter))
                    items.Add(new TreeViewItemData<object>(id++, fact));
            return items;
        }

        private List<TreeViewItemData<object>> FilterEventView(FactDatabase db, string filter, ref int id)
        {
            List<TreeViewItemData<object>> items = new();
            foreach (var @event in db.EventStorage)
                if (MatchesFilter(@event.name, typeof(EventDefinition), filter))
                    items.Add(new TreeViewItemData<object>(id++, @event));
            return items;
        }

        private void SaveTreeViewState()
        {
            if (_treeView != null && string.IsNullOrWhiteSpace(_filter))
            {
                var collapsed = new List<string>();
                var controller = _treeView.viewController;
                if (controller == null) return;

                bool save = false;
                foreach (var id in controller.GetAllItemIds())
                {
                    if (_treeView.GetItemDataForId<object>(id) is ScriptableObject obj && obj is KeyContainer)
                    {
                        save = true;
                        if (!controller.IsExpanded(id))
                        {
                            string guid = (obj as KeyContainer)?.Guid ?? (obj as Definition)?.Guid ?? obj.GetInstanceID().ToString();
                            collapsed.Add(guid);
                        }
                    }
                }

                if (save)
                {
                    EditorPrefs.SetString($"{nameof(FactsEditorWindow)}.TreeViewState", string.Join(",", collapsed));
                }
            }
        }

        private void LoadTreeViewState()
        {
            if (_treeView == null) return;

            if (string.IsNullOrWhiteSpace(_filter) && _view == 0)
            {
                string str = EditorPrefs.GetString($"{nameof(FactsEditorWindow)}.TreeViewState", string.Empty);
                if (string.IsNullOrWhiteSpace(str))
                {
                    _treeView.ExpandAll();
                    return;
                }

                var collapsed = str.Split(",").ToList();
                var controller = _treeView.viewController;
                if (controller == null) return;

                foreach (var id in controller.GetAllItemIds())
                {
                    var index = controller.GetIndexForId(id);
                    if (controller.GetItemForIndex(index) is ScriptableObject obj)
                    {
                        string guid = (obj as KeyContainer)?.Guid ?? (obj as Definition)?.Guid ?? obj.GetInstanceID().ToString();
                        if (collapsed.Contains(guid)) _treeView.CollapseItem(id, false);
                        else _treeView.ExpandItem(id, false);
                    }
                    else
                    {
                        _treeView.ExpandItem(id, false);
                    }
                }
            }
            else
            {
                _treeView.ExpandAll();
            }
        }

        private KeyContainer GetParentKey(int index)
        {
            if (_treeView == null) return null;
            var item = GetItemByIndex(index);

            for (int i = index - 1; i > -1; i--)
            {
                if (GetItemByIndex(i) is KeyContainer parent)
                {
                    if (item is KeyContainer key && parent.Children.Contains(key)) return parent;
                    if (item is FactDefinition fact && parent.DefinedFacts.Contains(fact)) return parent;
                    if (item is EventDefinition @event && parent.DefinedEvents.Contains(@event)) return parent;
                }
            }
            return null;
        }

        private void OnGameStateChange(PlayModeStateChange stateChange)
        {
            SaveTreeViewState();
            BuildTreeView();
        }

        private void CheckForDatabaseChanges()
        {
            var db = FactEditorUtils.Database;
            if (db == null) return;

            string path = AssetDatabase.GetAssetPath(db);
            int currentHash = AssetDatabase.GetAssetDependencyHash(path).GetHashCode();

            if (currentHash != _lastAssetHash)
            {
                _lastAssetHash = currentHash;
                SaveTreeViewState();
                BuildTreeView();
            }
        }

        private void OnDisable()
        {
            SaveTreeViewState();
            EditorApplication.update -= CheckForDatabaseChanges;
            EditorApplication.playModeStateChanged -= OnGameStateChange;
        }
    }
}